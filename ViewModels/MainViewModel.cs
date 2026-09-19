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
        CurrentView = _services.GetRequiredService<DashboardViewModel>();
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
        CurrentView = _services.GetRequiredService<DashboardViewModel>();
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
   private void ShowBarangMasuk()
   {
       PageTitle = "Pengeluaran (Barang Masuk)";
       CurrentView = _services.GetRequiredService<BarangMasukViewModel>();
   }
   
   [RelayCommand]
   private void ShowBarangKeluar()
   {
       PageTitle = "Barang Keluar";
       CurrentView = _services.GetRequiredService<BarangKeluarViewModel>();
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
    private void ShowPasiens()
    {
        PageTitle = "Pasien";
        CurrentView = _services.GetRequiredService<PasienViewModel>();
    }
    [RelayCommand]
    private void ShowBarangs()
    {
        PageTitle = "Barang";
        CurrentView = _services.GetRequiredService<BarangViewModel>();
    }
    [RelayCommand]
    private void ShowTransJasas()
    {
        PageTitle = "Pemasukan (Transaksi Jasa)";
        CurrentView = _services.GetRequiredService<TransJasaViewModel>();
    }
    [RelayCommand]
    private void ShowReports()
    {
        PageTitle = "Laporan Keuangan";
        CurrentView = _services.GetRequiredService<LaporanViewModel>();
    }

    [RelayCommand]
    private void ShowStok()
    {
        PageTitle = "Stok Barang";
        CurrentView = _services.GetRequiredService<StokViewModel>();
    }

    [RelayCommand]
    private void ShowBiayaOperasional()
    {
        PageTitle = "Biaya Operasional";
        CurrentView = _services.GetRequiredService<BiayaOperasionalViewModel>();
    }
}
