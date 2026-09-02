using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DocBookKeeping.ViewModels;

public partial class MainViewModel : ViewModelBase
{
   [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";

    [ObservableProperty]
    public partial string PageTitle { get; set; } = "Dashboard";

    [ObservableProperty]
    private ViewModelBase currentView;
    [ObservableProperty]
    private bool isMasterExpanded = true;
    private readonly UserRepository _userRepository;
    private readonly PemasokRepository _pemasokRepository;
    private readonly IServiceProvider _services;

    public MainViewModel(UserRepository userRepository,PemasokRepository pemasokRepository, IServiceProvider services)
    {
        _userRepository = userRepository;
        _pemasokRepository = pemasokRepository;
        _services = services;
        CurrentView = new DashboardViewModel();
    }

    [RelayCommand]
    private void ToggleMaster()
    {
        IsMasterExpanded = !IsMasterExpanded;
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        PageTitle = "Dashboard";
        CurrentView = new DashboardViewModel();
    }

    [RelayCommand]
    private void ShowIncome()
    {
        PageTitle = "Pemasukan";
        CurrentView = new IncomeViewModel();
    }

    [RelayCommand]
    private void ShowExpense()
    {
        PageTitle = "Pengeluaran";
        CurrentView = new ExpenseViewModel();
    }

    [RelayCommand]
    private void ShowUsers()
    {
        PageTitle = "Pengguna";
        CurrentView = _services.GetRequiredService<UserViewModel>();
    }

    [RelayCommand]
    private void ShowPemasoks()
    {
        PageTitle = "Suplier";
        CurrentView = _services.GetRequiredService<PemasokViewModel>();
    }

    [RelayCommand]
    private void ShowCategories()
    {
        PageTitle = "Kategori";
        CurrentView =  _services.GetRequiredService<KategoriViewModel>();
    }

    [RelayCommand]
    private void ShowJasas()
    {
        PageTitle = "Jasa";
        CurrentView = _services.GetRequiredService<JasaViewModel>();
    }

    [RelayCommand]
    private void ShowReports()
    {
        PageTitle = "Laporan Keuangan";
        CurrentView = _services.GetRequiredService<ReportViewModel>();
    }
}
