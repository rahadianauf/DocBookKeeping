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

public partial class BarangKeluarViewModel : ViewModelBase
{
    private readonly TransBarangRepository _transBarangRepository;
    private readonly BarangRepository _barangRepository;
    private List<TransBarang> _allTrans = new();
    private bool _isCalculating;

    public ObservableCollection<TransBarang> TransList { get; } = new();
    public ObservableCollection<MstBarang> BarangOptions { get; } = new();
    public ObservableCollection<string> TujuanKeluarOptions { get; } = new()
        { "JUAL", "PRODUKSI", "PAKAI_SENDIRI", "RUSAK_HILANG" };

    public ObservableCollection<PemakaianBatchDto> LastHppBreakdown { get; } = new();

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
    private string formTujuanKeluar = "JUAL";

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
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private decimal lastHppTotal;

    public string FormModeLabel => "Tambah Barang Keluar";
    public bool IsRowSelected => SelectedTrans is not null;

    public int JumlahTransaksi => TransList.Count;
    public decimal TotalNilai => TransList.Sum(t => t.Nilai);

    public ObservableCollection<TransBarang> ProduksiOptions { get; } = new();

    [ObservableProperty]
    private TransBarang? formProduksiTrans;

    public bool IsTujuanProduksi => FormTujuanKeluar == "PRODUKSI";
    public BarangKeluarViewModel(TransBarangRepository transBarangRepository, BarangRepository barangRepository)
    {
        _transBarangRepository = transBarangRepository;
        _barangRepository = barangRepository;

        LoadTransCommand.Execute(null);
        LoadDropdownOptionsCommand.Execute(null);
        LoadProduksiOptionsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDropdownOptions()
    {
        var barangs = await _barangRepository.GetAllBarangAsync();
        BarangOptions.Clear();
        foreach (var b in barangs) BarangOptions.Add(b);
    }

    partial void OnFormTujuanKeluarChanged(string value) => OnPropertyChanged(nameof(IsTujuanProduksi));

    [RelayCommand]
    private async Task LoadProduksiOptions()
    {
        var list = await _transBarangRepository.GetProduksiCandidatesAsync();
        ProduksiOptions.Clear();
        foreach (var p in list) ProduksiOptions.Add(p);
    }

    [RelayCommand]
    private void OpenAddForm()
    {
        ClearForm();
        IsFormVisible = true;
    }

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

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allTrans
            : _allTrans.Where(t => t.IdBarangNavigation?.NamaBarang.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false);

        TransList.Clear();
        int nomor = 1;
        foreach (var trans in filtered)
        {
            trans.No = nomor++;
            TransList.Add(trans);
        }

        OnPropertyChanged(nameof(JumlahTransaksi));
        OnPropertyChanged(nameof(TotalNilai));
    }

    [RelayCommand]
    private async Task LoadTrans()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            var all = await _transBarangRepository.GetAllTransBarangAsync();
            _allTrans = all.Where(t => t.Tag == "KELUAR").ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data barang keluar.";
            Debug.WriteLine($"[BarangKeluarViewModel] LoadTrans error: {ex}");
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

            if (!int.TryParse(FormJumlah, out var jumlah) || jumlah <= 0)
            {
                ErrorMessage = "Jumlah harus angka lebih dari 0.";
                return;
            }
            if (!decimal.TryParse(FormHargaSatuan, NumberStyles.Number, CultureInfo.InvariantCulture, out var harga))
            {
                ErrorMessage = "Harga satuan harus berupa angka.";
                return;
            }
            if (!decimal.TryParse(FormNilai, NumberStyles.Number, CultureInfo.InvariantCulture, out var nilai))
            {
                ErrorMessage = "Nilai harus berupa angka.";
                return;
            }

            var newId = await _transBarangRepository.AddTransBarangAsync(   // <-- pakai hasil return ini
                FormBarang!.IdBarang, null, "KELUAR", null, FormTujuanKeluar,
                jumlah, harga, nilai, null, FormKeterangan,
                FormProduksiTrans?.IdTrans);

            var pemakaian = await _transBarangRepository.GetPemakaianByTransAsync(newId);
            LastHppBreakdown.Clear();
            foreach (var p in pemakaian) LastHppBreakdown.Add(p);
            LastHppTotal = pemakaian.Sum(p => p.SubtotalNilai);

            await LoadTrans();
            ClearFormKeepBreakdown();
        }
        catch (Exception ex)
        {
            ErrorMessage = TranslateDbError(ex, "Gagal menambah barang keluar.");
            Debug.WriteLine($"[BarangKeluarViewModel] AddTrans error: {ex}");
        }
    }

    private bool CanDeleteSelected() => SelectedTrans is not null;

    [RelayCommand(CanExecute = nameof(CanDeleteSelected))]
    private async Task DeleteTrans()
    {
        if (SelectedTrans is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _transBarangRepository.DeleteTransBarangAsync(SelectedTrans.IdTrans);
            await LoadTrans();
            SelectedTrans = null;              
            OnPropertyChanged(nameof(IsRowSelected));   
        }
        catch (Exception ex)
        {
            ErrorMessage = TranslateDbError(ex, "Gagal menghapus transaksi.");
            Debug.WriteLine($"[BarangKeluarViewModel] DeleteTrans error: {ex}");
        }
    }

    private static string TranslateDbError(Exception ex, string fallback)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        if (msg.Contains("Stok tidak mencukupi"))
            return "Stok tidak mencukupi untuk barang ini.";
        return fallback;
    }

    partial void OnSelectedTransChanged(TransBarang? value)
    {
        DeleteTransCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsRowSelected)); 
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedTrans = null;
        FormBarang = null;
        FormTujuanKeluar = "JUAL";
        FormJumlah = string.Empty;
        FormHargaSatuan = string.Empty;
        FormNilai = string.Empty;
        FormKeterangan = string.Empty;
        IsFormVisible = false;
        LastHppBreakdown.Clear();
        LastHppTotal = 0;
         OnPropertyChanged(nameof(IsRowSelected));
         FormProduksiTrans = null;
    }

    // Setelah simpan sukses, form ditutup tapi breakdown HPP tetap ditampilkan
    private void ClearFormKeepBreakdown()
    {
        SelectedTrans = null;
        FormBarang = null;
        FormTujuanKeluar = "JUAL";
        FormJumlah = string.Empty;
        FormHargaSatuan = string.Empty;
        FormNilai = string.Empty;
        FormKeterangan = string.Empty;
        IsFormVisible = false;
        // LastHppBreakdown & LastHppTotal sengaja TIDAK direset di sini
    }

}
