namespace DocBookKeeping.Services;

public static class PasswordHasher
{
    public static string Hash(string plainPassword)
        => BCrypt.Net.BCrypt.HashPassword(plainPassword);

    public static bool Verify(string plainPassword, string hashedPassword)
        => BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
}