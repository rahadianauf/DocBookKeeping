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
#if DEBUG
        // MODE DEBUG — selalu development, cari .csproj
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && dir.GetFiles("*.csproj").Length == 0)
            dir = dir.Parent;

        if (dir is not null)
            return dir.FullName;

        throw new InvalidOperationException(
            "Mode Debug tapi .csproj tidak ditemukan — struktur project mungkin rusak.");
#else
        // MODE RELEASE — selalu pakai folder AppData, tidak peduli lokasi .exe
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var root = Path.Combine(appData, "DocBookKeeping");

        Directory.CreateDirectory(Path.Combine(root, "Data"));
        Directory.CreateDirectory(Path.Combine(root, "Config"));

        return root;
#endif
    }
}