namespace ClinicAdmin.Application.Authentication;

public sealed record RegisterAccountRequest(
    string FileNumber,
    string Username,
    string Password,
    string IdNumber,
    string Email,
    string ConfirmedIdNumber,
    string ConfirmedEmail);
