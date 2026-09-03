namespace DocBookKeeping.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class PasienRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public PasienRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<MstPasien>> GetAllPasienAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstPasiens
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<string> GenerateNextIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var lastId = await context.MstPasiens
            .OrderByDescending(p => p.IdPasien)
            .Select(p => p.IdPasien)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastId is not null && lastId.Length > 2 &&
            int.TryParse(lastId.Substring(2), out var lastNumber))
        {
            nextNumber = lastNumber + 1;
        }

        return $"PS{nextNumber:D4}";
    }

    public async Task AddPasienAsync(string nama, string? noTelepon, string? alamat)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var newId = await GenerateNextIdAsync();

        context.MstPasiens.Add(new MstPasien
        {
            IdPasien = newId,
            NamaPasien = nama,
            NoTelepon = noTelepon,
            Alamat = alamat,
            TanggalDaftar = DateTime.Now.ToString("yyyy-MM-dd")   // isi manual dari C#, bukan andalkan DEFAULT SQL
        });

        await context.SaveChangesAsync();
    }

    public async Task UpdatePasienAsync(string id, string nama, string? noTelepon, string? alamat)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var pasien = await context.MstPasiens.FindAsync(id);
        if (pasien is null) return;

        pasien.NamaPasien = nama;
        pasien.NoTelepon = noTelepon;
        pasien.Alamat = alamat;
        // TanggalDaftar sengaja tidak diubah saat update — tanggal daftar tetap yang pertama kali

        await context.SaveChangesAsync();
    }

    public async Task DeletePasienAsync(string id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var pasien = await context.MstPasiens.FindAsync(id);
        if (pasien is null) return;

        context.MstPasiens.Remove(pasien);
        await context.SaveChangesAsync();
    }
}