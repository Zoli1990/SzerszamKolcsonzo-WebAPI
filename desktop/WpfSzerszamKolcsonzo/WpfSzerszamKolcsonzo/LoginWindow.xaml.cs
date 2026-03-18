using System.Windows;
using System.Windows.Controls;
using WpfSzerszamKolcsonzo.ViewModels;

namespace WpfSzerszamKolcsonzo
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            var vm = new LoginViewModel();
            vm.LoginSucceeded += OnLoginSucceeded;
            DataContext = vm;
        }

        private void OnLoginSucceeded(string token)
        {
            var main = new MainWindow();
            main.DataContext = new ViewModels.MainViewModel(token);
            main.Show();
            Close();
        }
    }
}
