using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace SzerszamkolcsonzoMaui.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private const string API_BASE = "https://szerszamkolcsonzo.runasp.net/api";
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

        public event Action<string>? LoginSucceeded;

        // ── Properties ───────────────────────────────────────────────────
        private string _email = "";
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _errorMessage = "";
        public string ErrorMessage
        {
            get => _errorMessage;
            set { SetProperty(ref _errorMessage, value); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        private bool _rememberMe;
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetProperty(ref _rememberMe, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsNotLoading));
                OnPropertyChanged(nameof(ButtonText));
            }
        }

        public bool IsNotLoading => !_isLoading;
        public string ButtonText => _isLoading ? "Bejelentkezés..." : "Bejelentkezés";

        public ICommand LoginCommand { get; }

        // ── Constructor ──────────────────────────────────────────────────
        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(HandleLogin, () => !IsLoading);
            LoadSavedCredentials();
        }

        private void LoadSavedCredentials()
        {
            var savedEmail = Preferences.Get("saved_email", "");
            if (!string.IsNullOrEmpty(savedEmail))
            {
                Email = savedEmail;
                RememberMe = true;
            }
        }

        // ── Login ────────────────────────────────────────────────────────
        private async Task HandleLogin()
        {
            ErrorMessage = "";

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Add meg az email és jelszó mezőket!";
                return;
            }

            IsLoading = true;
            try
            {
                var response = await _http.PostAsJsonAsync($"{API_BASE}/Auth/login", new
                {
                    email = Email,
                    password = Password
                });

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        ? "Hibás email vagy jelszó!"
                        : $"Szerverhiba: {(int)response.StatusCode}";
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                if (result == null) { ErrorMessage = "Érvénytelen szerver válasz."; return; }

                if (!string.Equals(result.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Csak admin felhasználók léphetnek be!";
                    return;
                }

                // Beállítások mentése
                if (RememberMe)
                    Preferences.Set("saved_email", Email);
                else
                    Preferences.Remove("saved_email");

                await SecureStorage.SetAsync("jwt_token", result.Token);

                MainThread.BeginInvokeOnMainThread(() => LoginSucceeded?.Invoke(result.Token));
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

        private record AuthResponse(
            [property: JsonPropertyName("token")] string Token,
            [property: JsonPropertyName("role")] string Role
        );
    }
}
