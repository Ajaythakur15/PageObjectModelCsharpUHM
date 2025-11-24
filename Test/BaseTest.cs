using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using PageObjectModelCsharp.Base;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System;
using System.Threading.Tasks;

[TestClass]
[ExceptionHandler] // ✅ Centralized failure handling
public class BaseTest
{
    public TestContext TestContext { get; set; } = null!;  // ✅ MSTest injects this at runtime

    protected IWebDriver Driver { get; private set; } = null!;
    protected static string BaseUrl { get; private set; } = null!;

    [ClassInitialize]
    public static void OneTimeSetup(TestContext context)
    {
        Console.WriteLine("🔍 Validating configuration...");
        ConfigValidator.ValidateAll();

        BaseUrl = PropertyReader.GetPropertyValue("baseUrl", string.Empty);

        Console.WriteLine($"🌐 BaseUrl read from config: '{BaseUrl}'");

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException(
                "BaseUrl is not configured. Please check App.properties and PropertyReader implementation.");
        }

        string testEmail = PropertyReader.GetPropertyValue("testEmail", "default@user.com");
        ExtentReportManager.SetReportEmail(testEmail);

        string driverMode = PropertyReader.GetPropertyValue("driverMode", "local");
        if (driverMode.Equals("webdrivermanager", StringComparison.OrdinalIgnoreCase))
        {
            new WebDriverManager.DriverManager().SetUpDriver(new WebDriverManager.DriverConfigs.Impl.ChromeConfig());
            Console.WriteLine("🌐 WebDriverManager used to fetch ChromeDriver");
        }
        else
        {
            Console.WriteLine("📦 Using local/NuGet ChromeDriver");
        }

        ExtentReportManager.InitReport();
        Console.WriteLine("✅ ChromeDriver setup completed");
    }

    [TestInitialize]
    public void Setup()
    {
        Driver = DriverFactory.GetDriver();
        DriverProvider.SetDriver(Driver);

        // Ensure properties are loaded and get baseUrl explicitly here
        var baseUrl = PropertyReader.GetPropertyValue("baseUrl", string.Empty).Trim();
        Console.WriteLine($"📡 BaseTest.Setup: resolved baseUrl = '{baseUrl}'");

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "BaseUrl is not configured. Please check App.properties and PropertyReader implementation.");
        }

        Driver.Navigate().GoToUrl(baseUrl);

        // ✅ Defensive handling of TestContext.TestName
        var testName = string.IsNullOrWhiteSpace(TestContext?.TestName)
            ? "UnnamedTest"
            : TestContext.TestName;

        ExtentReportManager.CreateTest(testName);
        Console.WriteLine($"🧪 Starting test: {testName}");
    }

    [TestCleanup]
    public void Teardown()
    {
        DriverProvider.ClearDriver();
        DriverFactory.QuitDriver();
    }

    [ClassCleanup]
    public static async Task OneTimeTeardown()
    {
        await ExtentReportManager.FlushReport();
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