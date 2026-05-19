using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using AnimalHaus.Shared.Core;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace AdministrationApp;

public partial class MainWindow : Window
{
    private const string SolutionFileName = "AnimalHaus.sln";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly Regex PortSegmentRegex = new("^(?<prefix>.*:)(?<port>\\d+)(?<suffix>.*)$", RegexOptions.Compiled);

    private readonly ComboBox _projectComboBox;
    private readonly Button _reloadButton;
    private readonly TextBox _projectPubPortTextBox;
    private readonly TextBox _projectCommandPortTextBox;
    private readonly ListBox _peersListBox;
    private readonly TextBlock _statusTextBlock;
    private readonly Button _saveButton;

    private readonly ObservableCollection<ProjectSettingsFile> _availableProjects = [];
    private readonly ObservableCollection<PeerEndpointRow> _peerRows = [];

    private ProjectSettingsFile? _selectedProject;
    private SystemConfiguration? _currentConfiguration;

    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        _projectComboBox = this.FindControl<ComboBox>(nameof(ProjectComboBox));
        _reloadButton = this.FindControl<Button>(nameof(ReloadButton));
        _projectPubPortTextBox = this.FindControl<TextBox>(nameof(ProjectPubPortTextBox));
        _projectCommandPortTextBox = this.FindControl<TextBox>(nameof(ProjectCommandPortTextBox));
        _peersListBox = this.FindControl<ListBox>(nameof(PeersListBox));
        _statusTextBlock = this.FindControl<TextBlock>(nameof(StatusTextBlock));
        _saveButton = this.FindControl<Button>(nameof(SaveButton));

        _projectComboBox.ItemsSource = _availableProjects;
        _peersListBox.ItemsSource = _peerRows;

        _reloadButton.Click += ReloadButtonOnClick;
        _saveButton.Click += SaveButtonOnClick;
        _projectComboBox.SelectionChanged += ProjectComboBoxOnSelectionChanged;

        LoadProjects();
    }

    private void ReloadButtonOnClick(object? sender, RoutedEventArgs e)
    {
        LoadProjects();
    }

    private void SaveButtonOnClick(object? sender, RoutedEventArgs e)
    {
        SaveCurrentProjectConfiguration();
    }

    private void ProjectComboBoxOnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        LoadSelectedProjectConfiguration();
    }

    private void LoadProjects()
    {
        _availableProjects.Clear();
        foreach (var project in DiscoverProjectSettingsFiles().OrderBy(project => project.DisplayName))
        {
            _availableProjects.Add(project);
        }

        if (_availableProjects.Count == 0)
        {
            _selectedProject = null;
            _currentConfiguration = null;
            _peerRows.Clear();
            _projectPubPortTextBox.Text = string.Empty;
            _projectCommandPortTextBox.Text = string.Empty;
            SetStatus("No project appsettings.json files found.");
            return;
        }

        _projectComboBox.SelectedIndex = 0;
    }

    private void LoadSelectedProjectConfiguration()
    {
        if (_projectComboBox.SelectedItem is not ProjectSettingsFile project)
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(project.SettingsFilePath);
            var config = JsonSerializer.Deserialize<SystemConfiguration>(json, SerializerOptions);
            if (config is null)
            {
                SetStatus($"Failed loading {project.DisplayName}: Invalid JSON configuration.");
                return;
            }

            _selectedProject = project;
            _currentConfiguration = config;
            _projectPubPortTextBox.Text = ExtractPort(config.Messaging.PubEndpoint);
            _projectCommandPortTextBox.Text = ExtractPort(config.Messaging.CommandEndpoint);

            _peerRows.Clear();
            foreach (var peer in config.Messaging.Peers.OrderBy(peer => peer.Key))
            {
                _peerRows.Add(new PeerEndpointRow
                {
                    PeerName = peer.Key,
                    PubPort = ExtractPort(peer.Value.PubEndpoint),
                    CommandPort = ExtractPort(peer.Value.CommandEndpoint)
                });
            }

            SetStatus($"Loaded {project.DisplayName}");
        }
        catch (Exception ex)
        {
            SetStatus($"Failed loading {project.DisplayName}: {ex.Message}");
        }
    }

    private void SaveCurrentProjectConfiguration()
    {
        if (_selectedProject is null || _currentConfiguration is null)
        {
            SetStatus("No project is selected.");
            return;
        }

        if (!TryGetPort(_projectPubPortTextBox.Text, out var projectPubPort))
        {
            SetStatus("Project pub port must be an integer from 1 to 65535.");
            return;
        }

        if (!TryGetPort(_projectCommandPortTextBox.Text, out var projectCommandPort))
        {
            SetStatus("Project command port must be an integer from 1 to 65535.");
            return;
        }

        foreach (var row in _peerRows)
        {
            if (!TryGetPort(row.PubPort, out _))
            {
                SetStatus($"Peer {row.PeerName} pub port must be an integer from 1 to 65535.");
                return;
            }

            if (!TryGetPort(row.CommandPort, out _))
            {
                SetStatus($"Peer {row.PeerName} command port must be an integer from 1 to 65535.");
                return;
            }
        }

        _currentConfiguration.Messaging.PubEndpoint = ReplacePort(_currentConfiguration.Messaging.PubEndpoint, projectPubPort);
        _currentConfiguration.Messaging.CommandEndpoint = ReplacePort(_currentConfiguration.Messaging.CommandEndpoint, projectCommandPort);

        foreach (var row in _peerRows)
        {
            if (!_currentConfiguration.Messaging.Peers.TryGetValue(row.PeerName, out var peerConfig))
            {
                continue;
            }

            if (TryGetPort(row.PubPort, out var peerPubPort))
            {
                peerConfig.PubEndpoint = ReplacePort(peerConfig.PubEndpoint, peerPubPort);
            }

            if (TryGetPort(row.CommandPort, out var peerCommandPort))
            {
                peerConfig.CommandEndpoint = ReplacePort(peerConfig.CommandEndpoint, peerCommandPort);
            }
        }

        try
        {
            var json = JsonSerializer.Serialize(_currentConfiguration, SerializerOptions);
            File.WriteAllText(_selectedProject.SettingsFilePath, json);
            SetStatus($"Saved {_selectedProject.DisplayName}");
        }
        catch (Exception ex)
        {
            SetStatus($"Save failed: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        _statusTextBlock.Text = message;
    }

    private static IEnumerable<ProjectSettingsFile> DiscoverProjectSettingsFiles()
    {
        var repositoryRoot = ResolveRepositoryRoot(AppContext.BaseDirectory);
        if (repositoryRoot is null)
        {
            return [];
        }

        var srcDirectory = Path.Combine(repositoryRoot, "src");
        if (!Directory.Exists(srcDirectory))
        {
            return [];
        }

        var projectFiles = Directory.EnumerateFiles(srcDirectory, "*.csproj", SearchOption.AllDirectories);
        var results = new List<ProjectSettingsFile>();

        foreach (var projectFile in projectFiles)
        {
            var projectDirectory = Path.GetDirectoryName(projectFile);
            if (string.IsNullOrWhiteSpace(projectDirectory))
            {
                continue;
            }

            var settingsFilePath = Path.Combine(projectDirectory, "appsettings.json");
            if (!File.Exists(settingsFilePath))
            {
                continue;
            }

            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            results.Add(new ProjectSettingsFile(projectName, settingsFilePath));
        }

        return results;
    }

    private static string? ResolveRepositoryRoot(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current is not null)
        {
            var solutionPath = Path.Combine(current.FullName, SolutionFileName);
            if (File.Exists(solutionPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return null;
    }

    private static string ExtractPort(string endpoint)
    {
        var match = PortSegmentRegex.Match(endpoint);
        return match.Success ? match.Groups["port"].Value : string.Empty;
    }

    private static bool TryGetPort(string? input, out int port)
    {
        var isParsed = int.TryParse(input, out port);
        return isParsed && port is > 0 and <= 65535;
    }

    private static string ReplacePort(string endpoint, int port)
    {
        var match = PortSegmentRegex.Match(endpoint);
        if (!match.Success)
        {
            return endpoint;
        }

        return $"{match.Groups["prefix"].Value}{port}{match.Groups["suffix"].Value}";
    }
}

public sealed record ProjectSettingsFile(string DisplayName, string SettingsFilePath)
{
    public override string ToString() => DisplayName;
}

public sealed class PeerEndpointRow
{
    public string PeerName { get; set; } = string.Empty;

    public string PubPort { get; set; } = string.Empty;

    public string CommandPort { get; set; } = string.Empty;
}
