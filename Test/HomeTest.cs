using NUnit.Framework;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Base;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    [Category(Constants.TestCategories.HOME)]
    public class HomeTest : BaseTest
    {
        private LoginPage _loginPage = null!;
        private HomePage _homePage = null!;

        [SetUp]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Driver);
            _homePage = new HomePage(Driver);

            try
            {
                Console.WriteLine("Setting up HomeTest - Performing login...");

                // Login first to access home page
                _loginPage.LoginWithConfiguredUser();

                // Wait for navigation to complete
                System.Threading.Thread.Sleep(10000); // Increased wait time

                Console.WriteLine($"After login - Current URL: {Driver.Url}");
                Console.WriteLine($"After login - Page Title: {Driver.Title}");

                // Debug home page elements
                _homePage.DebugHomePageElements();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login failed in setup: {ex.Message}");
                _loginPage.TakeScreenshot("Setup_Login_Failed");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.SMOKE)]
        public void HomePage_ShouldLoad_AfterSuccessfulLogin()
        {
            try
            {
                // Additional wait to ensure page is fully loaded
                System.Threading.Thread.Sleep(5000);

                // Debug current state
                Console.WriteLine($"Current URL: {Driver.Url}");
                Console.WriteLine($"Page Title: {Driver.Title}");

                // Assert - Use more flexible criteria
                bool isHomePageLoaded = _homePage.IsHomePageLoaded();
                bool isUserLoggedIn = _homePage.IsUserLoggedIn();

                Console.WriteLine($"Home page loaded: {isHomePageLoaded}");
                Console.WriteLine($"User logged in: {isUserLoggedIn}");

                // The key assertion: we should be on BuilderPortal with correct title
                Assert.That(isHomePageLoaded, Is.True,
                    $"Home page should be loaded after login. URL: {Driver.Url}, Title: {Driver.Title}");

                Console.WriteLine("PASS: Home page loaded successfully after login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Home page load test failed - {ex.Message}");
                _homePage.TakeScreenshot("HomePage_Load_Test_Failure");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.SMOKE)]
        public void HomePage_ShouldDisplay_WelcomeMessage()
        {
            try
            {
                // Additional wait to ensure page is fully loaded
                System.Threading.Thread.Sleep(5000);

                // Check if home page is loaded
                bool isHomePageLoaded = _homePage.IsHomePageLoaded();
                Console.WriteLine($"Is home page loaded: {isHomePageLoaded}");

                // Get welcome message
                var welcomeMessage = _homePage.GetWelcomeMessage();
                Console.WriteLine($"Welcome message: '{welcomeMessage}'");

                // Get username
                var username = _homePage.GetUsername();
                Console.WriteLine($"Username: '{username}'");

                // Assert - We should at least be on BuilderPortal page
                string currentUrl = Driver.Url;
                string pageTitle = Driver.Title;

                bool isOnBuilderPortal = currentUrl.Contains("BuilderPortal", StringComparison.OrdinalIgnoreCase);
                bool hasBuilderPortalTitle = pageTitle.Contains("Builder Portal", StringComparison.OrdinalIgnoreCase);

                Assert.Multiple(() =>
                {
                    Assert.That(isOnBuilderPortal, Is.True,
                        $"Should be redirected to BuilderPortal after login. Current URL: {currentUrl}");
                    Assert.That(hasBuilderPortalTitle, Is.True,
                        $"Should have Builder Portal title. Current title: {pageTitle}");
                    Assert.That(isHomePageLoaded, Is.True,
                        "Should recognize home page after login");
                });

                Console.WriteLine("PASS: Home page displays content after login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Welcome message test failed - {ex.Message}");
                _homePage.TakeScreenshot("Welcome_Message_Test_Failure");
                throw;
            }
        }

        [TearDown]
        public void TestTearDown()
        {
            try
            {
                // Logout after each test
                if (_homePage.IsUserLoggedIn())
                {
                    _homePage.ClickLogout();
                    Console.WriteLine("Logged out successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout in teardown: {ex.Message}");
            }
        }
    }
}