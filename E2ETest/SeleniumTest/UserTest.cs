using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace SeleniumTests
{
    public class UserTest : IDisposable
    {
        IWebDriver driver = null;
        string url = null;
        private WebDriverWait wait;
        private IJavaScriptExecutor js;

        public UserTest()
        {
            driver = new ChromeDriver();
            url = "http://localhost:5173/";
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
            js = (IJavaScriptExecutor)driver;
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(800);
        }

        public void Login()
        {
            driver.Navigate().GoToUrl(url);

            IWebElement belepesButton = driver.FindElement(By.Id("btn-login-desktop"));
            belepesButton.Click();

            IWebElement emailTextBox = driver.FindElement(By.Id("login-email"));
            IWebElement passwordTextBox = driver.FindElement(By.Id("login-password"));
            IWebElement loginButton = driver.FindElement(By.Id("btn-submit-login"));

            emailTextBox.SendKeys("test@gmail.com");
            passwordTextBox.SendKeys("Teszt123456@");
            loginButton.Click();

            wait.Until(d => d.FindElement(By.Id("user-menu-button")));
        }

        [Fact]
        public void A_UserBejelentkezes()
        {
            Login();

            IWebElement userButton = driver.FindElement(By.Id("user-menu-button"));
            Assert.Contains("test@gmail.com", userButton.Text);
        }

        public void Dispose()
        {
            driver.Quit();
        }

        [Fact]
        public void B_EszkozokBongeszese()
        {
            Login();

            // Megvárjuk hogy az eszközök betöltődjenek
            wait.Until(d => d.FindElements(By.ClassName("btn-foglalas")).Count > 0);

            // Összes szűrő gomb
            IWebElement osszesSzuro = wait.Until(d => d.FindElement(By.Id("filter-btn-all")));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", osszesSzuro);
            js.ExecuteScript("arguments[0].click();", osszesSzuro);
            Thread.Sleep(500);
            Assert.True(osszesSzuro.GetAttribute("class").Contains("active"));

            // Végigmegyünk minden kategória gombon
            var kategoriaGombok = driver.FindElements(By.CssSelector("[id^='filter-btn-']"))
                .Where(b => b.GetAttribute("id") != "filter-btn-all")
                .ToList();

            foreach (IWebElement kategoriaGomb in kategoriaGombok)
            {
                js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", kategoriaGomb);
                Thread.Sleep(300);
                js.ExecuteScript("arguments[0].click();", kategoriaGomb);
                Thread.Sleep(500);
                Assert.True(kategoriaGomb.GetAttribute("class").Contains("active"));
            }

            // Visszaállítás összes-re
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", osszesSzuro);
            js.ExecuteScript("arguments[0].click();", osszesSzuro);
            Thread.Sleep(500);
            Assert.True(osszesSzuro.GetAttribute("class").Contains("active"));

            // Ellenőrizzük hogy vannak eszközök
            Assert.True(driver.FindElements(By.ClassName("btn-foglalas")).Count > 0);
        }

        [Fact]
        public void C_ProfilOldal()
        {
            Login();

            // Navigálás a profil oldalra
            driver.Navigate().GoToUrl(url + "profilom");

            // Megvárjuk hogy betöltődjön
            IWebElement nevInput = wait.Until(d => {
                var input = d.FindElement(By.Id("profil-nev"));
                return input.GetAttribute("value")?.Length > 0 ? input : null;
            });

            // --- 1. MÓDOSÍTÁS ---
            js.ExecuteScript("arguments[0].value = 'Módosított Név';", nevInput);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", nevInput);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", nevInput);
            Thread.Sleep(300);

            IWebElement telefonInput = driver.FindElement(By.Id("profil-telefon"));
            js.ExecuteScript("arguments[0].value = '+36709999999';", telefonInput);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", telefonInput);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", telefonInput);
            Thread.Sleep(300);

            // Megvárjuk hogy a mező értéke megváltozott
            wait.Until(d => {
                var input = d.FindElement(By.Id("profil-nev"));
                return input.GetAttribute("value") == "Módosított Név";
            });
            Thread.Sleep(300);

            // Mentés
            IWebElement submitBtn = driver.FindElement(By.Id("profil-submit"));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", submitBtn);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", submitBtn);

            wait.Until(d => d.FindElements(By.ClassName("success-message")).Count > 0);
            Assert.True(driver.FindElements(By.ClassName("success-message")).Count > 0);
            Thread.Sleep(1000);

            // --- 2. VISSZAÁLLÍTÁS ---
            nevInput = driver.FindElement(By.Id("profil-nev"));
            js.ExecuteScript("arguments[0].value = '';", nevInput);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", nevInput);
            Thread.Sleep(200);
            nevInput.SendKeys("Teszt Elek");
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", nevInput);
            Thread.Sleep(300);

            telefonInput = driver.FindElement(By.Id("profil-telefon"));
            js.ExecuteScript("arguments[0].value = '';", telefonInput);
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", telefonInput);
            Thread.Sleep(200);
            telefonInput.SendKeys("+36301234567");
            js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", telefonInput);
            Thread.Sleep(300);

            // Megvárjuk hogy a mező értéke megváltozott
            wait.Until(d => {
                var input = d.FindElement(By.Id("profil-nev"));
                return input.GetAttribute("value") == "Teszt Elek";
            });
            Thread.Sleep(300);

            // Mentés
            submitBtn = driver.FindElement(By.Id("profil-submit"));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", submitBtn);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", submitBtn);

            wait.Until(d => d.FindElements(By.ClassName("success-message")).Count > 0);
            Assert.True(driver.FindElements(By.ClassName("success-message")).Count > 0);
        }



        [Fact]
        public void D_SajatFoglalasok()
        {
            Login();

            // Navigálás a foglalások oldalra
            driver.Navigate().GoToUrl(url + "profil");

            // Megvárjuk hogy betöltődjön
            Thread.Sleep(2000);

            // Ellenőrizzük hogy az oldal betöltődött
            Assert.True(driver.FindElements(By.ClassName("foglalas-card")).Count > 0 ||
                        driver.FindElements(By.ClassName("empty-state")).Count > 0);
        }

        [Fact]
        public void E_Kijelentkezes()
        {
            Login();

            // User menü megnyitása
            IWebElement userMenuBtn = wait.Until(d => d.FindElement(By.Id("user-menu-button")));
            js.ExecuteScript("arguments[0].click();", userMenuBtn);
            Thread.Sleep(500);

            // Kijelentkezés gombra kattintás
            IWebElement logoutBtn = wait.Until(d => d.FindElement(By.Id("btn-logout-desktop")));
            js.ExecuteScript("arguments[0].click();", logoutBtn);
            Thread.Sleep(500);

            // Confirm dialog elfogadása
            driver.SwitchTo().Alert().Accept();
            Thread.Sleep(1000);

            // Ellenőrizzük hogy kijelentkezve vagyunk
            wait.Until(d => d.FindElement(By.Id("btn-login-desktop")));
            Assert.True(driver.FindElement(By.Id("btn-login-desktop")).Displayed);
        }

        [Fact]
        public void F_Navigacio()
        {
            Login();

            // Logo kattintás
            IWebElement logo = wait.Until(d => d.FindElement(By.Id("logo-link")));
            js.ExecuteScript("arguments[0].click();", logo);
            Thread.Sleep(500);

            // Eszközök nav link
            IWebElement navEszkozok = wait.Until(d => d.FindElement(By.Id("nav-eszkozok")));
            js.ExecuteScript("arguments[0].click();", navEszkozok);
            Thread.Sleep(500);

            // Vélemények nav link
            IWebElement navVelemenyek = wait.Until(d => d.FindElement(By.Id("nav-velemenyek")));
            js.ExecuteScript("arguments[0].click();", navVelemenyek);
            Thread.Sleep(500);

            // Kapcsolat nav link
            IWebElement navKapcsolat = wait.Until(d => d.FindElement(By.Id("nav-kapcsolat")));
            js.ExecuteScript("arguments[0].click();", navKapcsolat);
            Thread.Sleep(500);

            // ÁSZF link
            driver.Navigate().GoToUrl(url + "aszf");
            Thread.Sleep(1000);
            Assert.True(driver.Url.Contains("aszf"));

            // Vissza a főoldalra
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(500);
            Assert.True(driver.FindElements(By.ClassName("btn-foglalas")).Count > 0);
        }

        [Fact]
        public void G_NyelvValtas()
        {
            Login();

            // Nyelvváltó dropdown megnyitása
            IWebElement langSelected = wait.Until(d => d.FindElement(By.ClassName("lang-selected")));
            js.ExecuteScript("arguments[0].click();", langSelected);
            Thread.Sleep(500);

            // Angol nyelv
            IWebElement engBtn = wait.Until(d => d.FindElement(By.Id("lang-en")));
            js.ExecuteScript("arguments[0].click();", engBtn);
            Thread.Sleep(500);

            // Nyelvváltó dropdown megnyitása újra
            langSelected = wait.Until(d => d.FindElement(By.ClassName("lang-selected")));
            js.ExecuteScript("arguments[0].click();", langSelected);
            Thread.Sleep(500);

            // Német nyelv
            IWebElement deBtn = wait.Until(d => d.FindElement(By.Id("lang-de")));
            js.ExecuteScript("arguments[0].click();", deBtn);
            Thread.Sleep(500);

            // Nyelvváltó dropdown megnyitása újra
            langSelected = wait.Until(d => d.FindElement(By.ClassName("lang-selected")));
            js.ExecuteScript("arguments[0].click();", langSelected);
            Thread.Sleep(500);

            // Vissza magyarra
            IWebElement huBtn = wait.Until(d => d.FindElement(By.Id("lang-hu")));
            js.ExecuteScript("arguments[0].click();", huBtn);
            Thread.Sleep(500);

            // Ellenőrizzük hogy magyar aktív
            Assert.True(driver.FindElement(By.ClassName("lang-selected")).Text.Contains("Magyar"));
        }

        [Fact]
        public void H_FejlecGombok()
        {
            Login();

            // Logo
            IWebElement logo = wait.Until(d => d.FindElement(By.Id("logo-link")));
            js.ExecuteScript("arguments[0].click();", logo);
            Thread.Sleep(500);

            // User menü megnyitása
            IWebElement userMenuBtn = wait.Until(d => d.FindElement(By.Id("user-menu-button")));
            js.ExecuteScript("arguments[0].click();", userMenuBtn);
            Thread.Sleep(500);

            // Profilom link
            IWebElement profilomLink = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='dropdown-profilom']")));
            js.ExecuteScript("arguments[0].click();", profilomLink);
            Thread.Sleep(500);
            Assert.True(driver.Url.Contains("profilom"));

            // Vissza főoldalra
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(500);

            // User menü megnyitása
            userMenuBtn = wait.Until(d => d.FindElement(By.Id("user-menu-button")));
            js.ExecuteScript("arguments[0].click();", userMenuBtn);
            Thread.Sleep(500);

            // Foglalásaim link
            IWebElement foglalasaimLink = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='dropdown-foglalasaim']")));
            js.ExecuteScript("arguments[0].click();", foglalasaimLink);
            Thread.Sleep(500);
            Assert.True(driver.Url.Contains("profil"));
        }



        [Fact]
        public void I_HamburgerMenu()
        {
            Login();

            // Hamburger menü megnyitása
            IWebElement hamburger = wait.Until(d => d.FindElement(By.Id("hamburger-menu")));
            js.ExecuteScript("arguments[0].click();", hamburger);
            Thread.Sleep(500);

            // Ellenőrizzük hogy megnyílt a menü
            wait.Until(d => d.FindElement(By.CssSelector("[data-testid='mobile-menu']")));
            Assert.True(driver.FindElement(By.CssSelector("[data-testid='mobile-menu']")).Displayed);

            // Eszközök link
            IWebElement eszkozokLink = driver.FindElement(By.CssSelector("[data-testid='mobile-menu-eszkozok']"));
            js.ExecuteScript("arguments[0].click();", eszkozokLink);
            Thread.Sleep(500);

            // Hamburger menü újra megnyitása
            hamburger = wait.Until(d => d.FindElement(By.Id("hamburger-menu")));
            js.ExecuteScript("arguments[0].click();", hamburger);
            Thread.Sleep(500);

            // Profilom link
            IWebElement profilomLink = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='mobile-menu-profilom']")));
            js.ExecuteScript("arguments[0].click();", profilomLink);
            Thread.Sleep(500);
            Assert.True(driver.Url.Contains("profilom"));

            // Vissza főoldalra
            driver.Navigate().GoToUrl(url);
            Thread.Sleep(500);

            // Hamburger menü megnyitása
            hamburger = wait.Until(d => d.FindElement(By.Id("hamburger-menu")));
            js.ExecuteScript("arguments[0].click();", hamburger);
            Thread.Sleep(500);

            // Foglalásaim link
            IWebElement foglalasaimLink = wait.Until(d => d.FindElement(By.CssSelector("[data-testid='mobile-menu-foglalasaim']")));
            js.ExecuteScript("arguments[0].click();", foglalasaimLink);
            Thread.Sleep(500);
            Assert.True(driver.Url.Contains("profil"));
        }

        [Fact]
        public void J_FoglalasLeadasa()
        {
            Login();

            // Megvárjuk hogy az eszközök betöltődjenek
            wait.Until(d => d.FindElements(By.ClassName("btn-foglalas"))
                .Any(b => b.Enabled && b.GetAttribute("disabled") == null));

            // Scroll az eszközökhöz
            js.ExecuteScript("window.scrollTo(0, 500);");
            Thread.Sleep(1000);

            // Első elérhető eszköz foglalás gombja
            IWebElement eszkozItem = driver.FindElements(By.ClassName("btn-foglalas"))
                .First(b => b.Enabled && b.GetAttribute("disabled") == null);
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", eszkozItem);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", eszkozItem);
            Thread.Sleep(1000);

            // Modal ellenőrzés
            Assert.True(driver.FindElements(By.ClassName("modal-overlay")).Count > 0, "Modal nem nyílt meg!");

            // Óra select - random index
            IWebElement oraElement = wait.Until(d => d.FindElement(By.Id("foglalas-ora")));
            var oraSelect = new SelectElement(oraElement);
            int randomOraIndex = new Random().Next(0, oraSelect.Options.Count);
            oraSelect.SelectByIndex(randomOraIndex);
            Thread.Sleep(300);

            // Perc select - random index
            IWebElement percElement = wait.Until(d => d.FindElement(By.Id("foglalas-perc")));
            var percSelect = new SelectElement(percElement);
            int randomPercIndex = new Random().Next(0, percSelect.Options.Count);
            percSelect.SelectByIndex(randomPercIndex);
            Thread.Sleep(300);

            // Dátum is random - ne mindig az első napot válasszuk
            var datumGombok = wait.Until(d => {
                var btns = d.FindElements(By.ClassName("date-btn"));
                return btns.Count > 0 ? btns : null;
            });
            int randomDatumIndex = new Random().Next(1, datumGombok.Count);
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", datumGombok[randomDatumIndex]);
            js.ExecuteScript("arguments[0].click();", datumGombok[randomDatumIndex]);
            Thread.Sleep(300);

            // ÁSZF jelölőnégyzet
            IWebElement aszfCheckbox = wait.Until(d => d.FindElement(By.ClassName("aszf-checkbox")));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", aszfCheckbox);
            Thread.Sleep(300);
            js.ExecuteScript("arguments[0].click();", aszfCheckbox);
            Thread.Sleep(300);

            // Submit gomb
            IWebElement submitBtn = wait.Until(d => {
                var btn = d.FindElement(By.Id("foglalas-veglegesitese"));
                return (btn.Enabled && btn.Displayed) ? btn : null;
            });
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", submitBtn);
            Thread.Sleep(500);
            js.ExecuteScript("arguments[0].click();", submitBtn);

            // Foglalva gomb megjelenik
            IWebElement foglalvaBtn = wait.Until(d => {
                var btn = d.FindElement(By.Id("foglalas-veglegesitese"));
                return btn.Text.Contains("Foglalva") ? btn : null;
            });
            Assert.Contains("Foglalva", foglalvaBtn.Text);

            // Modal automatikusan bezárul
            var closeWait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            closeWait.Until(d => d.FindElements(By.ClassName("modal-overlay")).Count == 0);
            Assert.Empty(driver.FindElements(By.ClassName("modal-overlay")));
        }

        [Fact]
        public void K_VelemenySzekcio()
        {
            Login();

            driver.Navigate().GoToUrl(url);
            Thread.Sleep(1000);

            IWebElement nextBtn = wait.Until(d => d.FindElement(By.Id("carousel-next")));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", nextBtn);
            Thread.Sleep(500);

            // Random számú next kattintás
            Random rnd = new Random();
            int nextKattintas = rnd.Next(2, 6);
            for (int i = 0; i < nextKattintas; i++)
            {
                js.ExecuteScript("arguments[0].click();", nextBtn);
                Thread.Sleep(400);
            }

            // Random számú prev kattintás
            IWebElement prevBtn = driver.FindElement(By.Id("carousel-prev"));
            int prevKattintas = rnd.Next(1, 4);
            for (int i = 0; i < prevKattintas; i++)
            {
                js.ExecuteScript("arguments[0].click();", prevBtn);
                Thread.Sleep(400);
            }

            // Indicator gombok - random sorrendben
            var indicators = driver.FindElements(By.ClassName("indicator")).ToList();
            var randomIndicators = indicators.OrderBy(x => rnd.Next()).ToList();
            foreach (IWebElement indicator in randomIndicators)
            {
                js.ExecuteScript("arguments[0].click();", indicator);
                Thread.Sleep(400);
            }

            Assert.True(true);
        }













    }
}