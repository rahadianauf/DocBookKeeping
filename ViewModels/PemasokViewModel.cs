
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

public partial class PemasokViewModel : ViewModelBase
{
    private readonly PemasokRepository _pemasokRepository;
    private List<MstPemasok> _allPemasoks = new();
    public ObservableCollection<MstPemasok> Pemasoks { get; } = new();
    [ObservableProperty]
    private MstPemasok? selectedPemasok;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddPemasokCommand))]
    private string formNamaPemasok = string.Empty;
    [ObservableProperty]
    private string formKontak = string.Empty;
    [ObservableProperty]
    private string formAlamat = string.Empty;



    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    //Loading
    public PemasokViewModel(PemasokRepository pemasokRepository)
    {
        _pemasokRepository = pemasokRepository;
        LoadPemasoksCommand.Execute(null);
    }

    //Pencarian
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }
    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
        ? _allPemasoks
        : _allPemasoks.Where(u =>
            u.NamaPemasok.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Pemasoks.Clear();

        int nomor = 1;
        foreach (var pemasok in filtered)
        {
            pemasok.No = nomor++;
            Pemasoks.Add(pemasok);
        }
    }

    public string FormModeLabel => SelectedPemasok is null
        ? "Tambah Suplier Baru"
        : $"Edit Suplier — {SelectedPemasok.NamaPemasok}";

    // Saat baris di grid dipilih, isi form dengan data user itu (mode edit)
    partial void OnSelectedPemasokChanged(MstPemasok? value)
    {
        FormNamaPemasok = value?.NamaPemasok ?? string.Empty;
        FormKontak = string.Empty;
        FormAlamat = string.Empty; 
        UpdatePemasokCommand.NotifyCanExecuteChanged();
        DeletePemasokCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
    }

    [RelayCommand]
    private async Task LoadPemasoks()
    {
        try 
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _allPemasoks = await _pemasokRepository.GetAllPemasoksAsync();
            ApplyFilter();

            //Debug.WriteLine($"[UserViewModel] Loaded {_allUsers.Count} users");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data suplier.";
            Debug.WriteLine($"[PemasokViewModel] LoadPemasoks error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddPemasok() => !string.IsNullOrWhiteSpace(FormNamaPemasok);

    [RelayCommand(CanExecute = nameof(CanAddPemasok))]
    private async Task AddPemasok()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (Pemasoks.Any(u => u.NamaPemasok.Equals(FormNamaPemasok, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Nama sudah dipakai.";
                return;
            }

            await _pemasokRepository.AddPemasokAsync(FormNamaPemasok, FormKontak,FormAlamat);
            await LoadPemasoks();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah pengguna.";
            Debug.WriteLine($"[PemasokViewModel] AddPemasok error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedPemasok is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdatePemasok()
    {
        if (SelectedPemasok is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FormNamaPemasok))
            {
                ErrorMessage = "Nama tidak boleh kosong.";
                return;
            }

            bool duplikat = Pemasoks.Any(u =>
                u.IdPemasok != SelectedPemasok.IdPemasok &&
                u.NamaPemasok.Equals(FormNamaPemasok, StringComparison.OrdinalIgnoreCase));

            if (duplikat)
            {
                ErrorMessage = "Nama sudah dipakai suplier lain.";
                return;
            }

            await _pemasokRepository.UpdatePemasokAsync(SelectedPemasok.IdPemasok, FormNamaPemasok, FormKontak, FormAlamat);
            await LoadPemasoks();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah pengguna.";
            Debug.WriteLine($"[PemasokViewModel] UpdatePemasok error: {ex}");
        }

        
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeletePemasok()
    {
        if (SelectedPemasok is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _pemasokRepository.DeletePemasokAsync(SelectedPemasok.IdPemasok);
            await LoadPemasoks();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus pemasok.";
            Debug.WriteLine($"[PemasokViewModel] DeletePemasok error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedPemasok = null;
        FormNamaPemasok = string.Empty;
        FormKontak = string.Empty;
        FormAlamat = string.Empty;
    }
}