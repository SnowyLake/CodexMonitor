namespace CodexMonitor.Core;

public static class TrafficMonitorPluginInstaller
{
    private const string k_FallbackPluginConfig = """
[CodexMonitor]
UsageUrl=http://127.0.0.1:17890/codex-monitor
""";

    /// <summary>
    /// Installs the TrafficMonitor plugin DLL and local configuration into the selected directory.
    /// </summary>
    public static string Install(string trafficMonitorDirectory, int port, string? pluginBinaryPath = null)
    {
        if (!TrafficMonitorLocator.IsTrafficMonitorDirectory(trafficMonitorDirectory))
        {
            throw new DirectoryNotFoundException($"TrafficMonitor.exe not found in {trafficMonitorDirectory}");
        }

        string sourcePath = ResolvePluginBinaryPath(pluginBinaryPath);
        string pluginDirectory = Path.Combine(trafficMonitorDirectory, "plugins");
        Directory.CreateDirectory(pluginDirectory);

        string targetPath = Path.Combine(pluginDirectory, CodexMonitorDefaults.TrafficMonitorPluginFileName);
        File.Copy(sourcePath, targetPath, true);

        string configPath = Path.Combine(pluginDirectory, CodexMonitorDefaults.TrafficMonitorPluginConfigFileName);
        File.WriteAllText(configPath, BuildPluginConfig(port));
        return targetPath;
    }

    /// <summary>
    /// Builds the TrafficMonitor plugin configuration content.
    /// </summary>
    private static string BuildPluginConfig(int port)
    {
        int normalizedPort = port is > 0 and <= 65535 ? port : CodexMonitorDefaults.Port;
        string bridgeUrl = $"http://127.0.0.1:{normalizedPort}{CodexMonitorDefaults.UsageEndpointPath}";
        return SetUsageUrl(ReadTemplateConfig(), bridgeUrl);
    }

    /// <summary>
    /// Sets the backend URL in the TrafficMonitor plugin configuration content.
    /// </summary>
    private static string SetUsageUrl(string content, string bridgeUrl)
    {
        string normalizedContent = content.Replace("\r\n", "\n", StringComparison.Ordinal);
        string[] lines = normalizedContent.TrimEnd('\n').Split('\n');
        bool updated = false;
        for (int index = 0; index < lines.Length; index++)
        {
            if (lines[index].StartsWith("UsageUrl=", StringComparison.Ordinal))
            {
                lines[index] = $"UsageUrl={bridgeUrl}";
                updated = true;
            }
        }

        List<string> outputLines = [.. lines];
        if (!updated)
        {
            outputLines.Add($"UsageUrl={bridgeUrl}");
        }

        return string.Join(Environment.NewLine, outputLines) + Environment.NewLine;
    }

    /// <summary>
    /// Reads the packaged TrafficMonitor configuration template when available.
    /// </summary>
    private static string ReadTemplateConfig()
    {
        string templatePath = Path.Combine(AppContext.BaseDirectory, "Plugins", "TrafficMonitor", CodexMonitorDefaults.TrafficMonitorPluginConfigFileName);
        return File.Exists(templatePath) ? File.ReadAllText(templatePath) : k_FallbackPluginConfig;
    }

    /// <summary>
    /// Resolves the compiled TrafficMonitor plugin DLL path.
    /// </summary>
    private static string ResolvePluginBinaryPath(string? pluginBinaryPath)
    {
        if (!string.IsNullOrWhiteSpace(pluginBinaryPath))
        {
            return EnsurePluginBinaryExists(pluginBinaryPath);
        }

        foreach (string candidate in EnumeratePluginBinaryCandidates())
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new FileNotFoundException($"TrafficMonitor plugin DLL was not found. Run Scripts\\Build-TrafficMonitorPlugin.ps1 first, then install {CodexMonitorDefaults.TrafficMonitorPluginFileName}.");
    }

    /// <summary>
    /// Returns a plugin binary path when it exists.
    /// </summary>
    private static string EnsurePluginBinaryExists(string pluginBinaryPath)
    {
        if (!File.Exists(pluginBinaryPath))
        {
            throw new FileNotFoundException($"TrafficMonitor plugin DLL was not found: {pluginBinaryPath}");
        }

        return pluginBinaryPath;
    }

    /// <summary>
    /// Enumerates likely locations for the compiled TrafficMonitor plugin DLL.
    /// </summary>
    private static IEnumerable<string> EnumeratePluginBinaryCandidates()
    {
        string baseDirectory = AppContext.BaseDirectory;
        yield return Path.Combine(baseDirectory, "Plugins", "TrafficMonitor", CodexMonitorDefaults.TrafficMonitorPluginFileName);
        yield return Path.Combine(baseDirectory, CodexMonitorDefaults.TrafficMonitorPluginFileName);

        string currentDirectory = Directory.GetCurrentDirectory();
        yield return Path.Combine(currentDirectory, "Plugins", "TrafficMonitor", CodexMonitorDefaults.TrafficMonitorPluginFileName);
        yield return Path.Combine(currentDirectory, "Plugins", "TrafficMonitor", "Builds", "x64", "Release", CodexMonitorDefaults.TrafficMonitorPluginFileName);
        yield return Path.Combine(currentDirectory, "Plugins", "TrafficMonitor", "Builds", "x64", "Debug", CodexMonitorDefaults.TrafficMonitorPluginFileName);
    }
}
