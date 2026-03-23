namespace ClinicAdmin.Desktop.ViewModels;

public sealed class WorkspacePlaceholderViewModel : ViewModelBase
{
    public WorkspacePlaceholderViewModel(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }
    public string Description { get; }
}

