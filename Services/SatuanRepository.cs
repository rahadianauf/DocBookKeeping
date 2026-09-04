namespace DocBookKeeping.Services;

using System.Collections.Generic;
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
        return await context.MstSatuans
            .AsNoTracking()
            .ToListAsync();
    }
}