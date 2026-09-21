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

    [ObservableProperty]
    private DateTimeOffset? startDate;

    [ObservableProperty]
    private DateTimeOffset? endDate;

    public string BulanIniLabel => DateTime.Now.ToString("MMMM yyyy");

    public DashboardViewModel(DashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;

        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(now.Date);

        LoadDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDashboard()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var startStr = StartDate?.ToString("yyyy-MM-dd");
            var endStr = EndDate?.ToString("yyyy-MM-dd");

            var summary = await _dashboardRepository.GetSummaryAsync(startStr, endStr);
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

    [RelayCommand]
    private void SetRangeBulanIni()
    {
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(now.Date);
        LoadDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private void SetRangeBulanLalu()
    {
        var now = DateTime.Now;
        var bulanLalu = now.AddMonths(-1);
        var awalBulanLalu = new DateTime(bulanLalu.Year, bulanLalu.Month, 1);
        var akhirBulanLalu = awalBulanLalu.AddMonths(1).AddDays(-1);
        StartDate = new DateTimeOffset(awalBulanLalu);
        EndDate = new DateTimeOffset(akhirBulanLalu);
        LoadDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private void SetRangeTahunIni()
    {
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, 1, 1));
        EndDate = new DateTimeOffset(now.Date);
        LoadDashboardCommand.Execute(null);
    }

    [RelayCommand]
    private void SetRangeSemua()
    {
        StartDate = null;
        EndDate = null;
        LoadDashboardCommand.Execute(null);
    }

    partial void OnStartDateChanged(DateTimeOffset? value) => LoadDashboardCommand.Execute(null);
    partial void OnEndDateChanged(DateTimeOffset? value) => LoadDashboardCommand.Execute(null);
}