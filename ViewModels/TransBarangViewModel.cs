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

public partial class TransBarangViewModel : ViewModelBase
{
    private readonly TransBarangRepository _transBarangRepository;
    private readonly BarangRepository _barangRepository;
    private readonly PemasokRepository _pemasokRepository;
    private List<TransBarang> _allTrans = new();

    // Flag supaya perubahan HargaSatuan <-> Nilai tidak saling memicu tanpa henti
    private bool _isCalculating = false;

    public ObservableCollection<TransBarang> TransList { get; } = new();
    public ObservableCollection<MstBarang> BarangOptions { get; } = new();
    public ObservableCollection<MstPemasok> PemasokOptions { get; } = new();

    [ObservableProperty]
    private TransBarang? selectedTrans;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private MstBarang? formBarang;

    [ObservableProperty]
    private MstPemasok? formPemasok;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private string formJumlah = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private string formHargaSatuan = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private string formNilai = string.Empty;

    [ObservableProperty]
    private string formTanggalKadaluwarsa = string.Empty;

    [ObservableProperty]
    private string formTag = string.Empty;

    [ObservableProperty]
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isFormVisible;

    public TransBarangViewModel(
        TransBarangRepository transBarangRepository,
        BarangRepository barangRepository,
        PemasokRepository pemasokRepository)
    {
        _transBarangRepository = transBarangRepository;
        _barangRepository = barangRepository;
        _pemasokRepository = pemasokRepository;

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
        ? "Tambah Transaksi Barang Baru"
        : $"Edit Transaksi — {SelectedTrans.IdTrans}";


    public bool IsEditMode => SelectedTrans is not null;   // <-- tambahkan persis di bawah/dekat ini

    [RelayCommand]
    private async Task LoadDropdownOptions()
    {
        var barangs = await _barangRepository.GetAllBarangAsync();
        BarangOptions.Clear();
        foreach (var b in barangs) BarangOptions.Add(b);

        var pemasoks = await _pemasokRepository.GetAllPemasoksAsync();
        PemasokOptions.Clear();
        foreach (var p in pemasoks) PemasokOptions.Add(p);
    }

    // ── PERHITUNGAN OTOMATIS ──────────────────────────────

    partial void OnFormJumlahChanged(string value)
    {
        RecalculateFromHarga();
    }

    partial void OnFormHargaSatuanChanged(string value)
    {
        if (_isCalculating) return;   // cegah loop kalau perubahan ini berasal dari Nilai
        RecalculateFromHarga();
    }

    partial void OnFormNilaiChanged(string value)
    {
        if (_isCalculating) return;   // cegah loop kalau perubahan ini berasal dari HargaSatuan
        RecalculateFromNilai();
    }

    private void RecalculateFromHarga()
    {
        if (!int.TryParse(FormJumlah, out var jumlah) || jumlah <= 0) return;
        if (!decimal.TryParse(FormHargaSatuan, NumberStyles.Number, CultureInfo.InvariantCulture, out var harga)) return;

        _isCalculating = true;
        FormNilai = (jumlah * harga).ToString("0", CultureInfo.InvariantCulture);
        _isCalculating = false;
    }

    private void RecalculateFromNilai()
    {
        if (!int.TryParse(FormJumlah, out var jumlah) || jumlah <= 0) return;
        if (!decimal.TryParse(FormNilai, NumberStyles.Number, CultureInfo.InvariantCulture, out var nilai)) return;

        _isCalculating = true;
        FormHargaSatuan = Math.Round(nilai / jumlah, 2).ToString("0.##", CultureInfo.InvariantCulture);
        _isCalculating = false;
    }

    // ──────────────────────────────────────────────────────

    partial void OnSelectedTransChanged(TransBarang? value)
    {
        FormBarang = value?.IdBarangNavigation;
        FormPemasok = value?.IdPemasokNavigation;
        FormJumlah = value?.Jumlah.ToString() ?? string.Empty;
        FormHargaSatuan = value?.HargaSatuan.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
        FormNilai = value?.Nilai.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
        FormTanggalKadaluwarsa = value?.TanggalKadaluwarsa ?? string.Empty;
        FormTag = value?.Tag ?? string.Empty;
        FormKeterangan = value?.Keterangan ?? string.Empty;

        if (value is not null)
        IsFormVisible = true;   // <-- tambahan: auto-buka form saat pilih baris


        UpdateTransCommand.NotifyCanExecuteChanged();
        DeleteTransCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
        OnPropertyChanged(nameof(IsEditMode));   // <-- tambahan
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allTrans
            : _allTrans.Where(t =>
                (t.IdBarangNavigation?.NamaBarang.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (t.IdPemasokNavigation?.NamaPemasok.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        TransList.Clear();
        int nomor = 1;
        foreach (var trans in filtered)
        {
            trans.No = nomor++;
            TransList.Add(trans);
        }
    }

    [RelayCommand]
    private async Task LoadTrans()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            _allTrans = await _transBarangRepository.GetAllTransBarangAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data transaksi barang.";
            Debug.WriteLine($"[TransBarangViewModel] LoadTrans error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanAddTrans() =>
        FormBarang is not null &&
        int.TryParse(FormJumlah, out var j) && j > 0 &&
        decimal.TryParse(FormHargaSatuan, NumberStyles.Number, CultureInfo.InvariantCulture, out _) &&
        decimal.TryParse(FormNilai, NumberStyles.Number, CultureInfo.InvariantCulture, out _);

    private bool CanUpdateSelected() =>
        SelectedTrans is not null && SelectedTrans.Tag != "KELUAR";
    
    private bool CanDeleteSelected() => SelectedTrans is not null;

    [RelayCommand(CanExecute = nameof(CanAddTrans))]
    private async Task AddTrans()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (!ValidateAndParse(out var jumlah, out var harga, out var nilai))
                return;

            await _transBarangRepository.AddTransBarangAsync(
                FormBarang!.IdBarang,
                FormPemasok?.IdPemasok,
                jumlah, harga, nilai,
                string.IsNullOrWhiteSpace(FormTanggalKadaluwarsa) ? null : FormTanggalKadaluwarsa,
                FormTag,
                FormKeterangan);

            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah transaksi. Cek tanggal kadaluwarsa (harus lebih besar dari tanggal input).";
            Debug.WriteLine($"[TransBarangViewModel] AddTrans error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedTrans is not null;

    [RelayCommand(CanExecute = nameof(CanUpdateSelected))]
    private async Task UpdateTrans()
    {
        if (SelectedTrans is null || FormBarang is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (!ValidateAndParse(out var jumlah, out var harga, out var nilai))
                return;

            await _transBarangRepository.UpdateTransBarangAsync(
                SelectedTrans.IdTrans,
                FormBarang.IdBarang,
                FormPemasok?.IdPemasok,
                jumlah, harga, nilai,
                string.IsNullOrWhiteSpace(FormTanggalKadaluwarsa) ? null : FormTanggalKadaluwarsa,
                FormTag,
                FormKeterangan);

            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah transaksi.";
            Debug.WriteLine($"[TransBarangViewModel] UpdateTrans error: {ex}");
        }
    }

    private bool ValidateAndParse(out int jumlah, out decimal harga, out decimal nilai)
    {
        jumlah = 0; harga = 0; nilai = 0;

        if (!int.TryParse(FormJumlah, out jumlah) || jumlah <= 0)
        {
            ErrorMessage = "Jumlah harus angka lebih dari 0.";
            return false;
        }

        if (!decimal.TryParse(FormHargaSatuan, NumberStyles.Number, CultureInfo.InvariantCulture, out harga))
        {
            ErrorMessage = "Harga satuan harus berupa angka.";
            return false;
        }

        if (!decimal.TryParse(FormNilai, NumberStyles.Number, CultureInfo.InvariantCulture, out nilai))
        {
            ErrorMessage = "Nilai harus berupa angka.";
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteTrans()
    {
        if (SelectedTrans is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _transBarangRepository.DeleteTransBarangAsync(SelectedTrans.IdTrans);
            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus transaksi.";
            Debug.WriteLine($"[TransBarangViewModel] DeleteTrans error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedTrans = null;
        FormBarang = null;
        FormPemasok = null;
        FormJumlah = string.Empty;
        FormHargaSatuan = string.Empty;
        FormNilai = string.Empty;
        FormTanggalKadaluwarsa = string.Empty;
        FormTag = string.Empty;
        FormKeterangan = string.Empty;
        IsFormVisible = false;   // <-- tambahan: tutup form
        OnPropertyChanged(nameof(IsEditMode));   // <-- tambahan hide form
    }
}
