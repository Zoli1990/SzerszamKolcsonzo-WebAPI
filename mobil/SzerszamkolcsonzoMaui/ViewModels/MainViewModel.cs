using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;
using SzerszamkolcsonzoMaui.DTOs;

namespace SzerszamkolcsonzoMaui.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const string API_BASE = "https://szerszamkolcsonzo.runasp.net/api";
        private const string SIGNALR_HUB = "https://szerszamkolcsonzo.runasp.net/hubs/eszkoz";
        private const int FALLBACK_INTERVAL_SEC = 5;

        private readonly HttpClient _http;
        private readonly string _token;
        private HubConnection? _hub;
        private IDispatcherTimer? _fallbackTimer;
        private int? _lastPendingId;

        // ── Observable state ─────────────────────────────────────────────
        private bool _loading;
        public bool Loading
        {
            get => _loading;
            set
            {
                if (SetProperty(ref _loading, value))
                    RaiseAllCanExecute();
            }
        }

        private string _connectionStatus = "Kapcsolódás...";
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (SetProperty(ref _isConnected, value))
                    RaiseAllCanExecute();
            }
        }

        private FoglalasDto? _latestPending;
        public FoglalasDto? LatestPendingFoglalas
        {
            get => _latestPending;
            private set
            {
                SetProperty(ref _latestPending, value);
                OnPropertyChanged(nameof(HasPendingFoglalas));
                OnPropertyChanged(nameof(HasNoPendingFoglalas));
            }
        }

        public bool HasPendingFoglalas => _latestPending != null;
        public bool HasNoPendingFoglalas => _latestPending == null;

        public ObservableCollection<FoglalasDto> AktivFoglalasok { get; } = new();
        public bool AktivFoglalasokUres => AktivFoglalasok.Count == 0;

        // ── Commands ─────────────────────────────────────────────────────
        public ICommand KiadvaCommand { get; }
        public ICommand NemJottCommand { get; }
        public ICommand VisszahozvaCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand KijelentkezesCommand { get; }

        // ── Constructor ──────────────────────────────────────────────────
        public MainViewModel(string token)
        {
            _token = token;
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            KiadvaCommand = new RelayCommand<FoglalasDto>(
                async f => await ExecuteAction($"{API_BASE}/Foglalasok/{f!.Id}/kiadas", "✅ Eszköz kiadva!"),
                f => f != null && f.Status == 1 && !Loading && IsConnected);

            NemJottCommand = new RelayCommand<FoglalasDto>(
                async f => await HandleNemJott(f),
                f => f != null && f.Status == 1 && !Loading && IsConnected);

            VisszahozvaCommand = new RelayCommand<FoglalasDto>(
                async f => await HandleVisszahozva(f),
                f => f != null && f.Status == 2 && !Loading && IsConnected);

            RefreshCommand = new RelayCommand(async () => await FetchFoglalasok(), () => !Loading);

            KijelentkezesCommand = new RelayCommand(async () => await Kijelentkezes());

            _ = InitialLoad();
        }

        private async Task InitialLoad()
        {
            await Task.Delay(300);
            await FetchFoglalasok();
            await ConnectSignalR();
        }

        // ── SignalR ──────────────────────────────────────────────────────
        private async Task ConnectSignalR()
        {
            try
            {
                _hub = new HubConnectionBuilder()
                    .WithUrl(SIGNALR_HUB, o => o.AccessTokenProvider = () => Task.FromResult<string?>(_token))
                    .WithAutomaticReconnect()
                    .Build();

                _hub.On<object>("EszkozStatuszValtozas", async _ =>
                    await MainThread.InvokeOnMainThreadAsync(async () => await FetchFoglalasok()));

                _hub.Reconnecting += _ =>
                {
                    MainThread.BeginInvokeOnMainThread(() => StartFallback());
                    return Task.CompletedTask;
                };

                _hub.Reconnected += _ =>
                {
                    MainThread.BeginInvokeOnMainThread(() => StopFallback());
                    return Task.CompletedTask;
                };

                await _hub.StartAsync();
                ConnectionStatus = "🟢 SignalR csatlakozva";
            }
            catch
            {
                ConnectionStatus = "🟡 Polling mód (SignalR nem elérhető)";
                StartFallback();
            }
        }

        private void StartFallback()
        {
            if (_fallbackTimer?.IsRunning == true) return;
            _fallbackTimer = Application.Current!.Dispatcher.CreateTimer();
            _fallbackTimer.Interval = TimeSpan.FromSeconds(FALLBACK_INTERVAL_SEC);
            _fallbackTimer.Tick += async (_, _) => await FetchFoglalasok();
            _fallbackTimer.Start();
        }

        private void StopFallback() => _fallbackTimer?.Stop();

        // ── API ──────────────────────────────────────────────────────────
        private async Task FetchFoglalasok()
        {
            Loading = true;
            try
            {
                var response = await _http.GetAsync($"{API_BASE}/Foglalasok");

                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode == 401)
                        await ShowAlert("Munkamenet lejárt", "Kérjük jelentkezzen be újra!");
                    IsConnected = false;
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<FoglalasDto[]>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (data == null) return;

                await DetectNewBooking(data);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    AktivFoglalasok.Clear();
                    foreach (var f in data
                        .Where(f => f.Status == 1 || f.Status == 2)
                        .OrderBy(f => f.Status)
                        .ThenByDescending(f => f.CreatedAt))
                        AktivFoglalasok.Add(f);

                    OnPropertyChanged(nameof(AktivFoglalasokUres));

                    LatestPendingFoglalas = data
                        .Where(f => f.Status == 1)
                        .OrderByDescending(f => f.CreatedAt)
                        .FirstOrDefault();
                });

                ConnectionStatus = $"✅ Frissítve: {DateTime.Now:HH:mm:ss}";
                IsConnected = true;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ConnectionStatus = $"❌ {ex.Message}";
            }
            finally { Loading = false; }
        }

        private async Task DetectNewBooking(FoglalasDto[] data)
        {
            var latest = data.Where(f => f.Status == 1)
                             .OrderByDescending(f => f.CreatedAt)
                             .FirstOrDefault();

            if (latest == null || latest.Id == _lastPendingId) return;
            _lastPendingId = latest.Id;

            try { Vibration.Vibrate(TimeSpan.FromMilliseconds(600)); } catch { }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var ok = await ShowConfirm(
                    "🔔 Új foglalás érkezett!",
                    $"Eszköz: {latest.EszkozNev}\n" +
                    $"Ügyfél: {latest.FelhasznaloNev ?? "Ismeretlen"}\n" +
                    $"Időpont: {latest.IdopontString}\n\n" +
                    "Kiadod most?",
                    "Igen", "Nem");

                if (ok) await ExecuteAction($"{API_BASE}/Foglalasok/{latest.Id}/kiadas", "✅ Eszköz kiadva!");
            });
        }

        // ── Actions ──────────────────────────────────────────────────────
        private async Task HandleNemJott(FoglalasDto? f)
        {
            if (f == null) return;
            var ok = await ShowConfirm("Törlés", $"Törlöd a foglalást?\n{f.EszkozNev}", "Igen", "Nem");
            if (ok) await ExecuteAction($"{API_BASE}/Foglalasok/{f.Id}/torles", "✅ Foglalás törölve!");
        }

        private async Task HandleVisszahozva(FoglalasDto? f)
        {
            if (f == null) return;
            var ok = await ShowConfirm("Lezárás", $"Visszahozta az ügyfél?\n{f.EszkozNev}", "Igen", "Nem");
            if (ok) await ExecuteAction($"{API_BASE}/Foglalasok/{f.Id}/lezaras", "✅ Foglalás lezárva!");
        }

        private async Task ExecuteAction(string url, string successMsg)
        {
            Loading = true;
            try
            {
                var resp = await _http.PutAsync(url, null);
                resp.EnsureSuccessStatusCode();
                await FetchFoglalasok();
                await ShowAlert("Siker", successMsg);
            }
            catch (Exception ex)
            {
                await ShowAlert("Hiba", $"Művelet sikertelen:\n{ex.Message}");
            }
            finally { Loading = false; }
        }

        private async Task Kijelentkezes()
        {
            var ok = await ShowConfirm("Kijelentkezés", "Biztosan kijelentkezel?", "Igen", "Mégse");
            if (!ok) return;

            StopFallback();
            if (_hub != null) await _hub.DisposeAsync();
            SecureStorage.Remove("jwt_token");

            MainThread.BeginInvokeOnMainThread(() =>
                Application.Current!.MainPage = new NavigationPage(new Pages.LoginPage()));
        }

        private void RaiseAllCanExecute()
        {
            (KiadvaCommand as RelayCommand<FoglalasDto>)?.RaiseCanExecuteChanged();
            (NemJottCommand as RelayCommand<FoglalasDto>)?.RaiseCanExecuteChanged();
            (VisszahozvaCommand as RelayCommand<FoglalasDto>)?.RaiseCanExecuteChanged();
            (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private static Task ShowAlert(string title, string msg) =>
            MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current!.MainPage!.DisplayAlert(title, msg, "OK"));

        private static Task<bool> ShowConfirm(string title, string msg, string yes, string no) =>
            MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current!.MainPage!.DisplayAlert(title, msg, yes, no));

        public async Task Dispose()
        {
            StopFallback();
            if (_hub != null) await _hub.DisposeAsync();
            _http.Dispose();
        }
    }
}
