namespace DocBookKeeping.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Microsoft.Data.Sqlite;

public class GDriveBackupService
{
    private static readonly string OAuthClientPath =
        Path.Combine(AppPaths.ProjectRoot, "Config", "gdrive-oauth-client.json");

    private static readonly string TokenStorePath =
        Path.Combine(AppPaths.ProjectRoot, "Config", "gdrive-token-store");

    private const string FolderId = "1yI72dLBM8p8VQ0cjrHsP2XdbTlotvlqO";

    private const int MaxBackupTersimpan = 10;

    private static readonly string[] Scopes = { DriveService.Scope.Drive };

    public async Task<string> BackupDatabaseAsync()
    {
        if (!File.Exists(OAuthClientPath))
            throw new FileNotFoundException("File OAuth client tidak ditemukan.", OAuthClientPath);

        if (FolderId == "ISI_FOLDER_ID_ANDA_DI_SINI")
            throw new InvalidOperationException("Folder ID belum diisi di GDriveBackupService.cs");

        var tempBackupPath = Path.Combine(Path.GetTempPath(), $"docbookkeeping_temp_{Guid.NewGuid()}.db");
        var sourceConnStr = AppPaths.ConnectionString;
        var destConnStr = $"Data Source={tempBackupPath}";

        using (var source = new SqliteConnection(sourceConnStr))
        using (var destination = new SqliteConnection(destConnStr))
        {
            source.Open();
            destination.Open();
            source.BackupDatabase(destination);
        }

        SqliteConnection.ClearPool(new SqliteConnection(sourceConnStr));
        SqliteConnection.ClearPool(new SqliteConnection(destConnStr));
        await Task.Delay(300);

        try
        {
            UserCredential credential;
            await using (var stream = new FileStream(OAuthClientPath, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(TokenStorePath, true));
            }

            using var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "DocBookKeeping Backup"
            });

            var fileName = $"DocBookKeeping_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = new List<string> { FolderId }
            };

            await using (var fileStream = new FileStream(tempBackupPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var request = service.Files.Create(fileMetadata, fileStream, "application/x-sqlite3");
                request.Fields = "id";

                var uploadResult = await request.UploadAsync();

                if (uploadResult.Status != UploadStatus.Completed)
                    throw new Exception($"Upload gagal: {uploadResult.Exception?.Message}");

                var fileId = request.ResponseBody.Id;

                var permission = new Google.Apis.Drive.v3.Data.Permission
                {
                    Type = "anyone",
                    Role = "writer"
                };
                await service.Permissions.Create(permission, fileId).ExecuteAsync();
            }

            await CleanupOldBackupsAsync(service);

            return fileName;
        }
        finally
        {
            try
            {
                SqliteConnection.ClearPool(new SqliteConnection(destConnStr));
                if (File.Exists(tempBackupPath))
                    File.Delete(tempBackupPath);
            }
            catch (Exception cleanupEx)
            {
                System.Diagnostics.Debug.WriteLine($"Gagal hapus file temp (diabaikan): {cleanupEx.Message}");
            }
        }
    }

    private async Task CleanupOldBackupsAsync(DriveService service)
    {
        var listRequest = service.Files.List();
        listRequest.Q = $"'{FolderId}' in parents and name contains 'DocBookKeeping_backup_' and trashed = false";
        listRequest.Fields = "files(id, name, createdTime)";
        listRequest.OrderBy = "createdTime desc";

        var result = await listRequest.ExecuteAsync();
        var files = result.Files.ToList();

        if (files.Count <= MaxBackupTersimpan) return;

        var toDelete = files.Skip(MaxBackupTersimpan);
        foreach (var file in toDelete)
        {
            try { await service.Files.Delete(file.Id).ExecuteAsync(); }
            catch { }
        }
    }
}