namespace CodexMonitor.Core;

public static class LiteMonitorLocator
{
    /// <summary>
    /// Returns true when a directory looks like a LiteMonitor installation.
    /// </summary>
    public static bool IsLiteMonitorDirectory(string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return false;
        }

        return File.Exists(Path.Combine(directory, "LiteMonitor.exe"));
    }

    /// <summary>
    /// Attempts to find a LiteMonitor installation directory.
    /// </summary>
    public static string AutoDetect(string? savedDirectory = null)
    {
        foreach (string candidate in EnumerateCandidates(savedDirectory))
        {
            if (IsLiteMonitorDirectory(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Enumerates likely LiteMonitor installation directories.
    /// </summary>
    private static IEnumerable<string> EnumerateCandidates(string? savedDirectory)
    {
        if (!string.IsNullOrWhiteSpace(savedDirectory))
        {
            yield return savedDirectory;
        }

        foreach (string directory in EnumerateSearchRoots())
        {
            foreach (string match in FindLiteMonitorExe(directory))
            {
                yield return Path.GetDirectoryName(match) ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Enumerates local drive roots that can be searched.
    /// </summary>
    private static IEnumerable<string> EnumerateSearchRoots()
    {
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            bool canSearch;
            try
            {
                canSearch = (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable) && drive.IsReady;
            }
            catch (IOException)
            {
                canSearch = false;
            }
            catch (UnauthorizedAccessException)
            {
                canSearch = false;
            }

            if (canSearch)
            {
                yield return drive.RootDirectory.FullName;
            }
        }
    }

    /// <summary>
    /// Finds LiteMonitor.exe below a root directory.
    /// </summary>
    private static IEnumerable<string> FindLiteMonitorExe(string root)
    {
        EnumerationOptions options = new()
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive,
        };

        IEnumerator<string> files;
        try
        {
            files = Directory.EnumerateFiles(root, "LiteMonitor.exe", options).Take(3).GetEnumerator();
        }
        catch (IOException)
        {
            yield break;
        }
        catch (UnauthorizedAccessException)
        {
            yield break;
        }

        using (files)
        {
            while (true)
            {
                string file;
                try
                {
                    if (!files.MoveNext())
                    {
                        yield break;
                    }

                    file = files.Current;
                }
                catch (IOException)
                {
                    yield break;
                }
                catch (UnauthorizedAccessException)
                {
                    yield break;
                }

                yield return file;
            }
        }
    }
}
