namespace DocBookKeeping.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Services;

public partial class StokViewModel : ViewModelBase
{
    private readonly StokRepository _stokRepository;
    private List<StokBarangDto> _allStok = new();

    public ObservableCollection<StokBarangDto> StokList { get; } = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private decimal totalNilaiPersediaan;

    public StokViewModel(StokRepository stokRepository)
    {
        _stokRepository = stokRepository;
        LoadStokCommand.Execute(null);
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allStok
            : _allStok.Where(s => s.NamaBarang.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        StokList.Clear();
        foreach (var s in filtered)
            StokList.Add(s);

        TotalNilaiPersediaan = StokList.Sum(s => s.NilaiPersediaan);
    }

    [RelayCommand]
    private async Task LoadStok()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allStok = await _stokRepository.GetStokSemuaBarangAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data stok.";
            Debug.WriteLine($"[StokViewModel] LoadStok error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}