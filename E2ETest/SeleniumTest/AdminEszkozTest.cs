using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumTests;

public class AdminEszkozTest : IDisposable
{
    LoginAction login = null;
    IWebDriver driver = null;
    string url = null;
    private WebDriverWait wait;
    private IJavaScriptExecutor js;

    public AdminEszkozTest()
    {
        LoginAction MyLogin = new LoginAction();
        login = MyLogin;
        driver = MyLogin.driver;
        url = MyLogin.url;
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
        js = (IJavaScriptExecutor)driver;
    }

    [Fact]
    public void A_AdminEszkozTabMegnyitas()
    {
        login.Login();

        wait.Until(d => d.FindElement(By.Id("user-menu-button")));

        driver.Navigate().GoToUrl(url + "admin");

        IWebElement eszkozokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-eszkozok")));
        js.ExecuteScript("arguments[0].click();", eszkozokTab);

        IWebElement ujEszkozBtn = wait.Until(d => d.FindElement(By.Id("btn-new-eszkoz")));
        Assert.True(ujEszkozBtn.Displayed);
    }

    public void Dispose()
    {
        driver.Quit();
    }

    [Fact]
    public void B_AdminUjEszkozLetrehozas()
    {
        login.Login();
        wait.Until(d => d.FindElement(By.Id("user-menu-button")));

        driver.Navigate().GoToUrl(url + "admin");

        // Eszközök tab
        IWebElement eszkozokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-eszkozok")));
        js.ExecuteScript("arguments[0].click();", eszkozokTab);

        // Új eszköz gomb
        IWebElement ujEszkozBtn = wait.Until(d => d.FindElement(By.Id("btn-new-eszkoz")));
        js.ExecuteScript("arguments[0].click();", ujEszkozBtn);

        // Mezők kitöltése
        wait.Until(d => d.FindElement(By.Id("eszkoz-nev"))).SendKeys("Teszt Fúrógép");

        IWebElement kategoriaSelect = wait.Until(d => {
            var sel = d.FindElement(By.Id("eszkoz-kategoria"));
            var opts = new SelectElement(sel).Options;
            return opts.Count > 1 ? sel : null;
        });
        new SelectElement(kategoriaSelect).SelectByIndex(1);

        driver.FindElement(By.Id("eszkoz-leiras")).SendKeys("Teszt leírás");
        driver.FindElement(By.Id("eszkoz-kepurl")).SendKeys("https://cdn.hoffmann-group.com/derivatives/1433791/jpg_600/jpg_600_img-rd-295786-16.jpg");
        driver.FindElement(By.Id("eszkoz-vetelar")).SendKeys("50000");
        driver.FindElement(By.Id("eszkoz-kiadasiar")).SendKeys("1500");
        IWebElement datumInput = driver.FindElement(By.Id("eszkoz-datum"));
        js.ExecuteScript("arguments[0].value = '2026-04-17';", datumInput);
        js.ExecuteScript("arguments[0].dispatchEvent(new Event('input'));", datumInput);
        js.ExecuteScript("arguments[0].dispatchEvent(new Event('change'));", datumInput);

        // Mentés

        IWebElement submitBtn = driver.FindElement(By.Id("eszkoz-submit"));
        js.ExecuteScript(@"
            var btn = arguments[0];
            var modal = btn.closest('.modal-content');
            if (modal) { modal.scrollTop = modal.scrollHeight; }
            btn.scrollIntoView({block: 'center'});", submitBtn);
        Thread.Sleep(800);
        js.ExecuteScript("arguments[0].click();", submitBtn);


        // Sikeres mentés - toast megjelenik
        IWebElement toast = wait.Until(d => d.FindElement(By.ClassName("toast")));
        Assert.True(toast.Displayed);
    }

    [Fact]
    public void C_AdminEszkozTorles()
    {
        login.Login();

        driver.Navigate().GoToUrl(url + "admin");

        // Eszközök tab
        IWebElement eszkozokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-eszkozok")));
        js.ExecuteScript("arguments[0].click();", eszkozokTab);

        // Megvárjuk hogy betöltődjön a táblázat
        wait.Until(d => d.FindElement(By.Id("btn-new-eszkoz")));

        // Teszt eszköz törlés gombjának megkeresése
        // Az utolsó eszközt töröljük (amit az előző teszt hozott létre)
        wait.Until(d => d.FindElements(By.CssSelector("[id^='btn-delete-']")).Count > 0);
        var deleteGombok = driver.FindElements(By.CssSelector("[id^='btn-delete-']"));
        IWebElement utolsoDeleteBtn = deleteGombok.Last();

        js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", utolsoDeleteBtn);
        js.ExecuteScript("arguments[0].click();", utolsoDeleteBtn);

        // Confirm dialog elfogadása
        driver.SwitchTo().Alert().Accept();

        // Megvárjuk a törlést - toast megjelenik
        wait.Until(d => {
            var toasts = d.FindElements(By.ClassName("toast"));
            return toasts.Count > 0 && toasts[0].Displayed;
        });

        Assert.True(driver.FindElements(By.ClassName("toast")).Count > 0);
    }

    [Fact]
    public void D_AdminEszkozSzerkesztes()
    {
        login.Login();

        driver.Navigate().GoToUrl(url + "admin");

        // Eszközök tab
        IWebElement eszkozokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-eszkozok")));
        js.ExecuteScript("arguments[0].click();", eszkozokTab);

        // Megvárjuk hogy betöltődjön a táblázat
        wait.Until(d => d.FindElements(By.CssSelector("[id^='btn-edit-']")).Count > 0);

        // Első eszköz szerkesztés gombjára kattintás
        var editGombok = driver.FindElements(By.CssSelector("[id^='btn-edit-']"));
        IWebElement elsoEditBtn = editGombok.First();
        js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", elsoEditBtn);
        js.ExecuteScript("arguments[0].click();", elsoEditBtn);

        // Modal megnyílt ÉS az adatok betöltődtek
        IWebElement nevInput = wait.Until(d => {
            var input = d.FindElement(By.Id("eszkoz-nev"));
            return input.GetAttribute("value")?.Length > 0 ? input : null;
        });

        // Név mező módosítása
        nevInput.Clear();
        nevInput.SendKeys("Módosított Eszköz");

        // Kis várakozás hogy Vue frissítse a v-model-t
        Thread.Sleep(500);

        // Mentés
        IWebElement submitBtn = driver.FindElement(By.Id("eszkoz-submit"));

        // Modal content scrollozása a gombhoz
        js.ExecuteScript(@"
                var btn = arguments[0];
                var modal = btn.closest('.modal-content');
                if (modal) 
                {modal.scrollTop = modal.scrollHeight;} 
                btn.scrollIntoView({block: 'center'});", submitBtn);

        Thread.Sleep(800);
        submitBtn.Click();
    }

    [Fact]
    public void E_AdminEszkozSzuroGombok()
    {
        login.Login();

        driver.Navigate().GoToUrl(url + "admin");

        // Eszközök tab
        IWebElement eszkozokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-eszkozok")));
        js.ExecuteScript("arguments[0].click();", eszkozokTab);

        // Megvárjuk hogy betöltődjön
        wait.Until(d => d.FindElement(By.Id("filter-all")));

        // Összes gomb
        IWebElement osszesSzuro = driver.FindElement(By.Id("filter-all"));
        js.ExecuteScript("arguments[0].click();", osszesSzuro);
        Thread.Sleep(300);
        Assert.True(osszesSzuro.GetAttribute("class").Contains("active"));

        // Elérhető gomb
        IWebElement elerhetoSzuro = driver.FindElement(By.Id("filter-Elerheto"));
        js.ExecuteScript("arguments[0].click();", elerhetoSzuro);
        Thread.Sleep(300);
        Assert.True(elerhetoSzuro.GetAttribute("class").Contains("active"));

        // Foglalva gomb
        IWebElement foglalvaSzuro = driver.FindElement(By.Id("filter-Foglalva"));
        js.ExecuteScript("arguments[0].click();", foglalvaSzuro);
        Thread.Sleep(300);
        Assert.True(foglalvaSzuro.GetAttribute("class").Contains("active"));

        // Kiadva gomb
        IWebElement kiadavaSzuro = driver.FindElement(By.Id("filter-Kiadva"));
        js.ExecuteScript("arguments[0].click();", kiadavaSzuro);
        Thread.Sleep(300);
        Assert.True(kiadavaSzuro.GetAttribute("class").Contains("active"));

        // Kivonva gomb
        IWebElement kivonvaSzuro = driver.FindElement(By.Id("filter-Kivonva"));
        js.ExecuteScript("arguments[0].click();", kivonvaSzuro);
        Thread.Sleep(300);
        Assert.True(kivonvaSzuro.GetAttribute("class").Contains("active"));

        // Visszaállítás Összes-re
        js.ExecuteScript("arguments[0].click();", osszesSzuro);
        Thread.Sleep(300);
        Assert.True(osszesSzuro.GetAttribute("class").Contains("active"));
    }

    [Fact]
    public void F_AdminEszkozKivonas()
    {
        login.Login();

        driver.Navigate().GoToUrl(url + "admin");

        // Eszközök tab
        IWebElement eszkozokTab = wait.Until(d => d.FindElement(By.Id("admin-tab-eszkozok")));
        js.ExecuteScript("arguments[0].click();", eszkozokTab);

        // Megvárjuk hogy betöltődjön a táblázat
        wait.Until(d => d.FindElements(By.CssSelector("[id^='btn-kivon-']")).Count > 0);

        // Első kivonható eszköz kivonás gombjára kattintás
        var kivonGombok = driver.FindElements(By.CssSelector("[id^='btn-kivon-']"));
        IWebElement elsoKivonBtn = kivonGombok.First();
        js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", elsoKivonBtn);
        js.ExecuteScript("arguments[0].click();", elsoKivonBtn);

        // Modal megnyílt - megjegyzés mező megjelenik
        IWebElement megjegyzesInput = wait.Until(d => d.FindElement(By.Id("kivon-megjegyzes")));

        // Megjegyzés kitöltése
        megjegyzesInput.SendKeys("Teszt kivonás - szervizben van");

        // Kivonás megerősítése
        IWebElement kivonConfirmBtn = wait.Until(d => d.FindElement(By.Id("btn-kivonas-confirm")));

        // Modal content scrollozása a gombhoz
        js.ExecuteScript(@"
                 var btn = arguments[0];
                 var modal = btn.closest('.modal-content');
                 if (modal) {
                 modal.scrollTop = modal.scrollHeight;
                    }
                 btn.scrollIntoView({block: 'center'});
                 ", kivonConfirmBtn);

        Thread.Sleep(800);
        kivonConfirmBtn.Click();
    }

}