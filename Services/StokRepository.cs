namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class StokBarangDto
{
    public string IdBarang { get; set; } = null!;
    public string NamaBarang { get; set; } = null!;
    public string? Kategori { get; set; }
    public string? Satuan { get; set; }
    public int TotalStok { get; set; }
    public decimal NilaiPersediaan { get; set; }
}

public class StokRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public StokRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<StokBarangDto>> GetStokSemuaBarangAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Database.SqlQuery<StokBarangDto>($"""
            SELECT
                mb.id_barang AS IdBarang,
                mb.nama_barang AS NamaBarang,
                mk.kategori AS Kategori,
                ms.satuan AS Satuan,
                COALESCE(SUM(sb.sisa_jumlah), 0) AS TotalStok,
                COALESCE(SUM(sb.sisa_jumlah * sb.harga_beli), 0) AS NilaiPersediaan
            FROM mst_barang mb
            LEFT JOIN mst_kategori mk ON mk.id = mb.id_kategori
            LEFT JOIN mst_satuan ms ON ms.id = mb.id_satuan
            LEFT JOIN stok_batch sb ON sb.id_barang = mb.id_barang AND sb.sisa_jumlah > 0
            GROUP BY mb.id_barang, mb.nama_barang, mk.kategori, ms.satuan
            ORDER BY mb.nama_barang
            """).ToListAsync();
    }
}