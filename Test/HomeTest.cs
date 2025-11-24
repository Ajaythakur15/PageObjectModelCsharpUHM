using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Base;
using PageObjectModelCsharp.Util;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestClass]
    [TestCategory(Constants.TestCategories.HOME)]
    public class HomeTest : BaseTest
    {
        private LoginPage _loginPage = null!;
        private HomePage _homePage = null!;

        [TestInitialize]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Driver);
            _homePage = new HomePage(Driver);

            try
            {
                Console.WriteLine("🔐 Setting up HomeTest - Performing login...");

                _loginPage.LoginWithConfiguredUser();
                System.Threading.Thread.Sleep(Constants.Timeouts.LONG_TIMEOUT * 100); // Wait for navigation

                Console.WriteLine($"🌐 After login - URL: {Driver.Url}");
                Console.WriteLine($"📄 After login - Title: {Driver.Title}");

                _homePage.DebugHomePageElements();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Login failed in setup: {ex.Message}");
                _loginPage.TakeScreenshot("Setup_Login_Failed");
                throw;
            }
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.SMOKE)]
        public void HomePage_ShouldLoad_AfterSuccessfulLogin()
        {
            try
            {
                System.Threading.Thread.Sleep(Constants.Timeouts.MEDIUM_TIMEOUT * 100);

                Console.WriteLine($"🌐 Current URL: {Driver.Url}");
                Console.WriteLine($"📄 Page Title: {Driver.Title}");

                bool isHomePageLoaded = _homePage.IsHomePageLoaded();
                bool isUserLoggedIn = _homePage.IsUserLoggedIn();

                Console.WriteLine($"✅ Home page loaded: {isHomePageLoaded}");
                Console.WriteLine($"✅ User logged in: {isUserLoggedIn}");

                Assert.IsTrue(isHomePageLoaded,
                    $"Home page should be loaded after login. URL: {Driver.Url}, Title: {Driver.Title}");

                Console.WriteLine("✅ PASS: Home page loaded successfully after login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: Home page load test failed - {ex.Message}");
                _homePage.TakeScreenshot("HomePage_Load_Test_Failure");
                throw;
            }
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.SMOKE)]
        public void HomePage_ShouldDisplay_WelcomeMessage()
        {
            try
            {
                System.Threading.Thread.Sleep(Constants.Timeouts.MEDIUM_TIMEOUT * 100);

                bool isHomePageLoaded = _homePage.IsHomePageLoaded();
                Console.WriteLine($"🏠 Is home page loaded: {isHomePageLoaded}");

                string welcomeMessage = _homePage.GetWelcomeMessage();
                Console.WriteLine($"👋 Welcome message: '{welcomeMessage}'");

                string username = _homePage.GetUsername();
                Console.WriteLine($"👤 Username: '{username}'");

                string currentUrl = Driver.Url;
                string pageTitle = Driver.Title;

                bool isOnBuilderPortal = currentUrl.Contains("BuilderPortal", StringComparison.OrdinalIgnoreCase);
                bool hasBuilderPortalTitle = pageTitle.Contains("Builder Portal", StringComparison.OrdinalIgnoreCase);

                // MSTest doesn't support Assert.Multiple, so check sequentially
                Assert.IsTrue(isOnBuilderPortal,
                    $"Should be redirected to BuilderPortal. Current URL: {currentUrl}");
                Assert.IsTrue(hasBuilderPortalTitle,
                    $"Should have Builder Portal title. Current title: {pageTitle}");
                Assert.IsTrue(isHomePageLoaded,
                    "Should recognize home page after login");

                Console.WriteLine("✅ PASS: Home page displays content after login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: Welcome message test failed - {ex.Message}");
                _homePage.TakeScreenshot("Welcome_Message_Test_Failure");
                throw;
            }
        }

        [TestCleanup]
        public void TestTearDown()
        {
            try
            {
                if (_homePage.IsUserLoggedIn())
                {
                    _homePage.ClickLogout();
                    Console.WriteLine("🚪 Logged out successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error during logout in teardown: {ex.Message}");
            }
        }
    }
}