using System;
using System.IO;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    public class BaseTest
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected string BaseUrl { get; private set; } = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            BaseUrl = PropertyReader.GetPropertyValue("baseUrl");

            // Setup ChromeDriver automatically
            new DriverManager().SetUpDriver(new ChromeConfig());
            Console.WriteLine("ChromeDriver setup completed");
        }

        [SetUp]
        public void Setup()
        {
            InitializeDriver();
            Driver.Manage().Window.Maximize();
            Driver.Navigate().GoToUrl(BaseUrl);
            Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(Constants.Timeouts.LONG_TIMEOUT);
        }

        [TearDown]
        public void Teardown()
        {
            try
            {
                if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    var screenshotName = $"{TestContext.CurrentContext.Test.Name}_Failure";
                    TakeScreenshot(screenshotName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during teardown: {ex.Message}");
            }
            finally
            {
                Driver?.Quit();
                Driver?.Dispose();
            }
        }

        private void InitializeDriver()
        {
            var browser = PropertyReader.GetPropertyValue("browser", "chrome").ToLower();
            var headless = bool.Parse(PropertyReader.GetPropertyValue("headless", "false"));

            switch (browser)
            {
                case "chrome":
                    var chromeOptions = new ChromeOptions();

                    if (headless)
                    {
                        chromeOptions.AddArgument("--headless=new");
                        Console.WriteLine("Running in HEADLESS mode");
                    }

                    chromeOptions.AddArgument("--no-sandbox");
                    chromeOptions.AddArgument("--disable-dev-shm-usage");
                    chromeOptions.AddArgument("--disable-gpu");
                    chromeOptions.AddArgument("--window-size=1920,1080");
                    chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
                    chromeOptions.AddExcludedArgument("enable-automation");

                    if (!headless)
                    {
                        chromeOptions.AddArgument("--start-maximized");
                    }

                    Driver = new ChromeDriver(chromeOptions);
                    break;
                default:
                    throw new NotSupportedException($"Browser '{browser}' is not supported");
            }

            Console.WriteLine($"Browser: {browser}, Headless: {headless}");
        }

        // Change from private to protected so child classes can access it
        protected void TakeScreenshot(string screenshotName)
        {
            try
            {
                // Create screenshots directory if it doesn't exist
                var screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
                if (!Directory.Exists(screenshotsDir))
                {
                    Directory.CreateDirectory(screenshotsDir);
                }

                var fileName = Path.Combine(screenshotsDir, $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(fileName);
                TestContext.AddTestAttachment(fileName);
                Console.WriteLine($"Screenshot saved: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }
    }
}