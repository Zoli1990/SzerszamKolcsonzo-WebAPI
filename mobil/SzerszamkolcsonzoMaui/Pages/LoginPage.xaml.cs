using SzerszamkolcsonzoMaui.ViewModels;

namespace SzerszamkolcsonzoMaui.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _vm;

        public LoginPage()
        {
            InitializeComponent();
            _vm = new LoginViewModel();
            _vm.LoginSucceeded += OnLoginSucceeded;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Ha van mentett token, próbáljunk automatikusan belépni
            var token = await SecureStorage.GetAsync("jwt_token");
            if (!string.IsNullOrEmpty(token))
                NavigateToMain(token);
        }

        private void OnLoginSucceeded(string token) => NavigateToMain(token);

        private void NavigateToMain(string token)
        {
            Application.Current!.MainPage = new NavigationPage(new MainPage(token))
            {
                BarBackgroundColor = Color.FromArgb("#2C3E50"),
                BarTextColor = Colors.White
            };
        }
    }
}
