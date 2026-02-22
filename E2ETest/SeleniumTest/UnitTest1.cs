using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using static System.Net.WebRequestMethods;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;


namespace SeleniumTests

{
    public class LoginAction : IDisposable
    {
        // Initiate Webdriver
        public IWebDriver driver = null;
        public string url = null;


        // Teszt előtti erőforrás beállítás    
        public LoginAction()
        {
            driver = new ChromeDriver();
            url = "http://localhost:5173/";
        }


        public void Login()
        {
            driver.Navigate().GoToUrl(url);


            // belépés gomb megnyomása a logi oldal előtt
            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));

            belepesButton.Click();

            // elem kikeresése
            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));

            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));

            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));



            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("test@gmail.com");
                passwordTextBox.SendKeys("Teszt123456@");
                loginButton.Click();
            }
        }


        // Teszt utáni erőforrás felszabadítás
        public void Dispose()
        {
            driver.Quit();
        }
    };


    public class LoginTest : IDisposable  //***
    {
        // Initiate Webdriver
        IWebDriver driver = null;
        string url = null;


        // Teszt előtti erőforrás beállítás    

        public LoginTest()
        {

            driver = new ChromeDriver();
            url = "http://localhost:5173/";
        }


        [Fact]
        public void A_SikertelenLogin()
        {

            driver.Navigate().GoToUrl(url);


            // belépés gomb megnyomása a logi oldal előtt
            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));

            belepesButton.Click();

            // elem kikeresése
            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));

            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));

            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));



            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("test@gmail.com");
                passwordTextBox.SendKeys("Test123445");
                loginButton.Click();
            }
            // adding an implicit wait of 20 secs
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

            IWebElement userDivElement = driver.FindElement(By.Id("login-error"));


            Assert.Equal("Hibás email vagy jelszó.", userDivElement.Text);

        }

        [Fact]
        public void B_SikeresAdminLogin()
        {
            driver.Navigate().GoToUrl(url);


            // belépés gomb megnyomása a logi oldal előtt
            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));

            belepesButton.Click();

            // elem kikeresése
            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));

            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));

            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));



            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("admin@szerszam.hu");
                passwordTextBox.SendKeys("Admin123");
                loginButton.Click();
            }
            // adding an implicit wait of 20 secs
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            IWebElement userButton = driver.FindElement(By.Id("user-menu-button"));

            Assert.Contains("admin@szerszam.hu", userButton.Text);

        }


        [Fact]
        public void C_SikeresUserLogin()
        {
            driver.Navigate().GoToUrl(url);


            // belépés gomb megnyomása a logi oldal előtt
            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));

            belepesButton.Click();

            // elem kikeresése
            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));

            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));

            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));



            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("test@gmail.com");
                passwordTextBox.SendKeys("Teszt123456@");
                loginButton.Click();
            }
            // adding an implicit wait of 20 secs
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            IWebElement userButton = driver.FindElement(By.Id("user-menu-button"));

            Assert.Contains("test@gmail.com", userButton.Text);

        }




        // Teszt utáni erőforrás felszabadítás
        public void Dispose()
        {
            driver.Quit();
        }
    }

    public class EszkozFoglalasTest : IDisposable  //***
    {
        // Initiate Webdriver
        LoginAction login = null;
        IWebDriver driver = null;
        string url = null;
        private WebDriverWait wait;
        private IJavaScriptExecutor js;


        // Teszt előtti erőforrás beállítás    
        public EszkozFoglalasTest()
        {
            LoginAction MyLogin = new LoginAction();
            login = MyLogin;
            driver = MyLogin.driver;
            url = MyLogin.url;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            js = (IJavaScriptExecutor)driver;
        }







        [Fact]
        public void SikeresFoglalas()
        {
            login.Login();

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            wait.PollingInterval = TimeSpan.FromMilliseconds(300);
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            // 1. Foglalas gomb
            wait.Until(d => d.FindElements(By.ClassName("btn-foglalas")).Count > 0);
            IWebElement eszkozItem = driver.FindElements(By.ClassName("btn-foglalas")).First();
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", eszkozItem);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", eszkozItem);
            Thread.Sleep(1000);

            // 2. Modal ellenőrzés
            Assert.True(driver.FindElements(By.ClassName("modal-overlay")).Count > 0, "Modal nem nyílt meg!");

            // 3. Dátum
            var datumGombok = wait.Until(d => {
                var btns = d.FindElements(By.ClassName("date-btn"));
                return btns.Count > 0 ? btns : null;
            });
            js.ExecuteScript("arguments[0].click();", datumGombok.First());

            // 4. Perc
            var percGombok = wait.Until(d => {
                var btns = d.FindElements(By.ClassName("minute-btn"));
                return btns.Count > 0 ? btns : null;
            });
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", percGombok.First());
            js.ExecuteScript("arguments[0].click();", percGombok.First());

            // 5. Óra
            IWebElement oraElement = wait.Until(d => d.FindElement(By.ClassName("time-select")));
            new SelectElement(oraElement).SelectByIndex(0);

            // 6. Submit
            IWebElement submitBtn = wait.Until(d => {
                var btn = d.FindElement(By.Id("foglalas-veglegesitese"));
                return (btn.Enabled && btn.Displayed) ? btn : null;
            });
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", submitBtn);
            js.ExecuteScript("arguments[0].click();", submitBtn);

            // 7. Foglalva gomb megjelenik
            IWebElement foglalvaBtn = wait.Until(d => {
                var btn = d.FindElement(By.Id("foglalas-veglegesitese"));
                return btn.Text.Contains("Foglalva") ? btn : null;
            });
            Assert.Contains("Foglalva", foglalvaBtn.Text);

            // 8. Modal automatikusan bezárul (5mp Vue + animáció)
            var closeWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            closeWait.Until(d => d.FindElements(By.ClassName("modal-overlay")).Count == 0);
            Assert.Empty(driver.FindElements(By.ClassName("modal-overlay")));
        }

        private void FillInput(string id, string value)
        {
            IWebElement el = wait.Until(d => d.FindElement(By.Id(id)));
            el.Clear();
            el.SendKeys(value);
        }






        //Assert.Equal("Hibás email vagy jelszó.", item.Text);
        // Teszt utáni erőforrás felszabadítás
        public void Dispose()
        {
            driver.Quit();
        }
    }
}