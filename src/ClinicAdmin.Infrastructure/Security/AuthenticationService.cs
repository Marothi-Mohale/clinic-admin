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
    private readonly ValidatorExecutor<LoginRequest> _validatorExecutor;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IUserSessionService sessionService,
        IAuditService auditService,
        IClock clock,
        IFacilityContext facilityContext,
        ValidatorExecutor<LoginRequest> validatorExecutor,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _sessionService = sessionService;
        _auditService = auditService;
        _clock = clock;
        _facilityContext = facilityContext;
        _validatorExecutor = validatorExecutor;
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
            await _auditService.WriteAuthenticationAsync(username, false, message, cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.ValidationFailed, message);
        }

        var normalizedUsername = username.Trim().ToUpperInvariant();

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(
                x => x.FacilityId == _facilityContext.CurrentFacilityId && x.Username == normalizedUsername,
                cancellationToken);

        if (user is null)
        {
            await _auditService.WriteAuthenticationAsync(username, false, "Unknown username.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Invalid username or password.");
        }

        if (!user.IsActive)
        {
            await _auditService.WriteAuthenticationAsync(username, false, "Inactive user account.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.UserInactive, "Your account is inactive. Please contact an administrator.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
        {
            await _auditService.WriteAuthenticationAsync(username, false, "Invalid password.", cancellationToken);
            return AuthenticationResult.Failure(AuthenticationErrorCode.InvalidCredentials, "Invalid username or password.");
        }

        var loginAtUtc = _clock.UtcNow;
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

    public Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (_sessionService.CurrentSession is { } session)
        {
            _logger.LogInformation("User {Username} logged out.", session.Username);
        }

        _sessionService.ClearSession();
        return Task.CompletedTask;
    }
}
