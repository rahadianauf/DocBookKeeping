namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public class ProduksiSummaryDto
{
    public string IdTransMasuk { get; set; } = null!;
    public string Tanggal { get; set; } = null!;
    public string NamaBarangJadi { get; set; } = null!;
    public int JumlahDihasilkan { get; set; }
    public decimal NilaiProduksi { get; set; }
    public decimal TotalHppBahan { get; set; }
    public int JumlahTerjual { get; set; }
    public decimal TotalPendapatanJual { get; set; }
    public decimal TotalHppTerjual { get; set; }

    public decimal HppPerUnit => JumlahDihasilkan > 0 ? TotalHppBahan / JumlahDihasilkan : 0;
    public decimal HargaJualRataRata => JumlahTerjual > 0 ? TotalPendapatanJual / JumlahTerjual : 0;
    public decimal MarginTerealisasi => TotalPendapatanJual - TotalHppTerjual;
}

public class BahanDetailDto
{
    public string IdTransKeluar { get; set; } = null!;
    public string NamaBahan { get; set; } = null!;
    public int Jumlah { get; set; }
    public decimal HppSubtotal { get; set; }
}

public class ProduksiRepository
{
    private readonly IDbContextFactory<DocBookKeeping.Models.DocBookKeepingContext> _contextFactory;

    public ProduksiRepository(IDbContextFactory<DocBookKeeping.Models.DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<ProduksiSummaryDto>> GetAllProduksiAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Database.SqlQuery<ProduksiSummaryDto>($"""
            SELECT
                mp.id_trans AS IdTransMasuk,
                mp.tanggal_input AS Tanggal,
                mb.nama_barang AS NamaBarangJadi,
                mp.jumlah AS JumlahDihasilkan,
                mp.nilai AS NilaiProduksi,
                COALESCE(bahan.total_hpp_bahan, 0) AS TotalHppBahan,
                COALESCE(jual.total_terjual, 0) AS JumlahTerjual,
                COALESCE(jual.total_pendapatan, 0) AS TotalPendapatanJual,
                COALESCE(jual.total_hpp_terjual, 0) AS TotalHppTerjual
            FROM trans_barang mp
            JOIN mst_barang mb ON mb.id_barang = mp.id_barang
            LEFT JOIN (
                SELECT tb.id_trans_produksi AS id_trans_produksi,
                       SUM(pb.subtotal_nilai) AS total_hpp_bahan
                FROM trans_barang tb
                JOIN pemakaian_batch pb ON pb.id_trans_keluar = tb.id_trans
                WHERE tb.TAG = 'KELUAR' AND tb.id_trans_produksi IS NOT NULL
                GROUP BY tb.id_trans_produksi
            ) bahan ON bahan.id_trans_produksi = mp.id_trans
            LEFT JOIN (
                SELECT sb.id_trans_masuk AS id_trans_masuk,
                       SUM(pb.jumlah_diambil) AS total_terjual,
                       SUM(pb.subtotal_nilai) AS total_hpp_terjual,
                       SUM(tbk.nilai * (pb.jumlah_diambil * 1.0 / tbk.jumlah)) AS total_pendapatan
                FROM stok_batch sb
                JOIN pemakaian_batch pb ON pb.id_batch = sb.id_batch
                JOIN trans_barang tbk ON tbk.id_trans = pb.id_trans_keluar AND tbk.tujuan_keluar = 'JUAL'
                GROUP BY sb.id_trans_masuk
            ) jual ON jual.id_trans_masuk = mp.id_trans
            WHERE mp.TAG = 'MASUK' AND mp.sumber = 'PRODUKSI'
            ORDER BY mp.tanggal_input DESC
            """).ToListAsync();
    }

    public async Task<List<BahanDetailDto>> GetBahanDetailAsync(string idTransMasuk)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Database.SqlQuery<BahanDetailDto>($"""
            SELECT
                tb.id_trans AS IdTransKeluar,
                mb.nama_barang AS NamaBahan,
                tb.jumlah AS Jumlah,
                pb.subtotal_nilai AS HppSubtotal
            FROM trans_barang tb
            JOIN pemakaian_batch pb ON pb.id_trans_keluar = tb.id_trans
            JOIN mst_barang mb ON mb.id_barang = tb.id_barang
            WHERE tb.id_trans_produksi = {idTransMasuk} AND tb.TAG = 'KELUAR'
            """).ToListAsync();
    }
}