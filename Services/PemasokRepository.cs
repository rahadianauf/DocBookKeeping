namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class PemasokRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;
    public PemasokRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

     // GET ALL
    public async Task<List<MstPemasok>> GetAllPemasoksAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstPemasoks
            .AsNoTracking()
            .ToListAsync();
    }

    // GET BY ID
    public async Task<MstPemasok?> GetPemasokByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstPemasoks.FindAsync(id);
    }

    // INSERT
    public async Task AddPemasokAsync(string namaPemasok, string? kontak, string? alamat)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.MstPemasoks.Add(new MstPemasok
        {
            NamaPemasok = namaPemasok,
            Kontak = kontak,
            Alamat = alamat
        });
        await context.SaveChangesAsync();
    }

     // UPDATE
    public async Task UpdatePemasokAsync(int id, string namaPemasok, string? kontak, string? alamat)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var pemasok = await context.MstPemasoks.FindAsync(id);
        if (pemasok is null) return;

        pemasok.NamaPemasok = namaPemasok;
        pemasok.Kontak = kontak;
        pemasok.Alamat = alamat;

        await context.SaveChangesAsync();
    }

    // DELETE
    public async Task DeletePemasokAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var pemasok = await context.MstPemasoks.FindAsync(id);
        if (pemasok is null) return;

        context.MstPemasoks.Remove(pemasok);
        await context.SaveChangesAsync();
    }
}