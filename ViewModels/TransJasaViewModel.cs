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

public partial class TransJasaViewModel : ViewModelBase
{
    private readonly TransJasaRepository _transJasaRepository;
    private readonly PasienRepository _pasienRepository;
    private readonly JasaRepository _jasaRepository;
    private List<TransJasa> _allTrans = new();

    public ObservableCollection<TransJasa> TransList { get; } = new();
    public ObservableCollection<MstPasien> PasienOptions { get; } = new();
    public ObservableCollection<MstJasa> JasaOptions { get; } = new();
    

    [ObservableProperty]
    private TransJasa? selectedTrans;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private MstPasien? formPasien;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private MstJasa? formJasa;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private string formHarga = string.Empty;

    [ObservableProperty]
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string formTag = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;
    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    private DateTimeOffset? formTanggalTransaksi = DateTimeOffset.Now;

    public int JumlahTransaksi => TransList.Count;
    public decimal TotalPendapatan => TransList.Sum(t => t.Harga);
    
    public TransJasaViewModel(
        TransJasaRepository transJasaRepository,
        PasienRepository pasienRepository,
        JasaRepository jasaRepository)
    {
        _transJasaRepository = transJasaRepository;
        _pasienRepository = pasienRepository;
        _jasaRepository = jasaRepository;

        LoadTransCommand.Execute(null);
        LoadDropdownOptionsCommand.Execute(null);
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        ClearForm();
        IsFormVisible = true;
    }
    public string FormModeLabel => SelectedTrans is null
        ? "Tambah Transaksi Jasa Baru"
        : $"Edit Transaksi — {SelectedTrans.IdTrans}";

    public bool IsEditMode => SelectedTrans is not null;   // <-- tambahkan persis di bawah/dekat ini
    
    [RelayCommand]
    private async Task LoadDropdownOptions()
    {
        var pasiens = await _pasienRepository.GetAllPasienAsync();
        PasienOptions.Clear();
        foreach (var p in pasiens) PasienOptions.Add(p);

        var jasas = await _jasaRepository.GetAllJasaAsync();
        JasaOptions.Clear();
        foreach (var j in jasas) JasaOptions.Add(j);
    }

    partial void OnSelectedTransChanged(TransJasa? value)
    {
        FormPasien = value?.IdPasienNavigation;
        FormJasa = value?.IdJasaNavigation;
        FormHarga = value is null ? string.Empty : value.Harga.ToString("0");
        FormKeterangan = value?.Keterangan ?? string.Empty;
        FormTag = value?.Tag ?? string.Empty;
        FormTanggalTransaksi = DateTimeOffset.TryParse(value?.TanggalTransaksi, out var d) ? d : DateTimeOffset.Now;

        if (value is not null)
            IsFormVisible = true;   // <-- tambahan: auto-buka form saat pilih baris

        UpdateTransCommand.NotifyCanExecuteChanged();
        DeleteTransCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
        OnPropertyChanged(nameof(IsEditMode));  
        
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allTrans
            : _allTrans.Where(t =>
                (t.IdPasienNavigation?.NamaPasien.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.IdJasaNavigation?.NamaJasa.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        TransList.Clear();
        int nomor = 1;
        foreach (var trans in filtered)
        {
            trans.No = nomor++;
            TransList.Add(trans);
        }

        OnPropertyChanged(nameof(JumlahTransaksi));   // <-- tambahan
        OnPropertyChanged(nameof(TotalPendapatan));   // <-- tambahan
    }

    [RelayCommand]
    private async Task LoadTrans()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allTrans = await _transJasaRepository.GetAllTransJasaAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data transaksi jasa.";
            Debug.WriteLine($"[TransJasaViewModel] LoadTrans error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddTrans() =>
        FormJasa is not null &&
        decimal.TryParse(FormHarga, out _);

    [RelayCommand(CanExecute = nameof(CanAddTrans))]
    private async Task AddTrans()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (!decimal.TryParse(FormHarga, out var harga))
            {
                ErrorMessage = "Harga harus berupa angka.";
                return;
            }

            await _transJasaRepository.AddTransJasaAsync(
                FormPasien?.IdPasien, FormJasa!.IdJasa, harga, FormKeterangan, FormTag,
                (FormTanggalTransaksi ?? DateTimeOffset.Now).ToString("yyyy-MM-dd"));

            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            //ErrorMessage = "Gagal menambah transaksi.";
            ErrorMessage = ex.ToString();
            Debug.WriteLine($"[TransJasaViewModel] AddTrans error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedTrans is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateTrans()
    {
        if (SelectedTrans is null || FormJasa is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (!decimal.TryParse(FormHarga, out var harga))
            {
                ErrorMessage = "Harga harus berupa angka.";
                return;
            }

            await _transJasaRepository.UpdateTransJasaAsync(
                SelectedTrans.IdTrans, FormPasien?.IdPasien, FormJasa.IdJasa, harga, FormKeterangan, FormTag,
                (FormTanggalTransaksi ?? DateTimeOffset.Now).ToString("yyyy-MM-dd"));

            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah transaksi.";
            Debug.WriteLine($"[TransJasaViewModel] UpdateTrans error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteTrans()
    {
        if (SelectedTrans is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _transJasaRepository.DeleteTransJasaAsync(SelectedTrans.IdTrans);
            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus transaksi.";
            Debug.WriteLine($"[TransJasaViewModel] DeleteTrans error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedTrans = null;
        FormPasien = null;
        FormJasa = null;
        FormHarga = string.Empty;
        FormKeterangan = string.Empty;
        FormTag = string.Empty;
        FormTanggalTransaksi = DateTimeOffset.Now;
        IsFormVisible = false;   // <-- tambahan: tutup form
        OnPropertyChanged(nameof(IsEditMode));   // <-- tambahan
        
    }
}