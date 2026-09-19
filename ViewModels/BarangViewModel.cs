namespace DocBookKeeping.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Models;
using DocBookKeeping.Services;

public partial class BarangViewModel : ViewModelBase
{
    private readonly BarangRepository _barangRepository;
    private readonly KategoriRepository _kategoriRepository;
    private readonly SatuanRepository _satuanRepository;
    private List<MstBarang> _allBarang = new();

    public ObservableCollection<MstBarang> BarangList { get; } = new();
    public ObservableCollection<MstKategori> KategoriOptions { get; } = new();
    public ObservableCollection<MstSatuan> SatuanOptions { get; } = new();

    [ObservableProperty]
    private MstBarang? selectedBarang;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddBarangCommand))]
    private string formNamaBarang = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddBarangCommand))]
    private MstKategori? formKategori;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddBarangCommand))]
    private MstSatuan? formSatuan;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public BarangViewModel(
        BarangRepository barangRepository,
        KategoriRepository kategoriRepository,
        SatuanRepository satuanRepository)
    {
        _barangRepository = barangRepository;
        _kategoriRepository = kategoriRepository;
        _satuanRepository = satuanRepository;

        LoadBarangCommand.Execute(null);
        LoadDropdownOptionsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDropdownOptions()
    {
        var kategoris = await _kategoriRepository.GetAllKategorisAsync();
        KategoriOptions.Clear();
        foreach (var k in kategoris) KategoriOptions.Add(k);

        var satuans = await _satuanRepository.GetAllSatuanAsync();
        SatuanOptions.Clear();
        foreach (var s in satuans) SatuanOptions.Add(s);
    }

    public string FormModeLabel => SelectedBarang is null
        ? "Tambah Barang Baru"
        : $"Edit Barang — {SelectedBarang.NamaBarang}";

    partial void OnSelectedBarangChanged(MstBarang? value)
    {
        FormNamaBarang = value?.NamaBarang ?? string.Empty;
        FormKategori = value?.IdKategoriNavigation;
        FormSatuan = value?.IdSatuanNavigation;
        UpdateBarangCommand.NotifyCanExecuteChanged();
        DeleteBarangCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allBarang
            : _allBarang.Where(b => b.NamaBarang.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        BarangList.Clear();
        int nomor = 1;
        foreach (var barang in filtered)
        {
            barang.No = nomor++;
            BarangList.Add(barang);
        }
    }

    [RelayCommand]
    private async Task LoadBarang()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allBarang = await _barangRepository.GetAllBarangAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data barang.";
            Debug.WriteLine($"[BarangViewModel] LoadBarang error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddBarang() =>
        !string.IsNullOrWhiteSpace(FormNamaBarang) &&
        FormKategori is not null &&
        FormSatuan is not null;

    [RelayCommand(CanExecute = nameof(CanAddBarang))]
    private async Task AddBarang()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (_allBarang.Any(b => b.NamaBarang.Equals(FormNamaBarang, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Nama barang sudah dipakai.";
                return;
            }

            await _barangRepository.AddBarangAsync(FormNamaBarang, FormKategori!.Id, FormSatuan!.Id);
            await LoadBarang();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah barang.";
            Debug.WriteLine($"[BarangViewModel] AddBarang error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedBarang is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateBarang()
    {
        if (SelectedBarang is null || FormKategori is null || FormSatuan is null) return;

        try
        {
            ErrorMessage = string.Empty;

            bool duplikat = _allBarang.Any(b =>
                b.IdBarang != SelectedBarang.IdBarang &&
                b.NamaBarang.Equals(FormNamaBarang, StringComparison.OrdinalIgnoreCase));

            if (duplikat)
            {
                ErrorMessage = "Nama barang sudah dipakai barang lain.";
                return;
            }

            await _barangRepository.UpdateBarangAsync(SelectedBarang.IdBarang, FormNamaBarang, FormKategori.Id, FormSatuan.Id);
            await LoadBarang();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah barang.";
            Debug.WriteLine($"[BarangViewModel] UpdateBarang error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteBarang()
    {
        if (SelectedBarang is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _barangRepository.DeleteBarangAsync(SelectedBarang.IdBarang);
            await LoadBarang();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus barang. Kemungkinan barang ini masih dipakai di transaksi.";
            Debug.WriteLine($"[BarangViewModel] DeleteBarang error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedBarang = null;
        FormNamaBarang = string.Empty;
        FormKategori = null;
        FormSatuan = null;
    }
}