namespace DocBookKeeping.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class ReportRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public ReportRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // Rekap pemasukan per bulan (dari transaksi jasa)
    public async Task<List<MonthlySummaryDto>> GetPemasukanPerBulanAsync(int tahun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Database
            .SqlQuery<MonthlySummaryDto>($"""
                SELECT strftime('%m', tanggal_input) AS Bulan,
                       COALESCE(SUM(harga), 0) AS Total
                FROM trans_jasa
                WHERE strftime('%Y', tanggal_input) = {tahun.ToString()}
                GROUP BY Bulan
                ORDER BY Bulan
                """)
            .ToListAsync();
    }

    // Rekap pengeluaran per bulan (dari transaksi pembelian barang)
    public async Task<List<MonthlySummaryDto>> GetPengeluaranPerBulanAsync(int tahun)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.Database
            .SqlQuery<MonthlySummaryDto>($"""
                SELECT strftime('%m', tanggal_input) AS Bulan,
                       COALESCE(SUM(nilai_beli), 0) AS Total
                FROM trans_barang
                WHERE strftime('%Y', tanggal_input) = {tahun.ToString()}
                GROUP BY Bulan
                ORDER BY Bulan
                """)
            .ToListAsync();
    }

    // Ringkasan total untuk dashboard (bulan berjalan)
    public async Task<DashboardSummaryDto> GetRingkasanBulanIniAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var bulanIni = DateTime.Now.ToString("yyyy-MM");

        var pemasukan = await context.Database
            .SqlQuery<decimal>($"""
                SELECT COALESCE(SUM(harga), 0)
                FROM trans_jasa
                WHERE strftime('%Y-%m', tanggal_input) = {bulanIni}
                """)
            .FirstOrDefaultAsync();

        var pengeluaran = await context.Database
            .SqlQuery<decimal>($"""
                SELECT COALESCE(SUM(nilai_beli), 0)
                FROM trans_barang
                WHERE strftime('%Y-%m', tanggal_input) = {bulanIni}
                """)
            .FirstOrDefaultAsync();

        return new DashboardSummaryDto
        {
            TotalPemasukan = pemasukan,
            TotalPengeluaran = pengeluaran
        };
    }
}