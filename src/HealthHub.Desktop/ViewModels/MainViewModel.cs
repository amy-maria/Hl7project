using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Data;
using Microsoft.Extensions.Configuration;

namespace HealthHub.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly MessageRepository? _repository;

    public MainViewModel()
    {
        var config = new ConfigurationBuilder()
        .AddUserSecrets<MainViewModel>()
        .Build();

        string? connectionString = config.GetConnectionString("HealthHub");
        if (connectionString is null)
            StatusText = "Connection string 'HealthHub' is not set (user secrets).";
        else
            _repository = new MessageRepository(connectionString);
    }
    /// Rows shown in the msg list
    public ObservableCollection<MessageRecord> Messages {get; } = new();

    /// Row the user has clicked on, if any
    [ObservableProperty]
    public partial MessageRecord? SelectedMessage {get; set;}
    
    ///one-line status shown at the bottom of the window
    [ObservableProperty]
    public partial string StatusText { get; set; } = "Click Refresh to load messages.";

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_repository is null)
            return;
        
        try
        {
            StatusText = "Loading ...";
            var rows = await _repository.GetRecentAsync(50);

            Messages.Clear();
            foreach (var row in rows)
                Messages.Add(row);

            StatusText = $"{Messages.Count} messages loaded at {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusText = $"Could not load messages: {ex.Message}";
        }
    }

}