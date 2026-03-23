namespace ClinicAdmin.Application.Common.Validation;

public sealed record ValidationError(string PropertyName, string ErrorMessage);
