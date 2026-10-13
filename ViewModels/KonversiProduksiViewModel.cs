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

public partial class BahanRow : ObservableObject
{
    [ObservableProperty]
    private MstBarang? selectedBarang;

    [ObservableProperty]
    private string jumlah = string.Empty;
}

public partial class KonversiProduksiViewModel : ViewModelBase
{
    private readonly TransBarangRepository _transBarangRepository;
    private readonly BarangRepository _barangRepository;
    private readonly ProduksiRepository _produksiRepository;

    public ObservableCollection<MstBarang> BarangOptions { get; } = new();
    public ObservableCollection<BahanRow> BahanRows { get; } = new();
    public ObservableCollection<BahanDetailDto> LastResult { get; } = new();

    [ObservableProperty]
    private MstBarang? formBarangJadi;

    [ObservableProperty]
    private string formJumlahJadi = string.Empty;

    [ObservableProperty]
    private DateTimeOffset? formTanggalTransaksi = DateTimeOffset.Now;

    [ObservableProperty]
    private string formKeterangan = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? lastResultId;

    [ObservableProperty]
    private decimal lastHppPerUnit;

    public KonversiProduksiViewModel(
        TransBarangRepository transBarangRepository,
        BarangRepository barangRepository,
        ProduksiRepository produksiRepository)
    {
        _transBarangRepository = transBarangRepository;
        _barangRepository = barangRepository;
        _produksiRepository = produksiRepository;

        LoadBarangOptionsCommand.Execute(null);
        TambahBahanRow();
    }

    [RelayCommand]
    private async Task LoadBarangOptions()
    {
        var list = await _barangRepository.GetAllBarangAsync();
        BarangOptions.Clear();
        foreach (var b in list) BarangOptions.Add(b);
    }

    [RelayCommand]
    private void TambahBahanRow()
    {
        BahanRows.Add(new BahanRow());
    }

    [RelayCommand]
    private void HapusBahanRow(BahanRow row)
    {
        if (BahanRows.Count > 1)
            BahanRows.Remove(row);
    }

    [RelayCommand]
    private async Task ProsesKonversi()
    {
        try
        {
            ErrorMessage = string.Empty;
            LastResult.Clear();
            LastResultId = null;

            if (FormBarangJadi is null)
            {
                ErrorMessage = "Pilih barang jadi dulu.";
                return;
            }

            if (!int.TryParse(FormJumlahJadi, out var jumlahJadi) || jumlahJadi <= 0)
            {
                ErrorMessage = "Jumlah barang jadi harus angka lebih dari 0.";
                return;
            }

            var bahanList = new List<(string IdBahan, int Jumlah)>();
            foreach (var row in BahanRows)
            {
                if (row.SelectedBarang is null) continue;

                if (!int.TryParse(row.Jumlah, out var jumlahBahan) || jumlahBahan <= 0)
                {
                    ErrorMessage = $"Jumlah untuk bahan '{row.SelectedBarang.NamaBarang}' harus angka lebih dari 0.";
                    return;
                }

                bahanList.Add((row.SelectedBarang.IdBarang, jumlahBahan));
            }

            if (bahanList.Count == 0)
            {
                ErrorMessage = "Tambahkan minimal 1 bahan yang dipakai.";
                return;
            }

            IsLoading = true;

            var tanggalStr = (FormTanggalTransaksi ?? DateTimeOffset.Now).ToString("yyyy-MM-dd");

            var newId = await _transBarangRepository.ProsesKonversiProduksiAsync(
                FormBarangJadi.IdBarang, jumlahJadi, tanggalStr, FormKeterangan, bahanList);

            // Tampilkan hasil
            var detail = await _produksiRepository.GetBahanDetailAsync(newId);
            LastResult.Clear();
            foreach (var d in detail) LastResult.Add(d);

            LastResultId = newId;
            var totalHpp = detail.Sum(d => d.HppSubtotal);
            LastHppPerUnit = jumlahJadi > 0 ? Math.Round(totalHpp / jumlahJadi, 2) : 0;

            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = TranslateDbError(ex);
            Debug.WriteLine($"[KonversiProduksiViewModel] ProsesKonversi error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static string TranslateDbError(Exception ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        if (msg.Contains("Stok tidak mencukupi"))
            return "Stok salah satu bahan tidak mencukupi. Seluruh proses konversi dibatalkan, tidak ada data yang tersimpan.";
        return "Gagal memproses konversi produksi.";
    }

    [RelayCommand]
    private void ClearForm()
    {
        FormBarangJadi = null;
        FormJumlahJadi = string.Empty;
        FormTanggalTransaksi = DateTimeOffset.Now;
        FormKeterangan = string.Empty;
        BahanRows.Clear();
        TambahBahanRow();
    }
}