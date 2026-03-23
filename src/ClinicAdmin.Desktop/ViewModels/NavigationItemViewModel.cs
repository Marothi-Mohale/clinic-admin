namespace ClinicAdmin.Desktop.ViewModels;

public sealed class NavigationItemViewModel
{
    public NavigationItemViewModel(string title, string route, string description)
    {
        Title = title;
        Route = route;
        Description = description;
    }

    public string Title { get; }
    public string Route { get; }
    public string Description { get; }
}

