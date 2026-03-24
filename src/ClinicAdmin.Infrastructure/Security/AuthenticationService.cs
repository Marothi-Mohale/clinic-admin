using ClinicAdmin.Application.Abstractions;
using ClinicAdmin.Application.Authentication;
using ClinicAdmin.Application.Common.Exceptions;
using ClinicAdmin.Application.Common.Validation;
using ClinicAdmin.Domain.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicAdmin.Infrastructure.Security;

public sealed class AuthenticationService : IAuthenticationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserSessionService _sessionService;
    private readonly IAuditService _auditService;
    private readonly IClock _clock;
    private readonly IFacilityContext _facilityContext;
    private readonly ILoginAttemptLimiter _loginAttemptLimiter;
    private readonly ValidatorExecutor<LoginRequest> _validatorExecutor;
    private readonly ValidatorExecutor<RegisterAccountRequest> _registerValidatorExecutor;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IUserSessionService sessionService,
        IAuditService auditService,
        IClock clock,
        IFacilityContext facilityContext,
        ILoginAttemptLimiter loginAttemptLimiter,
        ValidatorExecutor<LoginRequest> validatorExecutor,
        ValidatorExecutor<RegisterAccountRequest> registerValidatorExecutor,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _sessionService = sessionService;
        _auditService = auditService;
        _clock = clock;
        _facilityContext = facilityContext;
        _loginAttemptLimiter = loginAttemptLimiter;
        _validatorExecutor = validatorExecutor;
        _registerValidatorExecutor = registerValidatorExecutor;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var request = new LoginRequest(username, password);

        try
        {
            await _validatorExecutor.ValidateAndThrowAsync(request, cancellationToken);
        }
        catch (ValidationException validationException)
        {
            var message = validationException.Errors.First().ErrorMessage;
            await _auditService.WriteAuthenticationAsync(username, false, "Login validation failed.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.ValidationFailed, message);
        }

        var normalizedUsername = username.Trim().ToUpperInvariant();
        var now = _clock.UtcNow;

        if (_loginAttemptLimiter.IsLockedOut(normalizedUsername, now, out _))
        {
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Login blocked due to temporary lockout.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.LockedOut, "Sign-in is temporarily locked. Please wait a few minutes and try again.");
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                x => x.FacilityId == _facilityContext.CurrentFacilityId && x.Username == normalizedUsername,
                cancellationToken);

        if (user is null)
        {
            _loginAttemptLimiter.RecordFailure(normalizedUsername, now);
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Login failed.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Invalid username or password.");
        }

        if (!user.IsActive)
        {
            _loginAttemptLimiter.RecordFailure(normalizedUsername, now);
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Login failed.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Invalid username or password.");
        }

        if (!user.IsIdentityConfirmed)
        {
            _loginAttemptLimiter.RecordFailure(normalizedUsername, now);
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Login blocked. Identity is not confirmed.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.IdentityNotConfirmed, "ID number and email confirmation is required before sign-in.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            _loginAttemptLimiter.RecordFailure(normalizedUsername, now);
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Login failed.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Invalid username or password.");
        }

        _loginAttemptLimiter.ClearFailures(normalizedUsername);
        var loginAtUtc = now;
        user.RecordSuccessfulLogin(loginAtUtc);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var session = new UserSession(
            user.Id,
            user.Username,
            user.DisplayName,
            user.Role,
            user.FacilityId,
            loginAtUtc);

        _sessionService.SetSession(session);
        await _auditService.WriteAuthenticationAsync(user.Username, true, "Login succeeded.", cancellationToken);
        _logger.LogInformation("User {Username} logged into facility {FacilityId} as {Role}", user.Username, user.FacilityId, user.Role);

        return AuthenticationResult.Success(session);
    }

    public async Task<AuthenticationResult> RegisterAccountAsync(
        string fileNumber,
        string username,
        string password,
        string idNumber,
        string email,
        string confirmedIdNumber,
        string confirmedEmail,
        CancellationToken cancellationToken = default)
    {
        var request = new RegisterAccountRequest(fileNumber, username, password, idNumber, email, confirmedIdNumber, confirmedEmail);

        try
        {
            await _registerValidatorExecutor.ValidateAndThrowAsync(request, cancellationToken);
        }
        catch (ValidationException validationException)
        {
            var message = validationException.Errors.First().ErrorMessage;
            await _auditService.WriteAuthenticationAsync(username, false, "Account registration validation failed.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.ValidationFailed, message);
        }

        var normalizedUsername = username.Trim().ToUpperInvariant();
        var normalizedFileNumber = fileNumber.Trim().ToUpperInvariant();

        var existingUsername = await _dbContext.Users
            .AnyAsync(x => x.FacilityId == _facilityContext.CurrentFacilityId && x.Username == normalizedUsername, cancellationToken);

        if (existingUsername)
        {
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Account registration failed. Username already exists.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.UsernameTaken, "Username is already registered.");
        }

        var existingFileNumber = await _dbContext.Users
            .AnyAsync(
                x => x.FacilityId == _facilityContext.CurrentFacilityId &&
                     x.FileNumber != null &&
                     x.FileNumber.ToUpper() == normalizedFileNumber,
                cancellationToken);

        if (existingFileNumber)
        {
            await _auditService.WriteAuthenticationAsync(normalizedUsername, false, "Account registration failed. File number already exists.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.ValidationFailed, "File number is already linked to another account.");
        }

        var passwordHash = _passwordHasher.HashPassword(password);
        var user = new AppUser(
            _facilityContext.CurrentFacilityId,
            username,
            username.Trim(),
            passwordHash.Hash,
            passwordHash.Salt,
            UserRole.Receptionist,
            fileNumber,
            idNumber,
            email,
            isIdentityConfirmed: true);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAuthenticationAsync(user.Username, true, "Account registration succeeded.", cancellationToken);
        _logger.LogInformation(
            "User account {Username} registered for facility {FacilityId} with file number {FileNumber}",
            user.Username,
            user.FacilityId,
            user.FileNumber);

        return new AuthenticationResult(true, AuthenticationErrorCode.None, "Account created successfully. You can now sign in.");
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionService.CurrentSession is { } session)
        {
            _logger.LogInformation("User {Username} logged out.", session.Username);
            await _auditService.WriteChangeAsync(
                "Logout",
                nameof(AppUser),
                session.UserId,
                $"User {session.Username} logged out.",
                metadata: "{\"category\":\"authentication\"}",
                cancellationToken: cancellationToken);
        }

        _sessionService.ClearSession();
    }
}
