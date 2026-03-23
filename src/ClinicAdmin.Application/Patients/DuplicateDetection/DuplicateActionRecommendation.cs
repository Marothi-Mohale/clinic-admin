namespace ClinicAdmin.Application.Patients.DuplicateDetection;

public enum DuplicateActionRecommendation
{
    SafeToCreate = 0,
    ShowWarning = 1,
    RequireManualReview = 2,
    BlockCreation = 3
}

