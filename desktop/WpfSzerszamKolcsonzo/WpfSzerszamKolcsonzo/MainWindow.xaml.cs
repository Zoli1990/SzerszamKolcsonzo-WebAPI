using System.Windows;
using WpfSzerszamKolcsonzo.ViewModels;

namespace WpfSzerszamKolcsonzo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (DataContext is MainViewModel vm)
                await vm.DisconnectSignalR();
        }
    }
}