using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using PageObjectModelCsharp.Util.Helpers;

namespace PageObjectModelCsharp.Util
{
    /// <summary>
    /// NUnit attribute to handle test lifecycle events and integrate with ExtentReports.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionHandlerAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test)
        {
            // Register test in ExtentReport lifecycle
            ExtentReportManager.CreateTest(test.Name);
            ExtentTestManager.CreateTest(test.Name);
        }

        public void AfterTest(ITest test)
        {
            var context = TestContext.CurrentContext;
            var status = context.Result.Outcome.Status;
            var testName = context.Test.Name;
            var message = context.Result.Message ?? "No failure message";

            switch (status)
            {
                case TestStatus.Passed:
                    ExtentTestManager.MarkTestAsPassed(testName);
                    break;

                case TestStatus.Failed:
                    string screenshotPath = "Screenshot not available";
                    IWebDriver? driver = DriverProvider.CurrentDriver;

                    if (driver != null)
                    {
                        try
                        {
                            screenshotPath = ScreenshotHelper.Capture(driver, testName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Failed to capture screenshot: {ex.Message}");
                        }
                    }

                    ExtentReportManager.LogFailure(testName, message, screenshotPath);
                    ExtentTestManager.MarkTestAsFailed(testName, message);
                    break;

                case TestStatus.Skipped:
                    string reason = context.Result.Message ?? "No skip reason provided";
                    ExtentTestManager.MarkTestAsSkipped(testName, reason);
                    break;

                default:
                    ExtentTestManager.GetTest(testName)?.Warning($"Test ended with status: {status}");
                    break;
            }
        }
    }
}