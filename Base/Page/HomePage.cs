using OpenQA.Selenium;
using PageObjectModelCsharp.Util;
using System;

namespace PageObjectModelCsharp.Page
{
    public class HomePage : BasePage
    {
        // More flexible locators - update these based on your actual application
        private readonly By WelcomeMessage = By.XPath("//*[contains(text(), 'Welcome') or contains(text(), 'welcome')]");
        private readonly By UserProfile = By.XPath("//*[contains(@class, 'user') or contains(@class, 'profile') or contains(text(), 'Welcome')]");
        private readonly By Dashboard = By.XPath("//*[contains(text(), 'Dashboard') or contains(text(), 'dashboard') or contains(@class, 'dashboard')]");
        private readonly By LogoutButton = By.XPath("//*[contains(text(), 'Logout') or contains(text(), 'Sign out') or contains(@class, 'logout')]");
        private readonly By AnyPageContent = By.XPath("//body//*[text()][not(self::script)]");

        public HomePage(IWebDriver driver) : base(driver)
        {
        }

        public bool IsHomePageLoaded()
        {
            try
            {
                WaitForPageToLoad();
                Console.WriteLine("Checking if home page is loaded...");

                // Check multiple indicators
                bool hasDashboard = IsElementVisible(Dashboard, 10);
                bool hasWelcome = IsElementVisible(WelcomeMessage, 5);
                bool hasUserProfile = IsElementVisible(UserProfile, 5);
                bool hasAnyContent = IsElementVisible(AnyPageContent, 5);

                string currentUrl = GetCurrentUrl();
                bool isLoginUrl = currentUrl.Contains("login") || currentUrl.Contains("auth");

                Console.WriteLine($"Dashboard visible: {hasDashboard}");
                Console.WriteLine($"Welcome message visible: {hasWelcome}");
                Console.WriteLine($"User profile visible: {hasUserProfile}");
                Console.WriteLine($"Any content visible: {hasAnyContent}");
                Console.WriteLine($"Is login URL: {isLoginUrl}");
                Console.WriteLine($"Current URL: {currentUrl}");

                // Home page is loaded if we have any content and we're not on login page
                return hasAnyContent && !isLoginUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking home page: {ex.Message}");
                return false;
            }
        }

        public string GetWelcomeMessage()
        {
            if (IsElementVisible(WelcomeMessage))
            {
                return GetText(WelcomeMessage) ?? string.Empty;
            }
            return string.Empty;
        }

        public string GetUsername()
        {
            if (IsElementVisible(UserProfile))
            {
                return GetText(UserProfile) ?? string.Empty;
            }
            return string.Empty;
        }

        public void ClickLogout()
        {
            if (IsElementVisible(LogoutButton))
            {
                Click(LogoutButton);
            }
        }

        public bool IsUserLoggedIn()
        {
            try
            {
                string currentUrl = GetCurrentUrl();
                bool isLoginPage = currentUrl.Contains("login") || currentUrl.Contains("signin") || currentUrl.Contains("auth");
                bool hasUserElements = IsElementVisible(UserProfile) || IsElementVisible(WelcomeMessage);

                Console.WriteLine($"Is login page: {isLoginPage}");
                Console.WriteLine($"Has user elements: {hasUserElements}");

                return !isLoginPage && hasUserElements;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking login status: {ex.Message}");
                return false;
            }
        }
    }
}