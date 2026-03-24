using ClinicAdmin.Domain.Common;

namespace ClinicAdmin.Domain.Security;

public sealed class AppUser : Entity
{
    public string Username { get; private set; }
    public string DisplayName { get; private set; }
    public string? FileNumber { get; private set; }
    public string? IdNumber { get; private set; }
    public string? Email { get; private set; }
    public bool IsIdentityConfirmed { get; private set; }
    public string PasswordHash { get; private set; }
    public string PasswordSalt { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public Guid FacilityId { get; private set; }
    public DateTimeOffset? LastLoginAtUtc { get; private set; }

    public AppUser(
        Guid facilityId,
        string username,
        string displayName,
        string passwordHash,
        string passwordSalt,
        UserRole role,
        string? fileNumber = null,
        string? idNumber = null,
        string? email = null,
        bool isIdentityConfirmed = true,
        bool isActive = true)
    {
        FacilityId = facilityId == Guid.Empty ? throw new ArgumentException("Facility is required.", nameof(facilityId)) : facilityId;
        Username = GuardRequired(username, nameof(username)).ToUpperInvariant();
        DisplayName = GuardRequired(displayName, nameof(displayName));
        FileNumber = GuardOptional(fileNumber);
        IdNumber = GuardOptional(idNumber);
        Email = GuardOptional(email);
        IsIdentityConfirmed = isIdentityConfirmed;
        PasswordHash = GuardRequired(passwordHash, nameof(passwordHash));
        PasswordSalt = GuardRequired(passwordSalt, nameof(passwordSalt));
        Role = role;
        IsActive = isActive;
    }

    public void RecordSuccessfulLogin(DateTimeOffset loggedInAtUtc)
    {
        LastLoginAtUtc = loggedInAtUtc;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void UpdateCredentials(string passwordHash, string passwordSalt)
    {
        PasswordHash = GuardRequired(passwordHash, nameof(passwordHash));
        PasswordSalt = GuardRequired(passwordSalt, nameof(passwordSalt));
    }

    private static string GuardRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldName} is required.", fieldName);
        }

        return value.Trim();
    }

    private static string? GuardOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}

