using CommunityToolkit.Mvvm.ComponentModel;

namespace HealthHub.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "HealthHub Monitor";
}
