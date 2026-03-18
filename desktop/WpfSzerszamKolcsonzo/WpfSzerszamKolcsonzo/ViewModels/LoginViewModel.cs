using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfSzerszamKolcsonzo.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private const string API_BASE = "https://szerszamkolcsonzo.runasp.net/api";

        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

        public event Action<string>? LoginSucceeded;

        private string _email = "";
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(nameof(Email)); }
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set { _rememberMe = value; OnPropertyChanged(nameof(RememberMe)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(ButtonText));
            }
        }

        public bool IsNotLoading => !_isLoading;
        public string ButtonText => _isLoading ? "Bejelentkezés..." : "Bejelentkezés";

        public ICommand LoginCommand { get; }

        private static string SettingsFilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                         "SzerszamKolcsonzo", "login.json");

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<object>(HandleLogin, _ => !IsLoading);
            LoadSavedEmail();
        }

        private void LoadSavedEmail()
        {
            try
            {
                if (!File.Exists(SettingsFilePath)) return;
                var json = File.ReadAllText(SettingsFilePath);
                var saved = JsonSerializer.Deserialize<SavedLogin>(json);
                if (saved != null && !string.IsNullOrEmpty(saved.Email))
                {
                    Email = saved.Email;
                    RememberMe = true;
                }
            }
            catch { }
        }

        private void SaveEmail()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
                File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(new SavedLogin(Email)));
            }
            catch { }
        }

        private void ClearSavedEmail()
        {
            try { if (File.Exists(SettingsFilePath)) File.Delete(SettingsFilePath); }
            catch { }
        }

        private async void HandleLogin(object? parameter)
        {
            var password = (parameter as PasswordBox)?.Password ?? "";
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Kérjük add meg az email címet és a jelszót!";
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _http.PostAsJsonAsync($"{API_BASE}/Auth/login", new
                {
                    email = Email,
                    password = password
                });

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Hibás email vagy jelszó!"
                        : $"Hiba: {(int)response.StatusCode}";
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result == null)
                {
                    ErrorMessage = "Érvénytelen szerver válasz.";
                    return;
                }

                if (!string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Csak admin felhasználók léphetnek be!";
                    return;
                }

                if (RememberMe)
                    SaveEmail();
                else
                    ClearSavedEmail();

                Application.Current.Dispatcher.Invoke(() => LoginSucceeded?.Invoke(result.Token));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Kapcsolati hiba: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private record AuthResponse(
            [property: JsonPropertyName("token")] string Token,
            [property: JsonPropertyName("role")] string Role
        );

        private record SavedLogin(string Email);
    }
}
