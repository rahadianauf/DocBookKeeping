namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class BiayaTambahanRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public BiayaTambahanRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<BiayaTambahanBatch>> GetByTransAsync(string idTransMasuk)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BiayaTambahanBatches
            .Where(b => b.IdTransMasuk == idTransMasuk)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(string idTransMasuk, string komponen, decimal nilai, string? keterangan)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.BiayaTambahanBatches.Add(new BiayaTambahanBatch
        {
            IdTransMasuk = idTransMasuk,
            Komponen = komponen,
            Nilai = nilai,
            Keterangan = keterangan
        });
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var biaya = await context.BiayaTambahanBatches.FindAsync(id);
        if (biaya is null) return;
        context.BiayaTambahanBatches.Remove(biaya);
        await context.SaveChangesAsync();
    }

    // Ambil harga_beli terkini dari stok_batch, untuk ditampilkan setelah biaya berubah
    private class HargaBeliResult
    {
        public decimal HargaBeli { get; set; }
    }
    public async Task<decimal?> GetHargaBeliSaatIniAsync(string idTransMasuk)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var result = await context.Database.SqlQuery<HargaBeliResult>($"""
            SELECT harga_beli AS HargaBeli FROM stok_batch WHERE id_trans_masuk = {idTransMasuk}
            """).FirstOrDefaultAsync();
        return result?.HargaBeli;
    }

    
}