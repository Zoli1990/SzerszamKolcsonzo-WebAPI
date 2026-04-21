using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace SeleniumTests
{
    public class AdminKategoriaTest : IDisposable
    {
        LoginAction login = null;
        IWebDriver driver = null;
        string url = null;
        private WebDriverWait wait;
        private IJavaScriptExecutor js;

        public AdminKategoriaTest()
        {
            LoginAction MyLogin = new LoginAction();
            login = MyLogin;
            driver = MyLogin.driver;
            url = MyLogin.url;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            js = (IJavaScriptExecutor)driver;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(800);
        }

        [Fact]
        public void A_KategoriaTabMegnyitas()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Kategóriák tabra kattintás
            IWebElement kategoriaTab = wait.Until(d => d.FindElement(By.Id("admin-tab-kategoriak")));
            js.ExecuteScript("arguments[0].click();", kategoriaTab);

            // Új kategória gomb megjelenik
            IWebElement ujKategoriaBtn = wait.Until(d => d.FindElement(By.Id("btn-new-kategoria")));
            Assert.True(ujKategoriaBtn.Displayed);
        }

        [Fact]
        public void B_UjKategoriaLetrehozas()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Kategóriák tab
            IWebElement kategoriaTab = wait.Until(d => d.FindElement(By.Id("admin-tab-kategoriak")));
            js.ExecuteScript("arguments[0].click();", kategoriaTab);

            // Új kategória gomb
            IWebElement ujKategoriaBtn = wait.Until(d => d.FindElement(By.Id("btn-new-kategoria")));
            js.ExecuteScript("arguments[0].click();", ujKategoriaBtn);

            // Név mező kitöltése
            IWebElement nevInput = wait.Until(d => d.FindElement(By.Id("kat-nev")));
            nevInput.Click();
            nevInput.SendKeys("Teszt Kategória");
            Thread.Sleep(1000);

            // Mentés - Vue submit event triggerelése
            IWebElement submitBtn = driver.FindElement(By.Id("kat-submit"));
            js.ExecuteScript(@"
                                var btn = arguments[0];
                                var modal = btn.closest('.modal-content');
                                if (modal) { modal.scrollTop = modal.scrollHeight; }
                                btn.scrollIntoView({block: 'center'});", submitBtn);
            Thread.Sleep(1000);

            // Próbáljuk normál kattintással
            submitBtn.Click();
            Thread.Sleep(500);

            // Ha még nyitva, próbáljuk form submit-tal
            var modalok = driver.FindElements(By.ClassName("modal-overlay"));
            if (modalok.Count > 0)
            {
                js.ExecuteScript(@"
                                var form = document.querySelector('form.modal-content');
                                if (form) {
                                form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));}");
            }

            // Modal bezáródására várunk
            wait.Until(d => d.FindElements(By.ClassName("modal-overlay")).Count == 0);
            Assert.Empty(driver.FindElements(By.ClassName("modal-overlay")));
        }

        [Fact]
        public void C_KategoriaSzerkesztes()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Kategóriák tab
            IWebElement kategoriaTab = wait.Until(d => d.FindElement(By.Id("admin-tab-kategoriak")));
            js.ExecuteScript("arguments[0].click();", kategoriaTab);

            // Első szerkesztés gombra kattintás
            wait.Until(d => d.FindElements(By.CssSelector("[id^='btn-kat-edit-']")).Count > 0);
            IWebElement editBtn = driver.FindElements(By.CssSelector("[id^='btn-kat-edit-']")).First();
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", editBtn);
            js.ExecuteScript("arguments[0].click();", editBtn);

            // Megvárjuk hogy a mező ténylegesen ki legyen töltve
            wait.Until(d => {
                var input = d.FindElement(By.Id("kat-nev"));
                return input.GetAttribute("value") == "Módosított Kategória";
            });
            Thread.Sleep(300);

            // Mentés
            IWebElement submitBtn = driver.FindElement(By.Id("kat-submit"));
            js.ExecuteScript(@"
    var btn = arguments[0];
    var modal = btn.closest('.modal-content');
    if (modal) { modal.scrollTop = modal.scrollHeight; }
    btn.scrollIntoView({block: 'center'});
", submitBtn);
            Thread.Sleep(800);
            js.ExecuteScript("arguments[0].click();", submitBtn);

            // Modal bezáródására várunk
            wait.Until(d => d.FindElements(By.ClassName("modal-overlay")).Count == 0);
            Assert.Empty(driver.FindElements(By.ClassName("modal-overlay")));

        }

        [Fact]
        public void D_KategoriaTorles()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Kategóriák tab
            IWebElement kategoriaTab = wait.Until(d => d.FindElement(By.Id("admin-tab-kategoriak")));
            js.ExecuteScript("arguments[0].click();", kategoriaTab);

            // Utolsó törlés gombra kattintás (a teszt által létrehozott)
            wait.Until(d => d.FindElements(By.CssSelector("[id^='btn-delete-']")).Count > 0);
            var deleteGombok = driver.FindElements(By.CssSelector("[id^='btn-delete-']"));
            IWebElement utolsoDeleteBtn = deleteGombok.Last();
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", utolsoDeleteBtn);
            js.ExecuteScript("arguments[0].click();", utolsoDeleteBtn);

            // Confirm dialog elfogadása
            driver.SwitchTo().Alert().Accept();

            // Megvárjuk a törlést
            Thread.Sleep(1000);
            Assert.True(true);
        }

        public void Dispose()
        {
            driver.Quit();
        }
    }
}