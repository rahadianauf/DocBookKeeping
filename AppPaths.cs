using System;
using System.IO;

namespace DocBookKeeping;

public static class AppPaths
{
    public static string ProjectRoot => ResolveRoot();

    public static string DatabasePath => Path.Combine(ProjectRoot, "Data", "DocBookKeeping.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";

    private static string ResolveRoot()
    {
        // MODE DEVELOPMENT — masih jalan dari source code
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.csproj").Length == 0)
            dir = dir.Parent;

        if (dir is not null)
            return dir.FullName;

        // MODE PUBLISHED — pakai folder data user
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var root = Path.Combine(appData, "DocBookKeeping");

        Directory.CreateDirectory(Path.Combine(root, "Data"));
        Directory.CreateDirectory(Path.Combine(root, "Config"));

        return root;
    }
}