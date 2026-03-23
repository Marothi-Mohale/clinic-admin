namespace ClinicAdmin.Application.Abstractions;

public interface ICurrentUserService
{
    string Username { get; }
    IReadOnlyCollection<string> Roles { get; }
}

