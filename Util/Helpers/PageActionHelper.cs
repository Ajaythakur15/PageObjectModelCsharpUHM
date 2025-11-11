using System;
using OpenQA.Selenium;
using PageObjectModelCsharp.Util;

namespace PageObjectModelCsharp.Util.Helpers
{
    public static class PageActionHelper
    {
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
                throw;
            }
        }

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
                throw;
            }
        }
    }
}