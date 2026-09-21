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

public partial class BarangMasukViewModel : ViewModelBase
{
    private readonly TransBarangRepository _transBarangRepository;
    private readonly BarangRepository _barangRepository;
    private readonly PemasokRepository _pemasokRepository;
    private List<TransBarang> _allTrans = new();
    private bool _isCalculating;

    public ObservableCollection<TransBarang> TransList { get; } = new();
    public ObservableCollection<MstBarang> BarangOptions { get; } = new();
    public ObservableCollection<MstPemasok> PemasokOptions { get; } = new();
    public ObservableCollection<string> SumberOptions { get; } = new() { "BELI", "PRODUKSI" };

    [ObservableProperty]
    private TransBarang? selectedTrans;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isFormVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddTransCommand))]
    private MstBarang? formBarang;

    [ObservableProperty]
    private MstPemasok? formPemasok;

    [ObservableProperty]
    private string formSumber = "BELI";

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
    private DateTimeOffset? formTanggalKadaluwarsa;

    [ObservableProperty]
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    private readonly BiayaTambahanRepository _biayaTambahanRepository;

    public ObservableCollection<BiayaTambahanBatch> BiayaList { get; } = new();

    [ObservableProperty]
    private BiayaTambahanBatch? selectedBiaya;

    [ObservableProperty]
    private string biayaKomponen = string.Empty;

    [ObservableProperty]
    private string biayaNilai = string.Empty;

    [ObservableProperty]
    private string biayaKeterangan = string.Empty;

    [ObservableProperty]
    private decimal? hargaBeliSaatIni;

    public bool IsSumberBeli => FormSumber == "BELI";
    public int JumlahTransaksi => TransList.Count;
    public decimal TotalNilai => TransList.Sum(t => t.Nilai);

    public string FormModeLabel => SelectedTrans is null
        ? "Tambah Barang Masuk"
        : $"Edit Barang Masuk — {SelectedTrans.IdTrans}";

    public bool IsEditMode => SelectedTrans is not null;

    public BarangMasukViewModel(
        TransBarangRepository transBarangRepository,
        BarangRepository barangRepository,
        PemasokRepository pemasokRepository,
        BiayaTambahanRepository biayaTambahanRepository)
    {
        _transBarangRepository = transBarangRepository;
        _barangRepository = barangRepository;
        _pemasokRepository = pemasokRepository;
        _biayaTambahanRepository = biayaTambahanRepository; 

        LoadTransCommand.Execute(null);
        LoadDropdownOptionsCommand.Execute(null);
    }

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

    [RelayCommand]
    private void OpenAddForm()
    {
        ClearForm();
        IsFormVisible = true;
    }

    partial void OnFormSumberChanged(string value) => OnPropertyChanged(nameof(IsSumberBeli));

    partial void OnFormJumlahChanged(string value) => RecalculateFromHarga();

    partial void OnFormHargaSatuanChanged(string value)
    {
        if (_isCalculating) return;
        RecalculateFromHarga();
    }

    partial void OnFormNilaiChanged(string value)
    {
        if (_isCalculating) return;
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

    partial void OnSelectedTransChanged(TransBarang? value)
    {
        try
        {
            FormBarang = value?.IdBarangNavigation;
            FormPemasok = value?.IdPemasokNavigation;
            FormSumber = value?.Sumber ?? "BELI";
            FormJumlah = value?.Jumlah.ToString() ?? string.Empty;
            FormHargaSatuan = value?.HargaSatuan.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;
            FormNilai = value?.Nilai.ToString("0", CultureInfo.InvariantCulture) ?? string.Empty;

            FormTanggalKadaluwarsa = DateTimeOffset.TryParse(value?.TanggalKadaluwarsa, out var parsedDate)
                ? parsedDate
                : null;

            FormKeterangan = value?.Keterangan ?? string.Empty;

            if (value is not null)
            {
                IsFormVisible = true;
                LoadBiayaCommand.Execute(null);
            }
            else
            {
                BiayaList.Clear();
                HargaBeliSaatIni = null;
            }

            UpdateTransCommand.NotifyCanExecuteChanged();
            DeleteTransCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(FormModeLabel));
            OnPropertyChanged(nameof(IsEditMode));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat detail transaksi.";
            Debug.WriteLine($"[BarangMasukViewModel] OnSelectedTransChanged error: {ex}");
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
       var filtered = string.IsNullOrWhiteSpace(SearchText)
        ? _allTrans
        : _allTrans.Where(t => 
            t.IdBarangNavigation != null && 
            t.IdBarangNavigation.NamaBarang.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        TransList.Clear();
        int nomor = 1;
        foreach (var trans in filtered)
        {
            trans.No = nomor++;
            TransList.Add(trans);
        }

        OnPropertyChanged(nameof(JumlahTransaksi));   // <-- tambahan
        OnPropertyChanged(nameof(TotalNilai));         // <-- tambahan
    }

    [RelayCommand]
    private async Task LoadTrans()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            var all = await _transBarangRepository.GetAllTransBarangAsync();
            _allTrans = all.Where(t => t.Tag == "MASUK").ToList();   // filter cuma MASUK
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data barang masuk.";
            Debug.WriteLine($"[BarangMasukViewModel] LoadTrans error: {ex}");
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
                IsSumberBeli ? FormPemasok?.IdPemasok : null,
                "MASUK",
                FormSumber,
                null,
                jumlah, harga, nilai,
                FormTanggalKadaluwarsa?.ToString("yyyy-MM-dd"),
                FormKeterangan);

            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = TranslateDbError(ex, "Gagal menambah barang masuk.");
            Debug.WriteLine($"[BarangMasukViewModel] AddTrans error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedTrans is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateTrans()
    {
        if (SelectedTrans is null || FormBarang is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (!ValidateAndParse(out var jumlah, out var harga, out var nilai))
                return;

            await _transBarangRepository.UpdateTransBarangAsync(
                SelectedTrans.IdTrans, FormBarang.IdBarang,
                IsSumberBeli ? FormPemasok?.IdPemasok : null,
                FormSumber,
                jumlah, harga, nilai,
                FormTanggalKadaluwarsa?.ToString("yyyy-MM-dd"),
                FormKeterangan);

            await LoadTrans();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = TranslateDbError(ex, "Gagal mengubah barang masuk.");
            Debug.WriteLine($"[BarangMasukViewModel] UpdateTrans error: {ex}");
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

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
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
            ErrorMessage = TranslateDbError(ex, "Gagal menghapus. Kemungkinan stok ini sudah terpakai di transaksi lain.");
            Debug.WriteLine($"[BarangMasukViewModel] DeleteTrans error: {ex}");
        }
    }

    private static string TranslateDbError(Exception ex, string fallback)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        if (msg.Contains("Batch sudah pernah dipakai"))
            return "Tidak bisa dihapus/diubah — stok ini sudah terpakai di transaksi keluar lain.";
        if (msg.Contains("Jumlah baru lebih kecil"))
            return "Jumlah tidak boleh dikurangi di bawah jumlah yang sudah terpakai.";
        return fallback;
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedTrans = null;
        FormBarang = null;
        FormPemasok = null;
        FormSumber = "BELI";
        FormJumlah = string.Empty;
        FormHargaSatuan = string.Empty;
        FormNilai = string.Empty;
        FormTanggalKadaluwarsa = null;
        FormKeterangan = string.Empty;
        IsFormVisible = false;
        OnPropertyChanged(nameof(IsEditMode));
    }

    [RelayCommand]
    private async Task LoadBiaya()
    {
        if (SelectedTrans is null) return;

        try
        {
            var biayaList = await _biayaTambahanRepository.GetByTransAsync(SelectedTrans.IdTrans);
            BiayaList.Clear();
            foreach (var b in biayaList) BiayaList.Add(b);

            HargaBeliSaatIni = await _biayaTambahanRepository.GetHargaBeliSaatIniAsync(SelectedTrans.IdTrans);
        }
        catch (Exception ex)
        {
            //ErrorMessage = ex.ToString();
            ErrorMessage = "Gagal memuat biaya tambahan.";
            Debug.WriteLine($"[BarangMasukViewModel] LoadBiaya error: {ex}");
        }
    }

    private bool CanAddBiaya() =>
        SelectedTrans is not null &&
        !string.IsNullOrWhiteSpace(BiayaKomponen) &&
        decimal.TryParse(BiayaNilai, NumberStyles.Number, CultureInfo.InvariantCulture, out _);

    [RelayCommand(CanExecute = nameof(CanAddBiaya))]
    private async Task AddBiaya()
    {
        if (SelectedTrans is null) return;

        try
        {
            if (!decimal.TryParse(BiayaNilai, NumberStyles.Number, CultureInfo.InvariantCulture, out var nilai))
            {
                ErrorMessage = "Nilai biaya harus berupa angka.";
                return;
            }

            await _biayaTambahanRepository.AddAsync(SelectedTrans.IdTrans, BiayaKomponen, nilai, BiayaKeterangan);
            await LoadBiaya();

            BiayaKomponen = string.Empty;
            BiayaNilai = string.Empty;
            BiayaKeterangan = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah biaya.";
            Debug.WriteLine($"[BarangMasukViewModel] AddBiaya error: {ex}");
        }
    }

    private bool CanDeleteBiaya() => SelectedBiaya is not null;

    [RelayCommand(CanExecute = nameof(CanDeleteBiaya))]
    private async Task DeleteBiaya()
    {
        if (SelectedBiaya is null) return;

        try
        {
            await _biayaTambahanRepository.DeleteAsync(SelectedBiaya.Id);
            await LoadBiaya();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus biaya.";
            Debug.WriteLine($"[BarangMasukViewModel] DeleteBiaya error: {ex}");
        }
    }

    partial void OnSelectedBiayaChanged(BiayaTambahanBatch? value) => DeleteBiayaCommand.NotifyCanExecuteChanged();

    partial void OnBiayaKomponenChanged(string value) => AddBiayaCommand.NotifyCanExecuteChanged();
    partial void OnBiayaNilaiChanged(string value) => AddBiayaCommand.NotifyCanExecuteChanged();
}
