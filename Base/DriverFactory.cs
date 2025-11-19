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
            if (_driver != null) return _driver;

            string browser = PropertyReader.GetPropertyValue("browser", "chrome").ToLower();
            bool headless = bool.Parse(PropertyReader.GetPropertyValue("headless", "false"));
            string windowBehavior = PropertyReader.GetPropertyValue("windowBehavior", "maximize").ToLower().Trim();

            switch (browser)
            {
                case "chrome":
                    _driver = CreateChromeDriver(headless);
                    break;

                case "edge":
                    _driver = CreateEdgeDriver(headless);
                    break;

                default:
                    throw new NotSupportedException($"❌ Browser '{browser}' is not supported");
            }

            _driver.Manage().Cookies.DeleteAllCookies(); // Clear session
            ApplyWindowBehavior(_driver, headless, windowBehavior);

            Console.WriteLine($"✅ Driver initialized: {browser}, Headless: {headless}, Behavior: {windowBehavior}");
            return _driver;
        }

        public static void QuitDriver()
        {
            if (_driver == null) return;

            try
            {
                _driver.Quit();
                _driver.Dispose();
                Console.WriteLine("🧹 Driver quit and cleaned up");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error quitting driver: {ex.Message}");
            }
            finally
            {
                _driver = null;
            }
        }

        private static IWebDriver CreateChromeDriver(bool headless)
        {
            var options = new ChromeOptions();

            if (headless)
            {
                options.AddArgument("--headless=new");
                Console.WriteLine("🕶️ Running Chrome in headless mode");
            }

            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");

            return new ChromeDriver(options);
        }

        private static IWebDriver CreateEdgeDriver(bool headless)
        {
            var options = new EdgeOptions();

            if (headless)
            {
                options.AddArgument("--headless=new");
                Console.WriteLine("🕶️ Running Edge in headless mode");
            }

            return new EdgeDriver(options);
        }

        private static void ApplyWindowBehavior(IWebDriver driver, bool headless, string behavior)
        {
            if (headless || string.IsNullOrWhiteSpace(behavior)) return;

            switch (behavior)
            {
                case "maximize":
                    driver.Manage().Window.Size = new Size(1024, 768); // Ensure visibility
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