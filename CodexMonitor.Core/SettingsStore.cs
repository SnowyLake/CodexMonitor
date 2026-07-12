using System.Text.Json;

namespace CodexMonitor.Core;

[Flags]
public enum TokenCostItem
{
    None = 0,
    Today = 1 << 0,
    Yesterday = 1 << 1,
    Week = 1 << 2,
    Month = 1 << 3,
    SevenDay = 1 << 4,
    ThirtyDay = 1 << 5,
    All = Today | Yesterday | Week | Month | SevenDay | ThirtyDay,
}

public sealed class AppSettings
{
    public const string ThemeModeSystem = "System";

    public const string ThemeModeLight = "Light";

    public const string ThemeModeDark = "Dark";

    public const string TokenUnitEnglish = "English unit";

    public const string TokenUnitChinese = "Chinese unit";

    public string LiteMonitorDir { get; set; } = string.Empty;

    public string TrafficMonitorDir { get; set; } = string.Empty;

    public int Port { get; set; } = CodexMonitorDefaults.Port;

    public int RefreshIntervalMinutes { get; set; } = CodexMonitorDefaults.RefreshIntervalMinutes;

    public bool StartWithWindows { get; set; }

    public string ThemeMode { get; set; } = ThemeModeSystem;

    public string TokenUnit { get; set; } = TokenUnitEnglish;

    public TokenCostItem TokenCostItems { get; set; } = TokenCostItem.All;

    public bool AcrylicEnabled { get; set; } = CodexMonitorDefaults.AcrylicEnabled;

    public int AcrylicOpacityPercent { get; set; } = CodexMonitorDefaults.AcrylicOpacityPercent;

    public bool ShowResetTimeInPlugins { get; set; } = CodexMonitorDefaults.ShowResetTimeInPlugins;

    public bool UseAbsoluteResetTime { get; set; } = CodexMonitorDefaults.UseAbsoluteResetTime;

    /// <summary>
    /// Creates a normalized copy of settings values.
    /// </summary>
    public AppSettings Normalize()
    {
        if (Port < CodexMonitorDefaults.MinimumPort || Port > CodexMonitorDefaults.MaximumPort)
        {
            Port = CodexMonitorDefaults.Port;
        }

        if (RefreshIntervalMinutes < CodexMonitorDefaults.MinimumRefreshIntervalMinutes ||
            RefreshIntervalMinutes > CodexMonitorDefaults.MaximumRefreshIntervalMinutes)
        {
            RefreshIntervalMinutes = CodexMonitorDefaults.RefreshIntervalMinutes;
        }

        if (AcrylicOpacityPercent < CodexMonitorDefaults.MinimumAcrylicOpacityPercent ||
            AcrylicOpacityPercent > CodexMonitorDefaults.MaximumAcrylicOpacityPercent)
        {
            AcrylicOpacityPercent = CodexMonitorDefaults.AcrylicOpacityPercent;
        }

        LiteMonitorDir = LiteMonitorDir.Trim();
        TrafficMonitorDir = TrafficMonitorDir.Trim();
        ThemeMode = NormalizeThemeMode(ThemeMode);
        TokenUnit = NormalizeTokenUnit(TokenUnit);
        TokenCostItems &= TokenCostItem.All;
        return this;
    }

    /// <summary>
    /// Normalizes a theme mode string to a supported value.
    /// </summary>
    private static string NormalizeThemeMode(string? themeMode)
    {
        return themeMode?.Trim().ToLowerInvariant() switch
        {
            "light" => ThemeModeLight,
            "dark" => ThemeModeDark,
            _ => ThemeModeSystem,
        };
    }

    /// <summary>
    /// Normalizes a token unit string to a supported value.
    /// </summary>
    private static string NormalizeTokenUnit(string? tokenUnit)
    {
        string normalized = tokenUnit?.Trim() ?? string.Empty;
        return string.Equals(normalized, TokenUnitChinese, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalized, "万/亿", StringComparison.Ordinal)
            || string.Equals(normalized, "K/W/E", StringComparison.OrdinalIgnoreCase)
            ? TokenUnitChinese
            : TokenUnitEnglish;
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
    /// Returns true when the settings file already exists.
    /// </summary>
    public bool Exists()
    {
        return File.Exists(SettingsPath);
    }

    /// <summary>
    /// Loads persisted settings, repairs missing fields, or returns defaults.
    /// </summary>
    public AppSettings Load()
    {
        if (!Exists())
        {
            return new AppSettings().Normalize();
        }

        try
        {
            string json = File.ReadAllText(SettingsPath);
            AppSettings settings = (JsonSerializer.Deserialize<AppSettings>(json, s_JsonOptions) ?? new AppSettings()).Normalize();
            Save(settings);
            return settings;
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
