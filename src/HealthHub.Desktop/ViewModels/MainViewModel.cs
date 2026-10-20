using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HealthHub.Core.Hl7;
using HealthHub.Data;
using HealthHub.Transforms;
using Microsoft.Extensions.Configuration;

namespace HealthHub.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly MessageRepository? _repository;
    private readonly PatientFhirTransformer _transformer;

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
        //initialize the transformer
        _transformer = new PatientFhirTransformer();
    }
    // Rows shown in the msg list
    public ObservableCollection<MessageRecord> Messages { get; } = new();

  
    [ObservableProperty]
    public partial MessageRecord? SelectedMessage { get; set;}
    /// <summary>The selected message, broken down into segments, fields, and components.///</summary>
    public ObservableCollection<MessageTreeNode> TreeNodes { get; } = new();

    /// <summary>
    /// This holds the converted FHIR JSON string.
    /// </summary>
    [ObservableProperty]
    public partial string FhirJson { get; set; } = "Select a message to see the FHIR resource ...";

    ///<summary>
    /// The status bar text.
    /// </summary>
    [ObservableProperty]
    public partial string StatusText { get; set; } = "Click Refresh to load messages.";

    ///<summary>
    /// This method runs automatically every time SelectedMessage changes.
    /// </summary>
    partial void OnSelectedMessageChanged(MessageRecord? value)
    {
        TreeNodes.Clear();
        FhirJson = "Select a message to see the FHIR resource ....";//reset JSON view

        if (value is null)
            return;

        try
        {
            //build the tree view(HL7 segments)
            foreach (var node in MessageTreeBuilder.Build(value.RawMessage))
                TreeNodes.Add(node);
            // build the FHIR JSON, parse the RawMessage string into Hl7Message obj before passing to transformer
            var parsedMessage = Hl7Message.Parse(value.RawMessage);
            FhirJson = _transformer.Transform(parsedMessage);
        }
        catch (Exception ex)
        {
            StatusText = $"Could not parse or transform message: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (_repository is null) return;

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

   