namespace DocBookKeeping.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class TransJasaRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public TransJasaRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<TransJasa>> GetAllTransJasaAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TransJasas
            .Include(t => t.IdPasienNavigation)
            .Include(t => t.IdJasaNavigation)
            .AsNoTracking()
            .OrderByDescending(t => t.TanggalInput)
            .ToListAsync();
    }

    public async Task<string> GenerateNextIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lastId = await context.TransJasas
            .OrderByDescending(t => t.IdTrans)
            .Select(t => t.IdTrans)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastId is not null && lastId.Length > 2 &&
            int.TryParse(lastId.Substring(2), out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return $"TJ{nextNumber:D4}";
    }

    public async Task AddTransJasaAsync(string? idPasien, string idJasa, decimal harga, string? keterangan, string? tag)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newId = await GenerateNextIdAsync();

        context.TransJasas.Add(new TransJasa
        {
            IdTrans = newId,
            IdPasien = idPasien,
            IdJasa = idJasa,
            Harga = harga,
            Keterangan = keterangan,
            Tag = tag,
            TanggalInput = DateTime.Now.ToString("yyyy-MM-dd")
        });

        await context.SaveChangesAsync();
    }

    public async Task UpdateTransJasaAsync(string id, string? idPasien, string idJasa, decimal harga, string? keterangan, string? tag)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var trans = await context.TransJasas.FindAsync(id);
        if (trans is null) return;

        trans.IdPasien = idPasien;
        trans.IdJasa = idJasa;
        trans.Harga = harga;
        trans.Keterangan = keterangan;
        trans.Tag = tag;

        await context.SaveChangesAsync();
    }

    public async Task DeleteTransJasaAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var trans = await context.TransJasas.FindAsync(id);
        if (trans is null) return;

        context.TransJasas.Remove(trans);
        await context.SaveChangesAsync();
    }
}