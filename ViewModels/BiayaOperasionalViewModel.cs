namespace DocBookKeeping.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocBookKeeping.Models;
using DocBookKeeping.Services;

public partial class BiayaOperasionalViewModel : ViewModelBase
{
    private readonly BiayaOperasionalRepository _repository;
    private List<BiayaOperasional> _allBiaya = new();

    public ObservableCollection<BiayaOperasional> BiayaList { get; } = new();
    public ObservableCollection<string> KategoriOptions { get; } = new()
        { "Sewa", "Listrik", "Air", "Internet", "Lainnya" };

    [ObservableProperty]
    private BiayaOperasional? selectedBiaya;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddBiayaCommand))]
    private string formKategori = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddBiayaCommand))]
    private string formNominal = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? formTanggal = DateTimeOffset.Now;

    [ObservableProperty]
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private decimal totalBiayaBulanIni;

    public string FormModeLabel => SelectedBiaya is null
        ? "Tambah Biaya Operasional"
        : $"Edit Biaya — {SelectedBiaya.IdBiaya}";

    public bool IsEditMode => SelectedBiaya is not null;

    public int JumlahEntri => BiayaList.Count;

    public BiayaOperasionalViewModel(BiayaOperasionalRepository repository)
    {
        _repository = repository;
        LoadBiayaCommand.Execute(null);
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        ClearForm();
        IsFormVisible = true;
    }

    partial void OnSelectedBiayaChanged(BiayaOperasional? value)
    {
        FormKategori = value?.Kategori ?? string.Empty;
        FormNominal = value?.Nominal.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
        FormTanggal = DateTimeOffset.TryParse(value?.Tanggal, out var d) ? d : DateTimeOffset.Now;
        FormKeterangan = value?.Keterangan ?? string.Empty;

        if (value is not null) IsFormVisible = true;

        UpdateBiayaCommand.NotifyCanExecuteChanged();
        DeleteBiayaCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
        OnPropertyChanged(nameof(IsEditMode));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allBiaya
            : _allBiaya.Where(b => b.Kategori.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        BiayaList.Clear();
        foreach (var b in filtered.OrderByDescending(x => x.Tanggal))
            BiayaList.Add(b);

        var bulanIni = DateTime.Now.ToString("yyyy-MM");
        TotalBiayaBulanIni = _allBiaya.Where(b => b.Tanggal.StartsWith(bulanIni)).Sum(b => b.Nominal);
        OnPropertyChanged(nameof(JumlahEntri));   
    }

    [RelayCommand]
    private async Task LoadBiaya()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allBiaya = await _repository.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data biaya operasional.";
            Debug.WriteLine($"[BiayaOperasionalViewModel] LoadBiaya error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddBiaya() =>
        !string.IsNullOrWhiteSpace(FormKategori) &&
        decimal.TryParse(FormNominal, NumberStyles.Number, CultureInfo.InvariantCulture, out _);

    [RelayCommand(CanExecute = nameof(CanAddBiaya))]
    private async Task AddBiaya()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (!decimal.TryParse(FormNominal, NumberStyles.Number, CultureInfo.InvariantCulture, out var nominal))
            {
                ErrorMessage = "Nominal harus berupa angka.";
                return;
            }

            var tanggal = (FormTanggal ?? DateTimeOffset.Now).ToString("yyyy-MM-dd");
            await _repository.AddAsync(FormKategori, nominal, tanggal, FormKeterangan);
            await LoadBiaya();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah biaya.";
            Debug.WriteLine($"[BiayaOperasionalViewModel] AddBiaya error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedBiaya is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateBiaya()
    {
        if (SelectedBiaya is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (!decimal.TryParse(FormNominal, NumberStyles.Number, CultureInfo.InvariantCulture, out var nominal))
            {
                ErrorMessage = "Nominal harus berupa angka.";
                return;
            }

            var tanggal = (FormTanggal ?? DateTimeOffset.Now).ToString("yyyy-MM-dd");
            await _repository.UpdateAsync(SelectedBiaya.IdBiaya, FormKategori, nominal, tanggal, FormKeterangan);
            await LoadBiaya();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah biaya.";
            Debug.WriteLine($"[BiayaOperasionalViewModel] UpdateBiaya error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteBiaya()
    {
        if (SelectedBiaya is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _repository.DeleteAsync(SelectedBiaya.IdBiaya);
            await LoadBiaya();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus biaya.";
            Debug.WriteLine($"[BiayaOperasionalViewModel] DeleteBiaya error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedBiaya = null;
        FormKategori = string.Empty;
        FormNominal = string.Empty;
        FormTanggal = DateTimeOffset.Now;
        FormKeterangan = string.Empty;
        IsFormVisible = false;
        OnPropertyChanged(nameof(IsEditMode));
    }
}