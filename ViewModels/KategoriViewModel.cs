
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

namespace DocBookKeeping.ViewModels;

public partial class KategoriViewModel : ViewModelBase
{
    private readonly KategoriRepository _kategoriRepository;
    private List<MstKategori> _allKategoris = new();
    public ObservableCollection<MstKategori> Kategoris { get; } = new();
    [ObservableProperty]
    private MstKategori? selectedKategori;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddKategoriCommand))]
    private string formNamaKategori = string.Empty;



    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    //Loading
    public KategoriViewModel(KategoriRepository kategoriRepository)
    {
        _kategoriRepository = kategoriRepository;
        LoadKategorisCommand.Execute(null);
    }

    //Pencarian
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }
    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
        ? _allKategoris
        : _allKategoris.Where(u =>
            u.Kategori.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Kategoris.Clear();

        int nomor = 1;
        foreach (var category in filtered)
        {
            category.No = nomor++;
            Kategoris.Add(category);
        }
    }

    public string FormModeLabel => SelectedKategori is null
        ? "Tambah Suplier Baru"
        : $"Edit Suplier — {SelectedKategori.Kategori}";

    // Saat baris di grid dipilih, isi form dengan data user itu (mode edit)
    partial void OnSelectedKategoriChanged(MstKategori? value)
    {
        FormNamaKategori = value?.Kategori ?? string.Empty;
        UpdateKategoriCommand.NotifyCanExecuteChanged();
        DeleteKategoriCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
    }

    [RelayCommand]
    private async Task LoadKategoris()
    {
        try 
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _allKategoris = await _kategoriRepository.GetAllKategorisAsync();
            ApplyFilter();

            //Debug.WriteLine($"[UserViewModel] Loaded {_allUsers.Count} users");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data Kategori.";
            Debug.WriteLine($"[KategoriViewModel] LoadKategoris error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddKategori() => !string.IsNullOrWhiteSpace(FormNamaKategori);

    [RelayCommand(CanExecute = nameof(CanAddKategori))]
    private async Task AddKategori()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (Kategoris.Any(u => u.Kategori.Equals(FormNamaKategori, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Nama sudah dipakai.";
                return;
            }

            await _kategoriRepository.AddKategoriAsync(FormNamaKategori);
            await LoadKategoris();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah Kategori.";
            Debug.WriteLine($"[KategoriViewModel] AddKategori error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedKategori is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateKategori()
    {
        if (SelectedKategori is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FormNamaKategori))
            {
                ErrorMessage = "Nama tidak boleh kosong.";
                return;
            }

            bool duplikat = Kategoris.Any(u =>
                u.Id != SelectedKategori.Id &&
                u.Kategori.Equals(FormNamaKategori, StringComparison.OrdinalIgnoreCase));

            if (duplikat)
            {
                ErrorMessage = "Nama sudah dipakai suplier lain.";
                return;
            }

            await _kategoriRepository.UpdateKategoriAsync(SelectedKategori.Id, FormNamaKategori);
            await LoadKategoris();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah Kategori.";
            Debug.WriteLine($"[KategoriViewModel] UpdateKategori error: {ex}");
        }

        
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteKategori()
    {
        if (SelectedKategori is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _kategoriRepository.DeleteKategoriAsync(SelectedKategori.Id);
            await LoadKategoris();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus Kategori.";
            Debug.WriteLine($"[KategoriViewModel] DeleteKategori error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedKategori = null;
        FormNamaKategori = string.Empty;
    }
}