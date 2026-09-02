using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Models;
using DocBookKeeping.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DocBookKeeping.ViewModels;

public partial class ReportViewModel : ViewModelBase
{
    private readonly ReportRepository _reportRepository;

    public ObservableCollection<MonthlySummaryDto> PemasukanPerBulan { get; } = new();
    public ObservableCollection<MonthlySummaryDto> PengeluaranPerBulan { get; } = new();

    [ObservableProperty]
    private decimal totalPemasukanBulanIni;

    [ObservableProperty]
    private decimal totalPengeluaranBulanIni;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ReportViewModel(ReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
        LoadReportCommand.Execute(null);
    }
    [RelayCommand]
    private async Task LoadReport()
    {
        try
        {
            ErrorMessage = string.Empty;
            var tahunIni = DateTime.Now.Year;

            var pemasukan = await _reportRepository.GetPemasukanPerBulanAsync(tahunIni);
            var pengeluaran = await _reportRepository.GetPengeluaranPerBulanAsync(tahunIni);
            var ringkasan = await _reportRepository.GetRingkasanBulanIniAsync();

            PemasukanPerBulan.Clear();
            foreach (var item in pemasukan) PemasukanPerBulan.Add(item);

            PengeluaranPerBulan.Clear();
            foreach (var item in pengeluaran) PengeluaranPerBulan.Add(item);

            TotalPemasukanBulanIni = ringkasan.TotalPemasukan;
            TotalPengeluaranBulanIni = ringkasan.TotalPengeluaran;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat laporan.";
            System.Diagnostics.Debug.WriteLine($"[ReportViewModel] LoadReport error: {ex}");
        }
    }
}