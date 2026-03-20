using SzerszamkolcsonzoMaui.Pages;

namespace SzerszamkolcsonzoMaui
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // NavigationPage-alapú navigáció — AppShell helyett
            MainPage = new NavigationPage(new LoginPage())
            {
                BarBackgroundColor = Color.FromArgb("#2C3E50"),
                BarTextColor = Colors.White
            };
        }
    }
}
