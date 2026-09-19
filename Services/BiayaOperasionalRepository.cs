namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class BiayaOperasionalRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public BiayaOperasionalRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<BiayaOperasional>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BiayaOperasionals.AsNoTracking().ToListAsync();
    }

    public async Task<string> GenerateNextIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var lastId = await context.BiayaOperasionals
            .OrderByDescending(b => b.IdBiaya)
            .Select(b => b.IdBiaya)
            .FirstOrDefaultAsync();

        int next = 1;
        if (lastId is not null && lastId.Length > 2 && int.TryParse(lastId.Substring(2), out var n))
            next = n + 1;

        return $"BO{next:D4}";
    }

    public async Task AddAsync(string kategori, decimal nominal, string tanggal, string? keterangan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.BiayaOperasionals.Add(new BiayaOperasional
        {
            IdBiaya = await GenerateNextIdAsync(),
            Kategori = kategori,
            Nominal = nominal,
            Tanggal = tanggal,
            Keterangan = keterangan
        });
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(string id, string kategori, decimal nominal, string tanggal, string? keterangan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var b = await context.BiayaOperasionals.FindAsync(id);
        if (b is null) return;
        b.Kategori = kategori;
        b.Nominal = nominal;
        b.Tanggal = tanggal;
        b.Keterangan = keterangan;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var b = await context.BiayaOperasionals.FindAsync(id);
        if (b is null) return;
        context.BiayaOperasionals.Remove(b);
        await context.SaveChangesAsync();
    }
}