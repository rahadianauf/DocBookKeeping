namespace DocBookKeeping.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class TransBarangRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    private class TotalHppRow
    {
        public decimal Total { get; set; }
    }
    public TransBarangRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }


    public async Task<List<TransBarang>> GetAllTransBarangAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TransBarangs
            .Include(t => t.IdBarangNavigation)
            .Include(t => t.IdPemasokNavigation)
            .Include(t => t.IdPasienNavigation) 
            .AsNoTracking()
            .OrderByDescending(t => t.TanggalTransaksi)   // sebelumnya .TanggalInput
            .ToListAsync();
    }

    public async Task<string> GenerateNextIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lastId = await context.TransBarangs
            .OrderByDescending(t => t.IdTrans)
            .Select(t => t.IdTrans)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastId is not null && lastId.Length > 2 &&
            int.TryParse(lastId.Substring(2), out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return $"TB{nextNumber:D4}";
    }

    public async Task<string> AddTransBarangAsync(
    string idBarang, int? idPemasok, string tag, string? sumber, string? tujuanKeluar,
    int jumlah, decimal hargaSatuan, decimal nilai,
    string? tanggalKadaluwarsa, string? keterangan, string? idTransProduksi,
    string? idPasien, string tanggalTransaksi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newId = await GenerateNextIdAsync();

        context.TransBarangs.Add(new TransBarang
        {
            IdTrans = newId,
            IdBarang = idBarang,
            IdPemasok = idPemasok,
            Tag = tag,
            Sumber = sumber,
            TujuanKeluar = tujuanKeluar,
            IdTransProduksi = idTransProduksi,
            IdPasien = idPasien,
            Jumlah = jumlah,
            HargaSatuan = hargaSatuan,
            Nilai = nilai,
            TanggalKadaluwarsa = tanggalKadaluwarsa,
            Keterangan = keterangan,
            TanggalTransaksi = tanggalTransaksi,
            TanggalInput = DateTime.Now.ToString("yyyy-MM-dd")
        });

        await context.SaveChangesAsync();
        return newId;
    }

    public async Task<List<TransBarang>> GetProduksiCandidatesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TransBarangs
            .Include(t => t.IdBarangNavigation)
            .Where(t => t.Tag == "MASUK" && t.Sumber == "PRODUKSI")
            .AsNoTracking()
            .OrderByDescending(t => t.TanggalInput)
            .ToListAsync();
    }

    public async Task UpdateTransBarangAsync(
    string id, string idBarang, int? idPemasok, string? sumber,
    int jumlah, decimal hargaSatuan, decimal nilai,
    string? tanggalKadaluwarsa, string? tag, string? keterangan,
    string? idPasien, string tanggalTransaksi)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var trans = await context.TransBarangs.FindAsync(id);
        if (trans is null) return;

        trans.IdBarang = idBarang;
        trans.IdPemasok = idPemasok;
        trans.Sumber = sumber;
        trans.Jumlah = jumlah;
        trans.HargaSatuan = hargaSatuan;
        trans.Nilai = nilai;
        trans.TanggalKadaluwarsa = tanggalKadaluwarsa;
        trans.Keterangan = keterangan;
        trans.IdPasien = idPasien;
        trans.TanggalTransaksi = tanggalTransaksi;

        await context.SaveChangesAsync();
    }

    public async Task DeleteTransBarangAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var trans = await context.TransBarangs.FindAsync(id);
        if (trans is null) return;

        context.TransBarangs.Remove(trans);
        await context.SaveChangesAsync();
    }

    public async Task<List<PemakaianBatchDto>> GetPemakaianByTransAsync(string idTransKeluar)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Database.SqlQuery<PemakaianBatchDto>($"""
            SELECT id_batch AS IdBatch,
                   jumlah_diambil AS JumlahDiambil,
                   harga_pokok_satuan AS HargaPokokSatuan,
                   subtotal_nilai AS SubtotalNilai
            FROM pemakaian_batch
            WHERE id_trans_keluar = {idTransKeluar}
            """).ToListAsync();
    }

    public async Task<List<HppDetailDto>> GetHppDetailAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Database.SqlQuery<HppDetailDto>($"""
            SELECT pb.id_trans_keluar AS IdTransKeluar,
                tb.tanggal_transaksi AS Tanggal,
                mb.nama_barang AS NamaBarang,
                tb.tujuan_keluar AS TujuanKeluar,
                pb.jumlah_diambil AS JumlahDiambil,
                pb.harga_pokok_satuan AS HargaPokokSatuan,
                pb.subtotal_nilai AS SubtotalNilai
            FROM pemakaian_batch pb
            JOIN trans_barang tb ON tb.id_trans = pb.id_trans_keluar
            JOIN mst_barang mb ON mb.id_barang = tb.id_barang
            ORDER BY tb.tanggal_transaksi DESC
            """).ToListAsync();
    }

    // Helper internal — generate ID pakai context yang SAMA (penting untuk transaksi atomik)
    private async Task<string> GenerateNextIdAsync(DocBookKeepingContext context)
    {
        var lastId = await context.TransBarangs
            .OrderByDescending(t => t.IdTrans)
            .Select(t => t.IdTrans)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastId is not null && lastId.Length > 2 && int.TryParse(lastId.Substring(2), out var lastNumber))
            nextNumber = lastNumber + 1;

        return $"TB{nextNumber:D4}";
    }

    public async Task<string> ProsesKonversiProduksiAsync(
        string idBarangJadi, int jumlahJadi, string tanggalTransaksi, string? keterangan,
        List<(string IdBahan, int Jumlah)> bahanList)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // 1. Catat Barang Masuk untuk barang jadi — harga sementara 0
            var idMasuk = await GenerateNextIdAsync(context);

            context.TransBarangs.Add(new TransBarang
            {
                IdTrans = idMasuk,
                IdBarang = idBarangJadi,
                Tag = "MASUK",
                Sumber = "PRODUKSI",
                Jumlah = jumlahJadi,
                HargaSatuan = 0,
                Nilai = 0,
                Keterangan = keterangan,
                TanggalTransaksi = tanggalTransaksi,
                TanggalInput = DateTime.Now.ToString("yyyy-MM-dd")
            });
            await context.SaveChangesAsync();

            // 2. Catat Barang Keluar untuk tiap bahan — otomatis tertaut via IdTransProduksi
            //    Kalau stok kurang, trigger FIFO akan RAISE(ABORT) di sini, exception
            //    ditangkap di catch, seluruh transaksi di-rollback otomatis.
            foreach (var (idBahan, jumlah) in bahanList)
            {
                var idKeluar = await GenerateNextIdAsync(context);

                context.TransBarangs.Add(new TransBarang
                {
                    IdTrans = idKeluar,
                    IdBarang = idBahan,
                    Tag = "KELUAR",
                    TujuanKeluar = "PRODUKSI",
                    IdTransProduksi = idMasuk,
                    Jumlah = jumlah,
                    HargaSatuan = 0,
                    Nilai = 0,
                    TanggalTransaksi = tanggalTransaksi,
                    TanggalInput = DateTime.Now.ToString("yyyy-MM-dd")
                });
                await context.SaveChangesAsync();
            }

            // 3. Hitung total HPP seluruh bahan yang baru dicatat
            var totalRow = await context.Database.SqlQuery<TotalHppRow>($"""
                SELECT COALESCE(SUM(pb.subtotal_nilai), 0) AS Total
                FROM pemakaian_batch pb
                JOIN trans_barang tb ON tb.id_trans = pb.id_trans_keluar
                WHERE tb.id_trans_produksi = {idMasuk}
                """).FirstOrDefaultAsync();

            var totalHpp = totalRow?.Total ?? 0;

            // 4. Update harga barang jadi berdasarkan total HPP bahan
            //    (trigger yang sudah ada otomatis hitung ulang Nilai & harga_beli di stok_batch)
            var hargaSatuanBaru = jumlahJadi > 0 ? Math.Round(totalHpp / jumlahJadi, 2) : 0;

            var masukEntity = await context.TransBarangs.FindAsync(idMasuk);
            masukEntity!.HargaSatuan = hargaSatuanBaru;
            await context.SaveChangesAsync();

            await transaction.CommitAsync();
            return idMasuk;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

public class PemakaianBatchDto
{
    public int IdBatch { get; set; }
    public int JumlahDiambil { get; set; }
    public decimal HargaPokokSatuan { get; set; }
    public decimal SubtotalNilai { get; set; }
}
public class HppDetailDto
{
    public string IdTransKeluar { get; set; } = null!;
    public string Tanggal { get; set; } = null!;
    public string NamaBarang { get; set; } = null!;
    public string? TujuanKeluar { get; set; }
    public int JumlahDiambil { get; set; }
    public decimal HargaPokokSatuan { get; set; }
    public decimal SubtotalNilai { get; set; }
}

