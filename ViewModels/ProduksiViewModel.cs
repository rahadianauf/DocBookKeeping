namespace DocBookKeeping.ViewModels;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Services;

public partial class ProduksiViewModel : ViewModelBase
{
    private readonly ProduksiRepository _produksiRepository;

    public ObservableCollection<ProduksiSummaryDto> ProduksiList { get; } = new();
    public ObservableCollection<BahanDetailDto> BahanDetail { get; } = new();

    [ObservableProperty]
    private ProduksiSummaryDto? selectedProduksi;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private DateTimeOffset? startDate;

    [ObservableProperty]
    private DateTimeOffset? endDate;

    public ProduksiViewModel(ProduksiRepository produksiRepository)
    {
        _produksiRepository = produksiRepository;

        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(now.Date);

        LoadProduksiCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadProduksi()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var startStr = StartDate?.ToString("yyyy-MM-dd");
            var endStr = EndDate?.ToString("yyyy-MM-dd");

            var list = await _produksiRepository.GetAllProduksiAsync(startStr, endStr);
            ProduksiList.Clear();
            foreach (var p in list) ProduksiList.Add(p);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data produksi.";
            Debug.WriteLine($"[ProduksiViewModel] LoadProduksi error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSelectedProduksiChanged(ProduksiSummaryDto? value)
    {
        BahanDetail.Clear();
        if (value is null) return;

        _ = LoadBahanDetail(value.IdTransMasuk);
    }

    private async Task LoadBahanDetail(string idTransMasuk)
    {
        try
        {
            var detail = await _produksiRepository.GetBahanDetailAsync(idTransMasuk);
            BahanDetail.Clear();
            foreach (var b in detail) BahanDetail.Add(b);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat rincian bahan.";
            Debug.WriteLine($"[ProduksiViewModel] LoadBahanDetail error: {ex}");
        }
    }

    [RelayCommand]
    private void SetRangeBulanIni()
    {
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(now.Date);
    }

    [RelayCommand]
    private void SetRangeBulanLalu()
    {
        var now = DateTime.Now;
        var bulanLalu = now.AddMonths(-1);
        var awal = new DateTime(bulanLalu.Year, bulanLalu.Month, 1);
        StartDate = new DateTimeOffset(awal);
        EndDate = new DateTimeOffset(awal.AddMonths(1).AddDays(-1));
    }

    [RelayCommand]
    private void SetRangeTahunIni()
    {
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, 1, 1));
        EndDate = new DateTimeOffset(now.Date);
    }

    [RelayCommand]
    private void SetRangeSemua()
    {
        StartDate = null;
        EndDate = null;
    }

    partial void OnStartDateChanged(DateTimeOffset? value) => LoadProduksiCommand.Execute(null);
    partial void OnEndDateChanged(DateTimeOffset? value) => LoadProduksiCommand.Execute(null);
}