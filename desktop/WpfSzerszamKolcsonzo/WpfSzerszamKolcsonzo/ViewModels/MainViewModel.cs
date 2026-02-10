using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Media;

// **FONTOS**: Settings eléréshez szükséges
using WpfSzerszamKolcsonzo.Properties;

namespace WpfSzerszamKolcsonzo.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private const string API_BASE = "https://szerszamkolcsonzo.runasp.net/api";
        private const int REFRESH_INTERVAL_SECONDS = 5;

        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _refreshTimer;
        private int _lastFoglalasCount = 0;
        private int? _lastPendingFoglalasId = null;

        public ObservableCollection<FoglalasDto> Foglalasok { get; } = new();

        private bool _loading;
        public bool Loading
        {
            get => _loading;
            set { _loading = value; OnPropertyChanged(nameof(Loading)); }
        }

        private string _connectionStatus = "Kapcsolódás...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set { _connectionStatus = value; OnPropertyChanged(nameof(ConnectionStatus)); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(nameof(IsConnected)); }
        }

        private string _debugInfo = "";
        public string DebugInfo
        {
            get => _debugInfo;
            set { _debugInfo = value; OnPropertyChanged(nameof(DebugInfo)); }
        }

        private FoglalasDto? _latestPendingFoglalas;
        public FoglalasDto? LatestPendingFoglalas
        {
            get => _latestPendingFoglalas;
            private set
            {
                _latestPendingFoglalas = value;
                OnPropertyChanged(nameof(LatestPendingFoglalas));
                OnPropertyChanged(nameof(HasPendingFoglalas));
            }
        }

        public bool HasPendingFoglalas => LatestPendingFoglalas != null;

        private ObservableCollection<FoglalasDto> _aktivFoglalasok = new();
        public ObservableCollection<FoglalasDto> AktivFoglalasok
        {
            get => _aktivFoglalasok;
            private set
            {
                _aktivFoglalasok = value;
                OnPropertyChanged(nameof(AktivFoglalasok));
            }
        }

        public ICommand KiadvaCommand { get; }
        public ICommand NemJottCommand { get; }
        public ICommand VisszahozvaCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand TestNotificationCommand { get; }

        public MainViewModel()
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            // JWT token betöltése a Settings-ből
            var token = Settings.Default.JwtToken;
            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                ConnectionStatus = $"Token beállítva ✓ (hossz: {token.Length})";
                LogDebug($"JWT Token configured, length: {token.Length}");
            }
            else
            {
                ConnectionStatus = "❌ Token NINCS beállítva!";
                LogDebug("ERROR: No JWT token configured!");
                MessageBox.Show(
                    "⚠️ JWT Token nincs beállítva!\n\n" +
                    "Az App.config fájlban add meg a tokent:\n" +
                    "<setting name=\"JwtToken\" serializeAs=\"String\">\n" +
                    "  <value>IDE_A_TOKEN</value>\n" +
                    "</setting>",
                    "Token hiányzik",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            KiadvaCommand = new RelayCommand<FoglalasDto>(HandleKiadva, CanExecuteKiadva);
            NemJottCommand = new RelayCommand<FoglalasDto>(HandleNemJott, CanExecuteNemJott);
            VisszahozvaCommand = new RelayCommand<FoglalasDto>(HandleVisszahozva, CanExecuteVisszahozva);
            RefreshCommand = new RelayCommand(async () => await FetchFoglalasok(), () => !Loading);
            TestNotificationCommand = new RelayCommand(TestNotification);

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(REFRESH_INTERVAL_SECONDS)
            };
            _refreshTimer.Tick += async (_, _) => await FetchFoglalasok();

            _ = InitialLoad();
        }

        private void LogDebug(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var logMessage = $"[{timestamp}] {message}";
            System.Diagnostics.Debug.WriteLine(logMessage);

            DebugInfo = logMessage + "\n" + DebugInfo;
            var lines = DebugInfo.Split('\n');
            if (lines.Length > 5)
            {
                DebugInfo = string.Join("\n", lines.Take(5));
            }
        }

        private async Task InitialLoad()
        {
            await Task.Delay(500);
            LogDebug("Initial load starting...");
            await FetchFoglalasok();

            if (IsConnected)
            {
                _refreshTimer.Start();
                LogDebug($"Auto-refresh timer started ({REFRESH_INTERVAL_SECONDS}s interval)");
            }
        }

        // ------------------------ API fetch ------------------------
        private async Task FetchFoglalasok()
        {
            Loading = true;
            ConnectionStatus = "Frissítés...";
            var url = $"{API_BASE}/Foglalasok";

            LogDebug("=== FETCH START ===");
            LogDebug($"GET {url}");

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    HandleErrorResponse(response, url);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<FoglalasDto[]>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null) { ConnectionStatus = "⚠️ Üres válasz"; return; }

                DetectNewBookings(data);

                Foglalasok.Clear();
                foreach (var f in data)
                    Foglalasok.Add(f);

                ConnectionStatus = $"✅ {data.Length} foglalás ({DateTime.Now:HH:mm:ss})";
                IsConnected = true;
                UpdateComputedProperties();
            }
            catch (Exception ex)
            {
                HandleException(ex, url);
            }
            finally
            {
                Loading = false;
            }
        }

        private void DetectNewBookings(FoglalasDto[] newData)
        {
            var pendingBookings = newData.Where(f => f.Status == 1).ToList();
            var latestPending = pendingBookings.OrderByDescending(f => f.CreatedAt).FirstOrDefault();

            if (latestPending != null && (_lastPendingFoglalasId == null || latestPending.Id != _lastPendingFoglalasId))
            {
                _lastPendingFoglalasId = latestPending.Id;
                SystemSounds.Beep.Play();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var result = MessageBox.Show(
                        $"🔔 ÚJ FOGLALÁS ÉRKEZETT!\n\nEszköz: {latestPending.Eszkoz?.Nev ?? "Ismeretlen"}\n" +
                        $"Ügyfél: {latestPending.Felhasznalo?.Nev ?? "Ismeretlen"}\n" +
                        $"Kezdés: {latestPending.KezdetDatum?.ToString("yyyy.MM.dd HH:mm") ?? "Nincs"}\n" +
                        $"Ár: {latestPending.Eszkoz?.Ar ?? 0:N0} Ft\n\n" +
                        $"Kiadod most?",
                        "Új foglalás",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                        HandleKiadva(latestPending);
                });
            }

            _lastFoglalasCount = newData.Length;
        }

        private void HandleErrorResponse(HttpResponseMessage response, string url)
        {
            IsConnected = false;
            var status = (int)response.StatusCode;
            if (status == 401)
                MessageBox.Show("🔐 JWT Token érvénytelen vagy lejárt!", "401 Unauthorized", MessageBoxButton.OK, MessageBoxImage.Warning);
            else if (status == 404)
                MessageBox.Show($"❌ API Endpoint nem található! URL: {url}", "404 Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show($"❌ HTTP Hiba: {status}", "API Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleException(Exception ex, string url)
        {
            IsConnected = false;
            ConnectionStatus = "❌ Kapcsolat hiba";
            MessageBox.Show($"❌ Hiba: {ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void UpdateComputedProperties()
        {
            LatestPendingFoglalas = Foglalasok.Where(f => f.Status == 1)
                                              .OrderByDescending(f => f.CreatedAt)
                                              .FirstOrDefault();

            AktivFoglalasok.Clear();
            foreach (var f in Foglalasok.Where(f => f.Status == 1 || f.Status == 2)
                                        .OrderBy(f => f.Status)
                                        .ThenByDescending(f => f.CreatedAt))
                AktivFoglalasok.Add(f);
        }

        private bool CanExecuteKiadva(FoglalasDto? f) => f != null && f.Status == 1 && !Loading && IsConnected;
        private bool CanExecuteNemJott(FoglalasDto? f) => f != null && f.Status == 1 && !Loading && IsConnected;
        private bool CanExecuteVisszahozva(FoglalasDto? f) => f != null && f.Status == 2 && !Loading && IsConnected;

        private async void HandleKiadva(FoglalasDto? f)
        {
            if (f == null) return;
            var result = MessageBox.Show($"Kiadod az eszközt: {f.Eszkoz?.Nev}?", "Kiadás", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                await ExecuteAction($"{API_BASE}/Foglalasok/{f.Id}/kiadas", "✅ Eszköz kiadva!");
        }

        private async void HandleNemJott(FoglalasDto? f)
        {
            if (f == null) return;
            var result = MessageBox.Show($"Törlöd a foglalást: {f.Eszkoz?.Nev}?", "Törlés", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
                await ExecuteAction($"{API_BASE}/Foglalasok/{f.Id}/torles", "✅ Foglalás törölve!");
        }

        private async void HandleVisszahozva(FoglalasDto? f)
        {
            if (f == null) return;
            var result = MessageBox.Show($"Visszahozta az ügyfél az eszközt: {f.Eszkoz?.Nev}?", "Lezárás", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                await ExecuteAction($"{API_BASE}/Foglalasok/{f.Id}/lezaras", "✅ Foglalás lezárva!");
        }

        private async Task ExecuteAction(string url, string successMessage)
        {
            Loading = true;
            try
            {
                var response = await _httpClient.PutAsync(url, null);
                response.EnsureSuccessStatusCode();
                await FetchFoglalasok();
                MessageBox.Show(successMessage, "Siker", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Művelet sikertelen\n\n{ex.Message}", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally { Loading = false; }
        }

        private void TestNotification()
        {
            SystemSounds.Beep.Play();
            MessageBox.Show($"🧪 TESZT ÉRTESÍTÉS\nFoglalások: {Foglalasok.Count}", "Teszt", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
