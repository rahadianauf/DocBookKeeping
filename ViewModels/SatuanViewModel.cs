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

public partial class SatuanViewModel : ViewModelBase
{
    private readonly SatuanRepository _satuanRepository;
    private List<MstSatuan> _allSatuan = new();

    public ObservableCollection<MstSatuan> SatuanList { get; } = new();

    [ObservableProperty]
    private MstSatuan? selectedSatuan;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSatuanCommand))]
    private string formKode = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddSatuanCommand))]
    private string formSatuan = string.Empty;

    [ObservableProperty]
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public int JumlahSatuan => SatuanList.Count;

    public string FormModeLabel => SelectedSatuan is null
        ? "Tambah Satuan Baru"
        : $"Edit Satuan — {SelectedSatuan.Satuan}";

    public bool IsEditMode => SelectedSatuan is not null;

    public SatuanViewModel(SatuanRepository satuanRepository)
    {
        _satuanRepository = satuanRepository;
        LoadSatuanCommand.Execute(null);
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        ClearForm();
        IsFormVisible = true;
    }

    partial void OnSelectedSatuanChanged(MstSatuan? value)
    {
        FormKode = value?.Kode ?? string.Empty;   // <-- tambahan
        FormSatuan = value?.Satuan ?? string.Empty;
        FormKeterangan = value?.Keterangan ?? string.Empty;

        if (value is not null)
            IsFormVisible = true;

        UpdateSatuanCommand.NotifyCanExecuteChanged();
        DeleteSatuanCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
        OnPropertyChanged(nameof(IsEditMode));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allSatuan
            : _allSatuan.Where(s => s.Satuan.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        SatuanList.Clear();
        int nomor = 1;
        foreach (var s in filtered)
        {
            s.No = nomor++;
            SatuanList.Add(s);
        }

        OnPropertyChanged(nameof(JumlahSatuan));
    }

    [RelayCommand]
    private async Task LoadSatuan()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allSatuan = await _satuanRepository.GetAllSatuanAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data satuan.";
            Debug.WriteLine($"[SatuanViewModel] LoadSatuan error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddSatuan() =>
        !string.IsNullOrWhiteSpace(FormKode) && !string.IsNullOrWhiteSpace(FormSatuan);

    [RelayCommand(CanExecute = nameof(CanAddSatuan))]
    private async Task AddSatuan()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (_allSatuan.Any(s => s.Satuan.Equals(FormSatuan, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Satuan ini sudah ada.";
                return;
            }

            await _satuanRepository.AddAsync(FormKode, FormSatuan, FormKeterangan);   // <-- tambahan FormKode
            await LoadSatuan();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah satuan.";
            Debug.WriteLine($"[SatuanViewModel] AddSatuan error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedSatuan is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateSatuan()
    {
        if (SelectedSatuan is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FormSatuan))
            {
                ErrorMessage = "Nama satuan tidak boleh kosong.";
                return;
            }

            bool duplikat = _allSatuan.Any(s =>
                s.Id != SelectedSatuan.Id &&
                s.Satuan.Equals(FormSatuan, StringComparison.OrdinalIgnoreCase));

            if (duplikat)
            {
                ErrorMessage = "Satuan ini sudah dipakai satuan lain.";
                return;
            }

            await _satuanRepository.UpdateAsync(SelectedSatuan.Id, FormKode, FormSatuan, FormKeterangan);   // <-- tambahan FormKode
            await LoadSatuan();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah satuan.";
            Debug.WriteLine($"[SatuanViewModel] UpdateSatuan error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteSatuan()
    {
        if (SelectedSatuan is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _satuanRepository.DeleteAsync(SelectedSatuan.Id);
            await LoadSatuan();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus satuan. Kemungkinan satuan ini masih dipakai barang.";
            Debug.WriteLine($"[SatuanViewModel] DeleteSatuan error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedSatuan = null;
        FormKode = string.Empty;   // <-- tambahan
        FormSatuan = string.Empty;
        FormKeterangan = string.Empty;
        IsFormVisible = false;
        OnPropertyChanged(nameof(IsEditMode));
    }
}