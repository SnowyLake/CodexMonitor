using System.Text.Json;

namespace CodexMonitor.Core;

public sealed class AppSettings
{
    public string LiteMonitorDir { get; set; } = string.Empty;

    public string TrafficMonitorDir { get; set; } = string.Empty;

    public int Port { get; set; } = CodexMonitorDefaults.Port;

    public int RefreshIntervalMinutes { get; set; } = CodexMonitorDefaults.RefreshIntervalMinutes;

    public bool FirstRunCompleted { get; set; }

    public bool StartWithWindows { get; set; }

    /// <summary>
    /// Creates a normalized copy of settings values.
    /// </summary>
    public AppSettings Normalize()
    {
        if (Port <= 0 || Port > 65535)
        {
            Port = CodexMonitorDefaults.Port;
        }

        if (RefreshIntervalMinutes < CodexMonitorDefaults.MinimumRefreshIntervalMinutes ||
            RefreshIntervalMinutes > CodexMonitorDefaults.MaximumRefreshIntervalMinutes)
        {
            RefreshIntervalMinutes = CodexMonitorDefaults.RefreshIntervalMinutes;
        }

        LiteMonitorDir = LiteMonitorDir.Trim();
        TrafficMonitorDir = TrafficMonitorDir.Trim();
        return this;
    }
}

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions s_JsonOptions = new()
    {
        WriteIndented = true,
    };

    public string SettingsPath { get; }

    /// <summary>
    /// Creates a settings store next to the executable.
    /// </summary>
    public SettingsStore()
        : this(AppContext.BaseDirectory)
    {
    }

    /// <summary>
    /// Creates a settings store under the specified application directory.
    /// </summary>
    public SettingsStore(string appDirectory)
    {
        SettingsPath = Path.Combine(appDirectory, CodexMonitorDefaults.SettingsFileName);
    }

    /// <summary>
    /// Loads persisted settings or returns defaults.
    /// </summary>
    public AppSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new AppSettings().Normalize();
        }

        try
        {
            string json = File.ReadAllText(SettingsPath);
            return (JsonSerializer.Deserialize<AppSettings>(json, s_JsonOptions) ?? new AppSettings()).Normalize();
        }
        catch (JsonException)
        {
            return new AppSettings().Normalize();
        }
        catch (IOException)
        {
            return new AppSettings().Normalize();
        }
    }

    /// <summary>
    /// Saves settings next to the executable.
    /// </summary>
    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        string json = JsonSerializer.Serialize(settings.Normalize(), s_JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }
}
