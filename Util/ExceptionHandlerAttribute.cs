using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using PageObjectModelCsharp.Util.Helpers;

namespace PageObjectModelCsharp.Util
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionHandlerAttribute : Attribute, ITestAction
    {
        public ActionTargets Targets => ActionTargets.Test;

        public void BeforeTest(ITest test) { }

        public void AfterTest(ITest test)
        {
            var context = TestContext.CurrentContext;

            if (context.Result.Outcome.Status == TestStatus.Failed)
            {
                IWebDriver? driver = DriverProvider.CurrentDriver;

                if (driver != null)
                {
                    string screenshotPath = ScreenshotHelper.Capture(driver, context.Test.Name);
                    ExtentReportManager.LogFailure(context.Test.Name, context.Result.Message, screenshotPath);
                }
            }
        }
    }
}