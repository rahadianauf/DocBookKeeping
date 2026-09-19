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

public partial class LaporanViewModel : ViewModelBase
{
    private readonly TransJasaRepository _transJasaRepository;
    private readonly TransBarangRepository _transBarangRepository;

    private List<TransJasa> _allTransJasa = new();
    private List<TransBarang> _allTransBarang = new();

    public ObservableCollection<TransJasa> JasaFiltered { get; } = new();
    public ObservableCollection<TransBarang> BarangFiltered { get; } = new();

    [ObservableProperty]
    private DateTimeOffset? startDate;

    [ObservableProperty]
    private DateTimeOffset? endDate;

    [ObservableProperty]
    private decimal totalPemasukan;

    [ObservableProperty]
    private decimal totalPengeluaran;

    [ObservableProperty]
    private decimal saldo;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private decimal pendapatanJasa;

    [ObservableProperty]
    private decimal pendapatanPenjualanBarang;

    [ObservableProperty]
    private decimal totalPendapatan;

    [ObservableProperty]
    private decimal hppPenjualan;

    [ObservableProperty]
    private decimal labaKotor;

    [ObservableProperty]
    private decimal biayaPakaiSendiri;

    [ObservableProperty]
    private decimal kerugianRusakHilang;

    [ObservableProperty]
    private decimal labaBersih;

    [ObservableProperty]
    private decimal totalPembelianBarang;

    [ObservableProperty]
    private decimal totalBiayaOperasional;

    [ObservableProperty]
    private decimal labaBersihSetelahOperasional;

    public LaporanViewModel(TransJasaRepository transJasaRepository, TransBarangRepository transBarangRepository,BiayaOperasionalRepository biayaOperasionalRepository) 
    {
        _transJasaRepository = transJasaRepository;
        _transBarangRepository = transBarangRepository;
        _biayaOperasionalRepository = biayaOperasionalRepository; 

        // default: awal bulan ini sampai hari ini
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(now.Date);

        LoadLaporanCommand.Execute(null);
    }

    private readonly BiayaOperasionalRepository _biayaOperasionalRepository;
    private List<BiayaOperasional> _allBiayaOperasional = new();

    public ObservableCollection<BiayaOperasional> BiayaOperasionalFiltered { get; } = new();


    [RelayCommand]
    private void SetRangeBulanIni()
    {
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, now.Month, 1));
        EndDate = new DateTimeOffset(now.Date);
        ApplyFilter();
    }

    [RelayCommand]
    private void SetRangeBulanLalu()
    {
        var now = DateTime.Now;
        var bulanLalu = now.AddMonths(-1);
        var awalBulanLalu = new DateTime(bulanLalu.Year, bulanLalu.Month, 1);
        var akhirBulanLalu = awalBulanLalu.AddMonths(1).AddDays(-1);
        StartDate = new DateTimeOffset(awalBulanLalu);
        EndDate = new DateTimeOffset(akhirBulanLalu);
        ApplyFilter();
    }

    [RelayCommand]
    private void SetRangeTahunIni()
    {
        var now = DateTime.Now;
        StartDate = new DateTimeOffset(new DateTime(now.Year, 1, 1));
        EndDate = new DateTimeOffset(now.Date);
        ApplyFilter();
    }

    [RelayCommand]
    private void SetRangeSemua()
    {
        StartDate = null;
        EndDate = null;
        ApplyFilter();
    }

    partial void OnStartDateChanged(DateTimeOffset? value) => ApplyFilter();
    partial void OnEndDateChanged(DateTimeOffset? value) => ApplyFilter();

    [RelayCommand]
    private async Task LoadLaporan()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _allTransJasa = await _transJasaRepository.GetAllTransJasaAsync();
            _allTransBarang = await _transBarangRepository.GetAllTransBarangAsync();
            _allHpp = await _transBarangRepository.GetHppDetailAsync();   // <-- tambahan
            _allBiayaOperasional = await _biayaOperasionalRepository.GetAllAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data laporan.";
            Debug.WriteLine($"[LaporanViewModel] LoadLaporan error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilter()
    {
        string? startStr = StartDate?.ToString("yyyy-MM-dd");
        string? endStr = EndDate?.ToString("yyyy-MM-dd");

        var jasaFiltered = _allTransJasa.Where(t =>
            (startStr is null || string.CompareOrdinal(t.TanggalInput, startStr) >= 0) &&
            (endStr is null || string.CompareOrdinal(t.TanggalInput, endStr) <= 0));

        var barangFiltered = _allTransBarang.Where(t =>
            (startStr is null || string.CompareOrdinal(t.TanggalInput, startStr) >= 0) &&
            (endStr is null || string.CompareOrdinal(t.TanggalInput, endStr) <= 0));

        var hppFiltered = _allHpp.Where(h =>
            (startStr is null || string.CompareOrdinal(h.Tanggal, startStr) >= 0) &&
            (endStr is null || string.CompareOrdinal(h.Tanggal, endStr) <= 0));

        JasaFiltered.Clear();
        foreach (var j in jasaFiltered.OrderByDescending(t => t.TanggalInput))
            JasaFiltered.Add(j);

        BarangFiltered.Clear();
        foreach (var b in barangFiltered.OrderByDescending(t => t.TanggalInput))
            BarangFiltered.Add(b);

        HppFiltered.Clear();
        foreach (var h in hppFiltered.OrderByDescending(x => x.Tanggal))
            HppFiltered.Add(h);

        // ── Perhitungan Laba Rugi ──────────────────────────────

        PendapatanJasa = JasaFiltered.Sum(t => t.Harga);

        PendapatanPenjualanBarang = BarangFiltered
            .Where(t => t.Tag == "KELUAR" && t.TujuanKeluar == "JUAL")
            .Sum(t => t.Nilai);

        TotalPendapatan = PendapatanJasa + PendapatanPenjualanBarang;

        HppPenjualan = HppFiltered
            .Where(h => h.TujuanKeluar == "JUAL")
            .Sum(h => h.SubtotalNilai);

        LabaKotor = TotalPendapatan - HppPenjualan;

        BiayaPakaiSendiri = HppFiltered
            .Where(h => h.TujuanKeluar == "PAKAI_SENDIRI")
            .Sum(h => h.SubtotalNilai);

        KerugianRusakHilang = HppFiltered
            .Where(h => h.TujuanKeluar == "RUSAK_HILANG")
            .Sum(h => h.SubtotalNilai);

        LabaBersih = LabaKotor - BiayaPakaiSendiri - KerugianRusakHilang;

        TotalPembelianBarang = BarangFiltered
            .Where(t => t.Tag == "MASUK")
            .Sum(t => t.Nilai);

        // Angka lama tetap dipertahankan untuk kompatibilitas, tapi maknanya sudah benar sekarang
        TotalPemasukan = PendapatanJasa;
        TotalPengeluaran = TotalPembelianBarang;
        TotalHpp = HppFiltered.Sum(h => h.SubtotalNilai);
        Saldo = TotalPemasukan - TotalPengeluaran;

        //Operasional
        var biayaOpFiltered = _allBiayaOperasional.Where(b =>
            (startStr is null || string.CompareOrdinal(b.Tanggal, startStr) >= 0) &&
            (endStr is null || string.CompareOrdinal(b.Tanggal, endStr) <= 0));

        BiayaOperasionalFiltered.Clear();
        foreach (var b in biayaOpFiltered.OrderByDescending(x => x.Tanggal))
            BiayaOperasionalFiltered.Add(b);

        TotalBiayaOperasional = BiayaOperasionalFiltered.Sum(b => b.Nominal);
        LabaBersihSetelahOperasional = LabaBersih - TotalBiayaOperasional;
    }
    //private readonly TransBarangRepository _transBarangRepositoryForHpp;  
    private List<HppDetailDto> _allHpp = new();

    public ObservableCollection<HppDetailDto> HppFiltered { get; } = new();

    [ObservableProperty]
    private decimal totalHpp;
}