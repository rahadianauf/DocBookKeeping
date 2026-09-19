using System;
using System.IO;

namespace DocBookKeeping;

public static class AppPaths
{
    public static string ProjectRoot => FindProjectRoot();

    public static string DatabasePath => Path.Combine(ProjectRoot, "Data", "DocBookKeeping.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        // Naik dari bin/Debug/net10.0/ sampai ketemu folder yang berisi file .csproj
        while (dir is not null && dir.GetFiles("*.csproj").Length == 0)
        {
            dir = dir.Parent;
        }

        if (dir is null)
            throw new InvalidOperationException(
                "Tidak menemukan folder project (.csproj). " +
                "Pastikan struktur folder project tidak dipindah/rusak.");

        return dir.FullName;
    }
}