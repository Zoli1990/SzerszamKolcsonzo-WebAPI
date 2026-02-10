//using System.Configuration;
//using System.Data;
//using System.Windows;

//namespace WpfSzerszamKolcsonzo
//{
//    /// <summary>
//    /// Interaction logic for App.xaml
//    /// </summary>
//    public partial class App : Application
//    {
//    }

//}

using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows;
using WpfSzerszamKolcsonzo.Properties; // ← fontos

namespace WpfSzerszamKolcsonzo
{
    public partial class App : Application
    {
        public static HttpClient HttpClient { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://szerszamkolcsonzo.runasp.net/api") // API címed
            };

            // JWT beolvasása a Settings-ből
            var token = Settings.Default.JwtToken; // ← így kell hivatkozni

            if (!string.IsNullOrWhiteSpace(token))
            {
                HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}

