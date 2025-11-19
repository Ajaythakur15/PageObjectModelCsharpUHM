using System;
using OpenQA.Selenium;
using PageObjectModelCsharp.Util;

namespace PageObjectModelCsharp.Util.Helpers
{
    /// <summary>
    /// Provides safe execution wrappers for Selenium actions with reporting and screenshot capture.
    /// </summary>
    public static class PageActionHelper
    {
        /// <summary>
        /// Executes a void action with reporting and failure screenshot.
        /// </summary>
        /// <param name="driver">WebDriver instance.</param>
        /// <param name="action">Action to execute.</param>
        /// <param name="stepName">Step description for reporting.</param>
        public static void Execute(IWebDriver driver, Action action, string stepName)
        {
            try
            {
                action.Invoke();
                ExtentReportManager.GetTest().Info($"✅ Step passed: {stepName}");
            }
            catch (Exception ex)
            {
                string screenshotPath = ScreenshotHelper.Capture(driver, $"Error_{stepName}");
                ExtentReportManager.LogFailure(stepName, ex.Message, screenshotPath);
                Console.WriteLine($"❌ Step failed: {stepName} — {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes a function with reporting and failure screenshot.
        /// </summary>
        /// <typeparam name="T">Return type of the function.</typeparam>
        /// <param name="driver">WebDriver instance.</param>
        /// <param name="func">Function to execute.</param>
        /// <param name="stepName">Step description for reporting.</param>
        /// <returns>Result of the function.</returns>
        public static T Execute<T>(IWebDriver driver, Func<T> func, string stepName)
        {
            try
            {
                T result = func.Invoke();
                ExtentReportManager.GetTest().Info($"✅ Step passed: {stepName}");
                return result;
            }
            catch (Exception ex)
            {
                string screenshotPath = ScreenshotHelper.Capture(driver, $"Error_{stepName}");
                ExtentReportManager.LogFailure(stepName, ex.Message, screenshotPath);
                Console.WriteLine($"❌ Step failed: {stepName} — {ex.Message}");
                throw;
            }
        }
    }
}