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

    public ProduksiViewModel(ProduksiRepository produksiRepository)
    {
        _produksiRepository = produksiRepository;
        LoadProduksiCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadProduksi()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            var list = await _produksiRepository.GetAllProduksiAsync();
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
}