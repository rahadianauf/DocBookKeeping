namespace DocBookKeeping.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.EntityFrameworkCore;

public class DashboardSummary
{
    public decimal TotalPemasukanBulanIni { get; set; }
    public decimal TotalPengeluaranBulanIni { get; set; }
    public decimal Saldo => TotalPemasukanBulanIni - TotalPengeluaranBulanIni;
    public int JumlahPasien { get; set; }
    public int JumlahBarang { get; set; }
    public int JumlahTransaksiJasaBulanIni { get; set; }
    public int JumlahTransaksiBarangBulanIni { get; set; }
}

public class DashboardRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public DashboardRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var bulanIni = DateTime.Now.ToString("yyyy-MM"); // contoh: "2026-09"

        var transJasaBulanIni = await context.TransJasas
            .Where(t => t.TanggalInput.StartsWith(bulanIni))
            .ToListAsync();

        var transBarangBulanIni = await context.TransBarangs
            .Where(t => t.TanggalInput.StartsWith(bulanIni))
            .ToListAsync();

        return new DashboardSummary
        {
            TotalPemasukanBulanIni = transJasaBulanIni.Sum(t => t.Harga),
            TotalPengeluaranBulanIni = transBarangBulanIni.Sum(t => t.Nilai),
            JumlahTransaksiJasaBulanIni = transJasaBulanIni.Count,
            JumlahTransaksiBarangBulanIni = transBarangBulanIni.Count,
            JumlahPasien = await context.MstPasiens.CountAsync(),
            JumlahBarang = await context.MstBarangs.CountAsync()
        };
    }

    public async Task<List<TransJasa>> GetRecentTransJasaAsync(int count = 5)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TransJasas
            .Include(t => t.IdJasaNavigation)
            .Include(t => t.IdPasienNavigation)
            .AsNoTracking()
            .OrderByDescending(t => t.TanggalInput)
            .ThenByDescending(t => t.IdTrans)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<TransBarang>> GetRecentTransBarangAsync(int count = 5)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.TransBarangs
            .Include(t => t.IdBarangNavigation)
            .Include(t => t.IdPemasokNavigation)
            .AsNoTracking()
            .OrderByDescending(t => t.TanggalInput)
            .ThenByDescending(t => t.IdTrans)
            .Take(count)
            .ToListAsync();
    }
}