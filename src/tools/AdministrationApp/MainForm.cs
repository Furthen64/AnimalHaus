using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using AnimalHaus.Shared.Core;

namespace AdministrationApp;

public sealed class MainForm : Form
{
    private const string SolutionFileName = "AnimalHaus.sln";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static readonly Regex PortSegmentRegex = new("^(?<prefix>.*:)(?<port>\\d+)(?<suffix>.*)$", RegexOptions.Compiled);

    private readonly ComboBox _projectCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 360 };
    private readonly Button _reloadButton = new() { Text = "Reload Projects", AutoSize = true };
    private readonly TextBox _projectPubPortText = new() { Width = 140 };
    private readonly TextBox _projectCommandPortText = new() { Width = 140 };
    private readonly DataGridView _peersGrid = new() { Dock = DockStyle.Fill, AllowUserToAddRows = false, AutoGenerateColumns = false };
    private readonly Label _statusLabel = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Button _saveButton = new() { Text = "Save Settings", AutoSize = true };

    private List<ProjectSettingsFile> _availableProjects = [];
    private ProjectSettingsFile? _selectedProject;
    private SystemConfiguration? _currentConfiguration;
    private BindingList<PeerEndpointRow> _peerRows = [];

    public MainForm()
    {
        Text = "AdministrationApp - Endpoint Settings";
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        BindEvents();
        LoadProjects();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var projectRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = false
        };
        projectRow.Controls.Add(new Label { Text = "Project:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        projectRow.Controls.Add(_projectCombo);
        projectRow.Controls.Add(_reloadButton);
        root.Controls.Add(projectRow, 0, 0);

        var localEndpoints = new GroupBox
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Text = "Project Endpoint Ports",
            Padding = new Padding(12)
        };
        var localLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = true
        };
        localLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        localLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        localLayout.Controls.Add(new Label { Text = "Pub Port:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 0);
        localLayout.Controls.Add(_projectPubPortText, 1, 0);
        localLayout.Controls.Add(new Label { Text = "Command Port:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 1);
        localLayout.Controls.Add(_projectCommandPortText, 1, 1);
        localEndpoints.Controls.Add(localLayout);
        root.Controls.Add(localEndpoints, 0, 1);

        _peersGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Peer Project",
            DataPropertyName = nameof(PeerEndpointRow.PeerName),
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
        });
        _peersGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Peer Pub Port",
            DataPropertyName = nameof(PeerEndpointRow.PubPort),
            Width = 140
        });
        _peersGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "Peer Command Port",
            DataPropertyName = nameof(PeerEndpointRow.CommandPort),
            Width = 160
        });

        var peersGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Text = "Peer Endpoint Ports",
            Padding = new Padding(12)
        };
        peersGroup.Controls.Add(_peersGrid);
        root.Controls.Add(peersGroup, 0, 2);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = true
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footer.Controls.Add(_statusLabel, 0, 0);
        footer.Controls.Add(_saveButton, 1, 0);
        root.Controls.Add(footer, 0, 3);

        Controls.Add(root);
    }

    private void BindEvents()
    {
        _reloadButton.Click += (_, _) => LoadProjects();
        _projectCombo.SelectedIndexChanged += (_, _) => LoadSelectedProjectConfiguration();
        _saveButton.Click += (_, _) => SaveCurrentProjectConfiguration();
    }

    private void LoadProjects()
    {
        _availableProjects = DiscoverProjectSettingsFiles().OrderBy(project => project.DisplayName).ToList();
        _projectCombo.DataSource = _availableProjects;
        _projectCombo.DisplayMember = nameof(ProjectSettingsFile.DisplayName);
        _projectCombo.ValueMember = nameof(ProjectSettingsFile.SettingsFilePath);

        if (_availableProjects.Count == 0)
        {
            _selectedProject = null;
            _currentConfiguration = null;
            _peerRows = [];
            _peersGrid.DataSource = _peerRows;
            _projectPubPortText.Clear();
            _projectCommandPortText.Clear();
            _statusLabel.Text = "No project appsettings.json files found.";
            return;
        }

        _projectCombo.SelectedIndex = 0;
    }

    private void LoadSelectedProjectConfiguration()
    {
        if (_projectCombo.SelectedItem is not ProjectSettingsFile project)
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(project.SettingsFilePath);
            var config = JsonSerializer.Deserialize<SystemConfiguration>(json, SerializerOptions);
            if (config is null)
            {
                _statusLabel.Text = $"Failed loading {project.DisplayName}: Invalid JSON configuration.";
                return;
            }

            _selectedProject = project;
            _currentConfiguration = config;
            _projectPubPortText.Text = ExtractPort(config.Messaging.PubEndpoint);
            _projectCommandPortText.Text = ExtractPort(config.Messaging.CommandEndpoint);
            _peerRows = new BindingList<PeerEndpointRow>(
                config.Messaging.Peers
                    .OrderBy(peer => peer.Key)
                    .Select(peer => new PeerEndpointRow
                    {
                        PeerName = peer.Key,
                        PubPort = ExtractPort(peer.Value.PubEndpoint),
                        CommandPort = ExtractPort(peer.Value.CommandEndpoint)
                    })
                    .ToList());
            _peersGrid.DataSource = _peerRows;
            _statusLabel.Text = $"Loaded {project.DisplayName}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Failed loading {project.DisplayName}: {ex.Message}";
        }
    }

    private void SaveCurrentProjectConfiguration()
    {
        if (_selectedProject is null || _currentConfiguration is null)
        {
            _statusLabel.Text = "No project is selected.";
            return;
        }

        if (!TryGetPort(_projectPubPortText.Text, out var projectPubPort))
        {
            _statusLabel.Text = "Project pub port must be an integer from 1 to 65535.";
            return;
        }

        if (!TryGetPort(_projectCommandPortText.Text, out var projectCommandPort))
        {
            _statusLabel.Text = "Project command port must be an integer from 1 to 65535.";
            return;
        }

        foreach (var row in _peerRows)
        {
            if (!TryGetPort(row.PubPort, out _))
            {
                _statusLabel.Text = $"Peer {row.PeerName} pub port must be an integer from 1 to 65535.";
                return;
            }

            if (!TryGetPort(row.CommandPort, out _))
            {
                _statusLabel.Text = $"Peer {row.PeerName} command port must be an integer from 1 to 65535.";
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
            _statusLabel.Text = $"Saved {_selectedProject.DisplayName}";
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"Save failed: {ex.Message}";
        }
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

    private static bool TryGetPort(string input, out int port)
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

    private sealed record ProjectSettingsFile(string DisplayName, string SettingsFilePath);

    private sealed class PeerEndpointRow
    {
        public string PeerName { get; set; } = string.Empty;

        public string PubPort { get; set; } = string.Empty;

        public string CommandPort { get; set; } = string.Empty;
    }
}
