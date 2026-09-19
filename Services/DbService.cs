namespace DocBookKeeping.Services;

using Microsoft.Data.Sqlite;
using System.IO;
using System;

public class DatabaseService
{
    private readonly string _connectionString = DocBookKeeping.AppPaths.ConnectionString;

    public SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}