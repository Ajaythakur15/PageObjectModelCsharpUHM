using OpenQA.Selenium;
using System;

namespace PageObjectModelCsharp.Util.Helpers
{
    /// <summary>
    /// Provides thread-safe access to the current WebDriver instance.
    /// </summary>
    public static class DriverProvider
    {
        [ThreadStatic]
        private static IWebDriver? _driver;
        public static IWebDriver? CurrentDriver => _driver;
        public static void SetDriver(IWebDriver driver)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Console.WriteLine("WebDriver instance set for thread.");
        }
        public static void ClearDriver()
        {
            _driver?.Quit(); // Optional: quit before clearing
            _driver = null;
            Console.WriteLine("WebDriver instance cleared for thread.");
        }
    }
}