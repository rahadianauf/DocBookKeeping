namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class SatuanRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public SatuanRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MstSatuan>> GetAllSatuanAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstSatuans.AsNoTracking().ToListAsync();
    }

    public async Task AddAsync(string kode, string satuan, string? keterangan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.MstSatuans.Add(new MstSatuan
        {
            Kode = kode,
            Satuan = satuan,
            Keterangan = keterangan
        });
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, string kode, string satuan, string? keterangan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.MstSatuans.FindAsync(id);
        if (item is null) return;

        item.Kode = kode;
        item.Satuan = satuan;
        item.Keterangan = keterangan;
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var item = await context.MstSatuans.FindAsync(id);
        if (item is null) return;

        context.MstSatuans.Remove(item);
        await context.SaveChangesAsync();
    }
}