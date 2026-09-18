using System;
using System.Diagnostics;
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
        // ...tambahkan ViewModel lain di sini seiring berkembang
        services.AddSingleton<ReportRepository>();
        services.AddTransient<ReportViewModel>();
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainViewModel>(),
                Title = $"DB PATH: {AppPaths.DatabasePath}"
            };
        }
    }
}
