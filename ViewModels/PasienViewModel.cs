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

public partial class PasienViewModel : ViewModelBase
{
    private readonly PasienRepository _pasienRepository;
    private List<MstPasien> _allPasien = new();

    public ObservableCollection<MstPasien> PasienList { get; } = new();

    [ObservableProperty]
    private MstPasien? selectedPasien;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddPasienCommand))]
    private string formNamaPasien = string.Empty;

    [ObservableProperty]
    private string formNoTelepon = string.Empty;

    [ObservableProperty]
    private string formAlamat = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public PasienViewModel(PasienRepository pasienRepository)
    {
        _pasienRepository = pasienRepository;
        LoadPasienCommand.Execute(null);
    }

    public string FormModeLabel => SelectedPasien is null
        ? "Tambah Pasien Baru"
        : $"Edit Pasien — {SelectedPasien.NamaPasien}";

    partial void OnSelectedPasienChanged(MstPasien? value)
    {
        FormNamaPasien = value?.NamaPasien ?? string.Empty;
        FormNoTelepon = value?.NoTelepon ?? string.Empty;
        FormAlamat = value?.Alamat ?? string.Empty;
        UpdatePasienCommand.NotifyCanExecuteChanged();
        DeletePasienCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allPasien
            : _allPasien.Where(p => p.NamaPasien.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        PasienList.Clear();
        int nomor = 1;
        foreach (var pasien in filtered)
        {
            pasien.No = nomor++;
            PasienList.Add(pasien);
        }
    }

    [RelayCommand]
    private async Task LoadPasien()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allPasien = await _pasienRepository.GetAllPasienAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data pasien.";
            Debug.WriteLine($"[PasienViewModel] LoadPasien error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddPasien() => !string.IsNullOrWhiteSpace(FormNamaPasien);

    [RelayCommand(CanExecute = nameof(CanAddPasien))]
    private async Task AddPasien()
    {
        try
        {
            ErrorMessage = string.Empty;
            await _pasienRepository.AddPasienAsync(FormNamaPasien, FormNoTelepon, FormAlamat);
            await LoadPasien();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah pasien.";
            Debug.WriteLine($"[PasienViewModel] AddPasien error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedPasien is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdatePasien()
    {
        if (SelectedPasien is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FormNamaPasien))
            {
                ErrorMessage = "Nama tidak boleh kosong.";
                return;
            }

            await _pasienRepository.UpdatePasienAsync(SelectedPasien.IdPasien, FormNamaPasien, FormNoTelepon, FormAlamat);
            await LoadPasien();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah pasien.";
            Debug.WriteLine($"[PasienViewModel] UpdatePasien error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeletePasien()
    {
        if (SelectedPasien is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _pasienRepository.DeletePasienAsync(SelectedPasien.IdPasien);
            await LoadPasien();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus pasien.";
            Debug.WriteLine($"[PasienViewModel] DeletePasien error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedPasien = null;
        FormNamaPasien = string.Empty;
        FormNoTelepon = string.Empty;
        FormAlamat = string.Empty;
    }
}