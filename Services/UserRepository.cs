namespace DocBookKeeping.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using DocBookKeeping.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class UserRepository
{
    private readonly IDbContextFactory<DocBookKeepingContext> _contextFactory;

    public UserRepository(IDbContextFactory<DocBookKeepingContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    // GET ALL
    public async Task<List<MstUser>> GetAllUsersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstUsers
            .AsNoTracking()
            .ToListAsync();
    }

    // GET BY ID
    public async Task<MstUser?> GetUserByIdAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MstUsers.FindAsync(id);
    }

    // INSERT
    public async Task AddUserAsync(string username, string password)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        context.MstUsers.Add(new MstUser
        {
            Username = username,
            Password = PasswordHasher.Hash(password)
        });
        await context.SaveChangesAsync();
    }

    // UPDATE
    public async Task UpdateUserAsync(int id, string username, string? newPassword)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.MstUsers.FindAsync(id);
        if (user is null) return;

        user.Username = username;
        // hanya update password kalau diisi ulang — kosong berarti "tidak diubah"
    if (!string.IsNullOrWhiteSpace(newPassword))
        user.Password = PasswordHasher.Hash(newPassword);

        await context.SaveChangesAsync();
    }

    // DELETE
    public async Task DeleteUserAsync(int id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.MstUsers.FindAsync(id);
        if (user is null) return;

        context.MstUsers.Remove(user);
        await context.SaveChangesAsync();
    }
}