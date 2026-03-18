using System.Windows;

namespace WpfSzerszamKolcsonzo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new LoginWindow().Show();
        }
    }
}
