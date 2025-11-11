using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using PageObjectModelCsharp.Util;
using System;
using System.Drawing;

namespace PageObjectModelCsharp.Base
{
    public static class DriverFactory
    {
        [ThreadStatic]
        private static IWebDriver? _driver;

        public static IWebDriver GetDriver()
        {
            if (_driver == null)
            {
                var browser = PropertyReader.GetPropertyValue("browser", "chrome").ToLower();
                var headless = bool.Parse(PropertyReader.GetPropertyValue("headless", "false"));
                var windowBehavior = PropertyReader.GetPropertyValue("windowBehavior", "maximize").ToLower().Trim();

                switch (browser)
                {
                    case "chrome":
                        var chromeOptions = new ChromeOptions();

                        if (headless)
                        {
                            chromeOptions.AddArgument("--headless=new");
                            Console.WriteLine("🕶️ Running Chrome in headless mode");
                        }

                        chromeOptions.AddArgument("--no-sandbox");
                        chromeOptions.AddArgument("--disable-dev-shm-usage");
                        chromeOptions.AddArgument("--disable-gpu");
                        chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
                        chromeOptions.AddExcludedArgument("enable-automation");

                        _driver = new ChromeDriver(chromeOptions);
                        _driver.Manage().Cookies.DeleteAllCookies(); // ✅ Clears session
                        ApplyWindowBehavior(_driver, headless, windowBehavior);

                        break;

                    case "edge":
                        var edgeOptions = new EdgeOptions();

                        if (headless)
                        {
                            edgeOptions.AddArgument("--headless=new");
                            Console.WriteLine("🕶️ Running Edge in headless mode");
                        }

                        _driver = new EdgeDriver(edgeOptions);
                        _driver.Manage().Cookies.DeleteAllCookies(); // ✅ Clears session
                        ApplyWindowBehavior(_driver, headless, windowBehavior);

                        break;

                    default:
                        throw new NotSupportedException($"Browser '{browser}' is not supported");
                }

                ApplyWindowBehavior(_driver, headless, windowBehavior);
                Console.WriteLine($"✅ Driver initialized: {browser}, Headless: {headless}, Behavior: {windowBehavior}");
            }

            return _driver;
        }

        public static void QuitDriver()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
                Console.WriteLine("🧹 Driver quit and cleaned up");
            }
        }

        private static void ApplyWindowBehavior(IWebDriver driver, bool headless, string behavior)
        {
            if (headless || string.IsNullOrWhiteSpace(behavior)) return;

            switch (behavior)
            {
                case "maximize":
                    driver.Manage().Window.Size = new Size(1024, 768); // Optional: ensure window is visible
                    driver.Manage().Window.Maximize();
                    Console.WriteLine("🪟 Window maximized");
                    break;

                case "minimize":
                    try
                    {
                        driver.Manage().Window.Minimize();
                        Console.WriteLine("🪟 Window minimized");
                    }
                    catch
                    {
                        driver.Manage().Window.Position = new Point(-2000, 0);
                        Console.WriteLine("🪟 Window moved off-screen to simulate minimize");
                    }
                    break;

                default:
                    Console.WriteLine($"⚠️ Unknown window behavior: '{behavior}' — no action taken");
                    break;
            }
        }
    }
}