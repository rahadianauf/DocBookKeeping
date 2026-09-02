namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class KategoriRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;
    public KategoriRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

     // GET ALL
    public async Task<List<MstKategori>> GetAllKategorisAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstKategoris
            .AsNoTracking()
            .ToListAsync();
    }

    // GET BY ID
    public async Task<MstKategori?> GetKategoriByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstKategoris.FindAsync(id);
    }

    // INSERT
    public async Task AddKategoriAsync(string kategori)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.MstKategoris.Add(new MstKategori
        {
            Kategori = kategori
        });
        await context.SaveChangesAsync();
    }

     // UPDATE
    public async Task UpdateKategoriAsync(int id, string kategori)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var category = await context.MstKategoris.FindAsync(id);
        if (category is null) return;

        category.Kategori = kategori;

        await context.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteKategoriAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var kategori = await context.MstKategoris.FindAsync(id);
        if (kategori is null) return;

        context.MstKategoris.Remove(kategori);
        await context.SaveChangesAsync();
    }
}