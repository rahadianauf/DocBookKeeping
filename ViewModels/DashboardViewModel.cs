namespace DocBookKeeping.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Models;
using DocBookKeeping.Services;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DashboardRepository _dashboardRepository;

    public ObservableCollection<TransJasa> RecentTransJasa { get; } = new();
    public ObservableCollection<TransBarang> RecentTransBarang { get; } = new();

    [ObservableProperty]
    private decimal totalPemasukanBulanIni;

    [ObservableProperty]
    private decimal totalPengeluaranBulanIni;

    [ObservableProperty]
    private decimal saldoBulanIni;

    [ObservableProperty]
    private int jumlahPasien;

    [ObservableProperty]
    private int jumlahBarang;

    [ObservableProperty]
    private int jumlahTransaksiJasaBulanIni;

    [ObservableProperty]
    private int jumlahTransaksiBarangBulanIni;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public string BulanIniLabel => DateTime.Now.ToString("MMMM yyyy");

    public DashboardViewModel(DashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
        LoadDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDashboard()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var summary = await _dashboardRepository.GetSummaryAsync();
            TotalPemasukanBulanIni = summary.TotalPemasukanBulanIni;
            TotalPengeluaranBulanIni = summary.TotalPengeluaranBulanIni;
            SaldoBulanIni = summary.Saldo;
            JumlahPasien = summary.JumlahPasien;
            JumlahBarang = summary.JumlahBarang;
            JumlahTransaksiJasaBulanIni = summary.JumlahTransaksiJasaBulanIni;
            JumlahTransaksiBarangBulanIni = summary.JumlahTransaksiBarangBulanIni;

            var recentJasa = await _dashboardRepository.GetRecentTransJasaAsync();
            RecentTransJasa.Clear();
            foreach (var t in recentJasa) RecentTransJasa.Add(t);

            var recentBarang = await _dashboardRepository.GetRecentTransBarangAsync();
            RecentTransBarang.Clear();
            foreach (var t in recentBarang) RecentTransBarang.Add(t);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat dashboard.";
            Debug.WriteLine($"[DashboardViewModel] LoadDashboard error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}