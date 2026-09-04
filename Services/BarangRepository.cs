namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class BarangRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public BarangRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MstBarang>> GetAllBarangAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstBarangs
            .Include(b => b.IdKategoriNavigation)
            .Include(b => b.IdSatuanNavigation)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<string> GenerateNextIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lastId = await context.MstBarangs
            .OrderByDescending(b => b.IdBarang)
            .Select(b => b.IdBarang)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastId is not null && lastId.Length > 2 &&
            int.TryParse(lastId.Substring(2), out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return $"BR{nextNumber:D4}";
    }

    public async Task AddBarangAsync(string namaBarang, int idKategori, int idSatuan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newId = await GenerateNextIdAsync();

        context.MstBarangs.Add(new MstBarang
        {
            IdBarang = newId,
            NamaBarang = namaBarang,
            IdKategori = idKategori,
            IdSatuan = idSatuan
        });

        await context.SaveChangesAsync();
    }

    public async Task UpdateBarangAsync(string id, string namaBarang, int idKategori, int idSatuan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var barang = await context.MstBarangs.FindAsync(id);
        if (barang is null) return;

        barang.NamaBarang = namaBarang;
        barang.IdKategori = idKategori;
        barang.IdSatuan = idSatuan;

        await context.SaveChangesAsync();
    }

    public async Task DeleteBarangAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var barang = await context.MstBarangs.FindAsync(id);
        if (barang is null) return;

        context.MstBarangs.Remove(barang);
        await context.SaveChangesAsync();
    }
}