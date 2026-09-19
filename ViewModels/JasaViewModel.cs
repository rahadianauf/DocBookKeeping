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

public partial class JasaViewModel : ViewModelBase
{
    private readonly JasaRepository _jasaRepository;
    private readonly KategoriRepository _kategoriRepository;
    private List<MstJasa> _allJasa = new();

    public ObservableCollection<MstJasa> JasaList { get; } = new();
    public ObservableCollection<MstKategori> KategoriOptions { get; } = new();

    [ObservableProperty]
    private MstJasa? selectedJasa;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddJasaCommand))]
    private string formNamaJasa = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddJasaCommand))]
    private MstKategori? formKategori;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public JasaViewModel(JasaRepository jasaRepository, KategoriRepository kategoriRepository)
    {
        _jasaRepository = jasaRepository;
        _kategoriRepository = kategoriRepository;
        LoadJasaCommand.Execute(null);
        LoadKategoriOptionsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadKategoriOptions()
    {
        var kategoris = await _kategoriRepository.GetAllKategorisAsync();
        KategoriOptions.Clear();
        foreach (var k in kategoris)
            KategoriOptions.Add(k);
    }

    public string FormModeLabel => SelectedJasa is null
        ? "Tambah Jasa Baru"
        : $"Edit Jasa — {SelectedJasa.NamaJasa}";

    partial void OnSelectedJasaChanged(MstJasa? value)
    {
        FormNamaJasa = value?.NamaJasa ?? string.Empty;
        FormKategori = value?.IdKategoriNavigation;
        UpdateJasaCommand.NotifyCanExecuteChanged();
        DeleteJasaCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allJasa
            : _allJasa.Where(j => j.NamaJasa.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        JasaList.Clear();
        int nomor = 1;
        foreach (var jasa in filtered)
        {
            jasa.No = nomor++;
            JasaList.Add(jasa);
        }
    }

    [RelayCommand]
    private async Task LoadJasa()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allJasa = await _jasaRepository.GetAllJasaAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data jasa.";
            Debug.WriteLine($"[JasaViewModel] LoadJasa error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddJasa() =>
        !string.IsNullOrWhiteSpace(FormNamaJasa) && FormKategori is not null;

    [RelayCommand(CanExecute = nameof(CanAddJasa))]
    private async Task AddJasa()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (_allJasa.Any(j => j.NamaJasa.Equals(FormNamaJasa, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Nama jasa sudah dipakai.";
                return;
            }

            await _jasaRepository.AddJasaAsync(FormNamaJasa, FormKategori!.Id);
            await LoadJasa();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah jasa.";
            Debug.WriteLine($"[JasaViewModel] AddJasa error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedJasa is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateJasa()
    {
        if (SelectedJasa is null || FormKategori is null) return;

        try
        {
            ErrorMessage = string.Empty;

            bool duplikat = _allJasa.Any(j =>
                j.IdJasa != SelectedJasa.IdJasa &&
                j.NamaJasa.Equals(FormNamaJasa, StringComparison.OrdinalIgnoreCase));

            if (duplikat)
            {
                ErrorMessage = "Nama jasa sudah dipakai jasa lain.";
                return;
            }

            await _jasaRepository.UpdateJasaAsync(SelectedJasa.IdJasa, FormNamaJasa, FormKategori.Id);
            await LoadJasa();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah jasa.";
            Debug.WriteLine($"[JasaViewModel] UpdateJasa error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteJasa()
    {
        if (SelectedJasa is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _jasaRepository.DeleteJasaAsync(SelectedJasa.IdJasa);
            await LoadJasa();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus jasa. Kemungkinan jasa ini masih dipakai di transaksi.";
            Debug.WriteLine($"[JasaViewModel] DeleteJasa error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedJasa = null;
        FormNamaJasa = string.Empty;
        FormKategori = null;
    }
}