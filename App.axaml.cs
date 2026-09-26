using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading; // Tambahkan ini jika dibutuhkan untuk UI Thread
using DocBookKeeping.Models;
using DocBookKeeping.Services;
using DocBookKeeping.ViewModels;
using DocBookKeeping.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DocBookKeeping;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        EnsureDatabaseExists(); //New

        var services = new ServiceCollection();

        // Factory, bukan instance langsung — supaya tiap operasi pakai context baru yang pendek umurnya
        services.AddDbContextFactory<DocBookKeepingContext>(options =>
            options.UseSqlite(AppPaths.ConnectionString));

        // Services & repositories
        services.AddScoped<UserRepository>();
        services.AddScoped<PemasokRepository>();
        services.AddScoped<KategoriRepository>();
        services.AddScoped<JasaRepository>();
        services.AddScoped<PasienRepository>();
        services.AddScoped<SatuanRepository>();
        services.AddScoped<BarangRepository>();
        services.AddScoped<TransJasaRepository>();
        services.AddScoped<TransBarangRepository>();
        services.AddScoped<DashboardRepository>();
        services.AddScoped<StokRepository>();
        services.AddScoped<BiayaTambahanRepository>();
        services.AddScoped<BiayaOperasionalRepository>();
        services.AddScoped<ProduksiRepository>();

        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<UserViewModel>();
        services.AddTransient<PemasokViewModel>();
        services.AddTransient<KategoriViewModel>();
        services.AddTransient<JasaViewModel>();
        services.AddTransient<PasienViewModel>();
        services.AddTransient<BarangViewModel>();
        services.AddTransient<TransJasaViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<LaporanViewModel>();
        services.AddTransient<BarangMasukViewModel>();
        services.AddTransient<BarangKeluarViewModel>();
        services.AddTransient<StokViewModel>();
        services.AddTransient<BiayaOperasionalViewModel>();
        services.AddTransient<ProduksiViewModel>();
        // ...tambahkan ViewModel lain di sini seiring berkembang
        services.AddSingleton<ReportRepository>();
        services.AddTransient<ReportViewModel>();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>(),
                //Title = "DocBooKeeping"
            };
            desktop.ShutdownRequested += OnShutdownRequested;   // <-- tambahan
        }

    }

    private bool _backupDone = false;

     private async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (_backupDone)
            return;

        e.Cancel = true;

        try
        {
            var backupService = new GDriveBackupService();
            var fileName = await backupService.BackupDatabaseAsync();
            await System.IO.File.WriteAllTextAsync(
                System.IO.Path.Combine(AppPaths.ProjectRoot, "backup_log.txt"),
                $"[{DateTime.Now}] Backup berhasil: {fileName}\n");
        }
        catch (Exception ex)
        {
            await System.IO.File.WriteAllTextAsync(
                System.IO.Path.Combine(AppPaths.ProjectRoot, "backup_log.txt"),
                $"[{DateTime.Now}] Backup GAGAL:\n{ex}\n");
        }

        _backupDone = true;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.Shutdown();
    }

    private static void EnsureDatabaseExists()
{
    if (File.Exists(AppPaths.DatabasePath))
        return;   // sudah ada database (baik dev maupun sudah pernah dipakai), tidak perlu apa-apa

    var assembly = System.Reflection.Assembly.GetExecutingAssembly();
    using var resourceStream = assembly.GetManifestResourceStream(
        "DocBookKeeping.Assets.schema.sql");

    if (resourceStream is null)
        throw new FileNotFoundException("File skema database tidak ditemukan di dalam aplikasi.");

    using var reader = new StreamReader(resourceStream);
    var schemaScript = reader.ReadToEnd();

    // Buat file database kosong baru, lalu jalankan skrip skema di atasnya
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={AppPaths.DatabasePath}");
    connection.Open();

    using var command = connection.CreateCommand();
    command.CommandText = schemaScript;
    command.ExecuteNonQuery();
}
}
