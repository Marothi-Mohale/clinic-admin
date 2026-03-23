namespace ClinicAdmin.Application.Abstractions;

public sealed record PasswordHashResult(
    string Hash,
    string Salt,
    int Iterations,
    string Algorithm);

