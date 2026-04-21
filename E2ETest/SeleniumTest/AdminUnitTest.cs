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
        public IWebDriver driver = null;
        public string url = null;

        public LoginAction()
        {
            driver = new ChromeDriver();
            url = "http://localhost:5173/";
            //url = "https://szerszamkolcsonzo.tryasp.net";
        }

        public void Login()
        {
            driver.Navigate().GoToUrl(url);

            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));
            belepesButton.Click();

            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));
            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));
            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));

            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("admin@szerszam.hu");
                passwordTextBox.SendKeys("Admin123");
                loginButton.Click();
            }

            WebDriverWait loginWait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            loginWait.Until(d => d.FindElement(By.Id("user-menu-button")));
        }

        public void Dispose()
        {
            driver.Quit();
        }
    };


    public class LoginTest : IDisposable
    {
        IWebDriver driver = null;
        string url = null;

        public LoginTest()
        {
            driver = new ChromeDriver();
            url = "http://localhost:5173/";
            //url = "https://szerszamkolcsonzo.tryasp.net";
        }

        [Fact]
        public void A_SikertelenLogin()
        {
            driver.Navigate().GoToUrl(url);

            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));
            belepesButton.Click();

            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));
            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));
            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));

            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("admin@szerszam.hu");
                passwordTextBox.SendKeys("RosszJelszo123");
                loginButton.Click();
            }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);

            IWebElement userDivElement = driver.FindElement(By.Id("login-error"));

            Assert.Equal("Hibás email vagy jelszó.", userDivElement.Text);
        }

        [Fact]
        public void B_SikeresAdminLogin()
        {
            driver.Navigate().GoToUrl(url);

            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));
            belepesButton.Click();

            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));
            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));
            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));

            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("admin@szerszam.hu");
                passwordTextBox.SendKeys("Admin123");
                loginButton.Click();
            }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            IWebElement userButton = driver.FindElement(By.Id("user-menu-button"));

            Assert.Contains("admin@szerszam.hu", userButton.Text);
        }

        [Fact]
        public void C_SikeresUserLogin()
        {
            driver.Navigate().GoToUrl(url);

            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));
            belepesButton.Click();

            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));
            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));
            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));

            if (emailTextBox != null && passwordTextBox != null && loginButton != null)
            {
                emailTextBox.SendKeys("admin@szerszam.hu");
                passwordTextBox.SendKeys("Admin123");
                loginButton.Click();
            }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            IWebElement userButton = driver.FindElement(By.Id("user-menu-button"));

            Assert.Contains("admin@szerszam.hu", userButton.Text);
        }

        public void Dispose()
        {
            driver.Quit();
        }
    }


    public class EszkozFoglalasTest : IDisposable
    {
        LoginAction login = null;
        IWebDriver driver = null;
        string url = null;
        private WebDriverWait wait;
        private IJavaScriptExecutor js;

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

            // 1. Foglalás gomb
            wait.Until(d => d.FindElements(By.ClassName("btn-foglalas")).Count > 0);
            IWebElement eszkozItem = driver.FindElements(By.ClassName("btn-foglalas")).First();
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", eszkozItem);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", eszkozItem);
            Thread.Sleep(1000);

            // 2. Modal ellenőrzés
            Assert.True(driver.FindElements(By.ClassName("modal-overlay")).Count > 0, "Modal nem nyílt meg!");

            // 3. Form kitöltése
            IWebElement nevInput = wait.Until(d => d.FindElement(By.Id("nev")));
            nevInput.Clear();
            nevInput.SendKeys("Teszt Felhasználó");

            driver.FindElement(By.Id("email")).Clear();
            driver.FindElement(By.Id("email")).SendKeys("teszt@teszt.hu");

            driver.FindElement(By.Id("telefonszam")).Clear();
            driver.FindElement(By.Id("telefonszam")).SendKeys("+36301234567");

            driver.FindElement(By.Id("cim")).Clear();
            driver.FindElement(By.Id("cim")).SendKeys("1234 Budapest, Teszt utca 1.");

            // 4. Dátum választó - első elérhető nap
            var datumGombok = wait.Until(d => {
                var btns = d.FindElements(By.ClassName("date-btn"));
                return btns.Count > 0 ? btns : null;
            });
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", datumGombok.First());
            js.ExecuteScript("arguments[0].click();", datumGombok.First());
            Thread.Sleep(300);

            // 5. Óra select
            IWebElement oraElement = wait.Until(d => d.FindElement(By.Id("foglalas-ora")));
            new SelectElement(oraElement).SelectByIndex(0);
            Thread.Sleep(300);

            // 6. Perc select
            IWebElement percElement = wait.Until(d => d.FindElement(By.Id("foglalas-perc")));
            new SelectElement(percElement).SelectByIndex(0);
            Thread.Sleep(300);

            // 7. ÁSZF jelölőnégyzet
            IWebElement aszfCheckbox = wait.Until(d => d.FindElement(By.ClassName("aszf-checkbox")));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", aszfCheckbox);
            Thread.Sleep(300);
            js.ExecuteScript("arguments[0].click();", aszfCheckbox);
            Thread.Sleep(300);

            // 8. Submit gomb
            IWebElement submitBtn = wait.Until(d => {
                var btn = d.FindElement(By.Id("foglalas-veglegesitese"));
                return (btn.Enabled && btn.Displayed) ? btn : null;
            });
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", submitBtn);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", submitBtn);

            // 9. Foglalva gomb megjelenik
            IWebElement foglalvaBtn = wait.Until(d => {
                var btn = d.FindElement(By.Id("foglalas-veglegesitese"));
                return btn.Text.Contains("Foglalva") ? btn : null;
            });
            Assert.Contains("Foglalva", foglalvaBtn.Text);

            // 10. Modal automatikusan bezárul
            var closeWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            closeWait.Until(d => d.FindElements(By.ClassName("modal-overlay")).Count == 0);
            Assert.Empty(driver.FindElements(By.ClassName("modal-overlay")));
        }

        public void Dispose()
        {
            driver.Quit();
        }
    }
}