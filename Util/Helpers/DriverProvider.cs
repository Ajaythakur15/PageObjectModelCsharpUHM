using OpenQA.Selenium;

namespace PageObjectModelCsharp.Util.Helpers
{
    public static class DriverProvider
    {
        [ThreadStatic]
        private static IWebDriver? _driver;

        public static IWebDriver? CurrentDriver => _driver;

        public static void SetDriver(IWebDriver driver)
        {
            _driver = driver;
        }

        public static void ClearDriver()
        {
            _driver = null;
        }
    }
}