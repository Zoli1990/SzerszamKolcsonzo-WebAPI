using SzerszamkolcsonzoMaui.ViewModels;

namespace SzerszamkolcsonzoMaui
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel _vm;

        public MainPage(string token)
        {
            InitializeComponent();
            _vm = new MainViewModel(token);
            BindingContext = _vm;
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await _vm.Dispose();
        }
    }
}
