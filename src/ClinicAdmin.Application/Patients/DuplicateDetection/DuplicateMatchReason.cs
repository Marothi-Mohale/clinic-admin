namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public enum DuplicateMatchReason
{
    ExactNationalId = 1,
    ExactPassportNumber = 2,
    ExactPhoneNumber = 3,
    ExactFullName = 4,
    ExactSurnameAndDateOfBirth = 5,
    SimilarFirstName = 6,
    SimilarSurname = 7,
    SimilarFullName = 8,
    MatchingDateOfBirth = 9,
    MatchingInitialAndSurname = 10
}

