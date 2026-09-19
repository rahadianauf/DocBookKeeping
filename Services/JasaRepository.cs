namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class JasaRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public JasaRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MstJasa>> GetAllJasaAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstJasas
            .Include(j => j.IdKategoriNavigation)   // <-- penting, supaya nama kategori ikut ke-load
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<string> GenerateNextIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lastId = await context.MstJasas
            .OrderByDescending(j => j.IdJasa)
            .Select(j => j.IdJasa)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastId is not null && lastId.Length > 2 &&
            int.TryParse(lastId.Substring(2), out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return $"JS{nextNumber:D4}";   // D4 = padding 4 digit: 0001, 0002, ...
    }

    public async Task AddJasaAsync(string namaJasa, int idKategori)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newId = await GenerateNextIdAsync();

        context.MstJasas.Add(new MstJasa
        {
            IdJasa = newId,
            NamaJasa = namaJasa,
            IdKategori = idKategori
        });

        await context.SaveChangesAsync();
    }

    public async Task UpdateJasaAsync(string id, string namaJasa, int idKategori)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var jasa = await context.MstJasas.FindAsync(id);
        if (jasa is null) return;

        jasa.NamaJasa = namaJasa;
        jasa.IdKategori = idKategori;

        await context.SaveChangesAsync();
    }

    public async Task DeleteJasaAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var jasa = await context.MstJasas.FindAsync(id);
        if (jasa is null) return;

        context.MstJasas.Remove(jasa);
        await context.SaveChangesAsync();
    }
}