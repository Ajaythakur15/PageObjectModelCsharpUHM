using NUnit.Framework;
using OpenQA.Selenium;
using PageObjectModelCsharp.Base;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;
using System;
using System.Threading.Tasks;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    [ExceptionHandler] // ✅ Centralized failure handling
    public class BaseTest
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected string BaseUrl { get; private set; } = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Console.WriteLine("🔍 Validating configuration...");
            ConfigValidator.ValidateAll(); // ✅ Auto-validate required config keys

            BaseUrl = PropertyReader.GetPropertyValue("baseUrl");
            string testEmail = PropertyReader.GetPropertyValue("testEmail", "default@user.com");

            ExtentReportManager.SetReportEmail(testEmail); // Set email before InitReport
            new DriverManager().SetUpDriver(new ChromeConfig()); // Optional: switch based on config
            ExtentReportManager.InitReport();

            Console.WriteLine("✅ ChromeDriver setup completed");
        }

        [SetUp]
        public void Setup()
        {
            Driver = DriverFactory.GetDriver();           // Centralized driver creation
            DriverProvider.SetDriver(Driver);             // Thread-safe for exception handling
            Driver.Navigate().GoToUrl(BaseUrl);           // Launch base URL
            ExtentReportManager.CreateTest(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void Teardown()
        {
            DriverProvider.ClearDriver();                 // Clear thread-local reference
            DriverFactory.QuitDriver();                   // Quit and dispose safely
        }

        [OneTimeTearDown]
        public async Task OneTimeTeardown()
        {
            await ExtentReportManager.FlushReport();      // Finalize report
            Console.WriteLine("📄 Report finalized");
        }

        protected string TakeScreenshot(string screenshotName)
        {
            return ScreenshotHelper.Capture(Driver, screenshotName);
        }

        protected void WaitForPageToLoad(int timeoutInSeconds = Constants.Timeouts.MEDIUM_TIMEOUT)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(driver => js.ExecuteScript("return document.readyState").ToString() == "complete");
        }
    }
}