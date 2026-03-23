namespace ClinicAdmin.Application.Abstractions;

public interface IPasswordHasher
{
    PasswordHashResult HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash, string passwordSalt);
}

