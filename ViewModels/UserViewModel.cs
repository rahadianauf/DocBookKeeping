using DocBookKeeping.Services;
using DocBookKeeping.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DocBookKeeping.ViewModels;

public partial class UserViewModel : ViewModelBase
{
    private readonly UserRepository _userRepository;
    private List<MstUser> _allUsers = new();

    public ObservableCollection<MstUser> Users { get; } = new();

    [ObservableProperty]
    private MstUser? selectedUser;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private string formUsername = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddUserCommand))]
    private string formPassword = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;
    public UserViewModel(UserRepository userRepository)
    {
        _userRepository = userRepository;
        LoadUsersCommand.Execute(null);
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }
    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
        ? _allUsers
        : _allUsers.Where(u =>
            u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Users.Clear();

        int nomor = 1;
        foreach (var user in filtered)
        {
            user.No = nomor++;
            Users.Add(user);
        }
    }
    public string FormModeLabel => SelectedUser is null
        ? "Tambah User Baru"
        : $"Edit User — {SelectedUser.Username}";

    // Saat baris di grid dipilih, isi form dengan data user itu (mode edit)
    partial void OnSelectedUserChanged(MstUser? value)
    {
        FormUsername = value?.Username ?? string.Empty;
        FormPassword = string.Empty; 
        UpdateUserCommand.NotifyCanExecuteChanged();
        DeleteUserCommand.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(FormModeLabel));
    }
    [RelayCommand]
    private async Task LoadUsers()
    {
        try 
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _allUsers = await _userRepository.GetAllUsersAsync();
            ApplyFilter();

            //Debug.WriteLine($"[UserViewModel] Loaded {_allUsers.Count} users");
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal memuat data pengguna.";
            Debug.WriteLine($"[UserViewModel] LoadUsers error: {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private bool CanAddUser() => !string.IsNullOrWhiteSpace(FormUsername);

    [RelayCommand(CanExecute = nameof(CanAddUser))]
    private async Task AddUser()
    {
        try
        {
            ErrorMessage = string.Empty;

            if (Users.Any(u => u.Username.Equals(FormUsername, StringComparison.OrdinalIgnoreCase)))
            {
                ErrorMessage = "Username sudah dipakai.";
                return;
            }

            await _userRepository.AddUserAsync(FormUsername, FormPassword);
            await LoadUsers();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menambah pengguna.";
            Debug.WriteLine($"[UserViewModel] AddUser error: {ex}");
        }
    }

    private bool CanModifySelected() => SelectedUser is not null;

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task UpdateUser()
    {
        if (SelectedUser is null) return;

        try
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FormUsername))
            {
                ErrorMessage = "Username tidak boleh kosong.";
                return;
            }

            bool duplikat = Users.Any(u =>
                u.Id != SelectedUser.Id &&
                u.Username.Equals(FormUsername, StringComparison.OrdinalIgnoreCase));

            if (duplikat)
            {
                ErrorMessage = "Username sudah dipakai user lain.";
                return;
            }

            await _userRepository.UpdateUserAsync(SelectedUser.Id, FormUsername, FormPassword);
            await LoadUsers();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal mengubah pengguna.";
            Debug.WriteLine($"[UserViewModel] UpdateUser error: {ex}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanModifySelected))]
    private async Task DeleteUser()
    {
        if (SelectedUser is null) return;

        try
        {
            ErrorMessage = string.Empty;
            await _userRepository.DeleteUserAsync(SelectedUser.Id);
            await LoadUsers();
            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Gagal menghapus pengguna.";
            Debug.WriteLine($"[UserViewModel] DeleteUser error: {ex}");
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedUser = null;
        FormUsername = string.Empty;
        FormPassword = string.Empty;
    }
}