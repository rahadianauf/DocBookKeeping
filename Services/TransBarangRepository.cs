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
            .AsNoTracking()
            .OrderByDescending(t => t.TanggalInput)
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

    public async Task AddTransBarangAsync(
        string idBarang, int? idPemasok, string tag, string? sumber, string? tujuanKeluar,
        int jumlah, decimal hargaSatuan, decimal nilai,
        string? tanggalKadaluwarsa, string? keterangan)
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
            Jumlah = jumlah,
            HargaSatuan = hargaSatuan,
            Nilai = nilai,
            TanggalKadaluwarsa = tanggalKadaluwarsa,
            Keterangan = keterangan,
            TanggalInput = DateTime.Now.ToString("yyyy-MM-dd")
        });

        await context.SaveChangesAsync();
    }

    public async Task UpdateTransBarangAsync(
        string id, string idBarang, int? idPemasok, string? sumber,
        int jumlah, decimal hargaSatuan, decimal nilai,
        string? tanggalKadaluwarsa, string? keterangan)
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
}

public class PemakaianBatchDto
{
    public int IdBatch { get; set; }
    public int JumlahDiambil { get; set; }
    public decimal HargaPokokSatuan { get; set; }
    public decimal SubtotalNilai { get; set; }
}
