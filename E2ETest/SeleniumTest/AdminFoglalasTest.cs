using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace SeleniumTests
{
    public class AdminFoglalasTest : IDisposable
    {
        LoginAction login = null;
        IWebDriver driver = null;
        string url = null;
        private WebDriverWait wait;
        private IJavaScriptExecutor js;

        public AdminFoglalasTest()
        {
            LoginAction MyLogin = new LoginAction();
            login = MyLogin;
            driver = MyLogin.driver;
            url = MyLogin.url;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));
            js = (IJavaScriptExecutor)driver;

            // Lassítás - látványosabb futás
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(1000);
        }

        [Fact]
        public void A_FoglalasokTabMegnyitas()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Foglalások tabra kattintás
            IWebElement foglalasokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-foglalasok")));
            js.ExecuteScript("arguments[0].click();", foglalasokTab);

            // Megvárjuk hogy betöltődjön
            wait.Until(d => d.FindElement(By.Id("foglalas-filter-all")));
            Assert.True(driver.FindElement(By.Id("foglalas-filter-all")).Displayed);
        }

        [Fact]
        public void B_FoglalasSzuroGombok()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Foglalások tab
            IWebElement foglalasokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-foglalasok")));
            js.ExecuteScript("arguments[0].click();", foglalasokTab);

            wait.Until(d => d.FindElement(By.Id("foglalas-filter-all")));

            // Összes
            IWebElement osszes = driver.FindElement(By.Id("foglalas-filter-all"));
            js.ExecuteScript("arguments[0].click();", osszes);
            Thread.Sleep(300);
            Assert.True(osszes.GetAttribute("class").Contains("active"));

            // Foglalva
            IWebElement foglalva = driver.FindElement(By.Id("foglalas-filter-Foglalva"));
            js.ExecuteScript("arguments[0].click();", foglalva);
            Thread.Sleep(300);
            Assert.True(foglalva.GetAttribute("class").Contains("active"));

            // Kiadva
            IWebElement kiadva = driver.FindElement(By.Id("foglalas-filter-Kiadva"));
            js.ExecuteScript("arguments[0].click();", kiadva);
            Thread.Sleep(300);
            Assert.True(kiadva.GetAttribute("class").Contains("active"));

            // Lezárt
            IWebElement lezart = driver.FindElement(By.Id("foglalas-filter-Lezart"));
            js.ExecuteScript("arguments[0].click();", lezart);
            Thread.Sleep(300);
            Assert.True(lezart.GetAttribute("class").Contains("active"));

            // Visszaállítás
            js.ExecuteScript("arguments[0].click();", osszes);
            Thread.Sleep(300);
            Assert.True(osszes.GetAttribute("class").Contains("active"));
        }

        [Fact]
        public void C_FoglalasReszletek()
        {
            login.Login();

            driver.Navigate().GoToUrl(url + "admin");

            // Foglalások tab
            IWebElement foglalasokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-foglalasok")));
            js.ExecuteScript("arguments[0].click();", foglalasokTab);

            // Megvárjuk hogy legyen legalább egy foglalás
            wait.Until(d => d.FindElements(By.CssSelector("[id^='btn-info-']")).Count > 0);

            // Első részletek gombra kattintás
            IWebElement infoBtn = driver.FindElements(By.CssSelector("[id^='btn-info-']")).First();
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", infoBtn);
            js.ExecuteScript("arguments[0].click();", infoBtn);

            // Modal megnyílt - megvárjuk hogy ténylegesen látható legyen
            wait.Until(d => {
                var modal = d.FindElements(By.ClassName("modal-overlay"));
                return modal.Count > 0;
            });
            Assert.True(driver.FindElements(By.ClassName("modal-overlay")).Count > 0);

            // Modal bezárása
            IWebElement closeBtn = driver.FindElement(By.ClassName("btn-close"));
            js.ExecuteScript("arguments[0].click();", closeBtn);

            wait.Until(d => d.FindElements(By.ClassName("modal-overlay")).Count == 0);
            Assert.Empty(driver.FindElements(By.ClassName("modal-overlay")));
        }

        public void Dispose()
        {
            driver.Quit();
        }
    }
}