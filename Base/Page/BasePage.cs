using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PageObjectModelCsharp.Base;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;

namespace PageObjectModelCsharp.Page
{
    public class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;

        protected BasePage(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(GetTimeoutFromConfig()));
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Constants.Timeouts.IMPLICIT_WAIT);
        }

        private int GetTimeoutFromConfig()
        {
            return int.TryParse(PropertyReader.GetPropertyValue("timeout", "30"), out var timeout)
                ? timeout
                : Constants.Timeouts.MEDIUM_TIMEOUT;
        }

        protected WebDriverWait GetWait(int? customTimeout)
        {
            return customTimeout.HasValue
                ? new WebDriverWait(Driver, TimeSpan.FromSeconds(customTimeout.Value))
                : Wait;
        }

        protected void WaitForElementToBeVisible(By by, int? customTimeout = null)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                GetWait(customTimeout).Until(driver => driver.FindElement(by).Displayed);
            }, $"Wait for element to be visible: {by}");
        }

        protected void WaitForElementToBeClickable(By by, int? customTimeout = null)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                GetWait(customTimeout).Until(driver =>
                {
                    var element = driver.FindElement(by);
                    return element.Displayed && element.Enabled;
                });
            }, $"Wait for element to be clickable: {by}");
        }

        protected IWebElement GetElement(By by, int? customTimeout = null)
        {
            WaitForElementToBeVisible(by, customTimeout);
            return PageActionHelper.Execute(Driver, () => Driver.FindElement(by), $"Get element: {by}");
        }

        public void WaitForUrlToContain(string keyword, int timeoutSeconds = 30)
        {
            new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds))
                .Until(driver => driver.Url.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        protected IList<IWebElement> GetElements(By by, int? customTimeout = null)
        {
            WaitForElementToBeVisible(by, customTimeout);
            return PageActionHelper.Execute(Driver, () => Driver.FindElements(by), $"Get elements: {by}");
        }

        protected void Click(By by, int? customTimeout = null)
        {
            WaitForElementToBeClickable(by, customTimeout);
            PageActionHelper.Execute(Driver, () => GetElement(by, customTimeout).Click(), $"Click element: {by}");
        }

        protected void SendKeys(By by, string text, int? customTimeout = null)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                var element = GetElement(by, customTimeout);
                element.Clear();
                element.SendKeys(text);
            }, $"Send keys to {by}: '{text}'");
        }

        protected string GetText(By by, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () => GetElement(by, customTimeout).Text, $"Get text from element: {by}");
        }

        protected string? GetAttribute(By by, string attributeName, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () => GetElement(by, customTimeout).GetAttribute(attributeName), $"Get attribute '{attributeName}' from {by}");
        }

        protected bool IsElementVisible(By by, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () =>
            {
                try
                {
                    WaitForElementToBeVisible(by, customTimeout ?? Constants.Timeouts.SHORT_TIMEOUT);
                    return true;
                }
                catch
                {
                    return false;
                }
            }, $"Check if element is visible: {by}");
        }

        protected bool IsElementClickable(By by, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () =>
            {
                try
                {
                    WaitForElementToBeClickable(by, customTimeout ?? Constants.Timeouts.SHORT_TIMEOUT);
                    return true;
                }
                catch
                {
                    return false;
                }
            }, $"Check if element is clickable: {by}");
        }

        public void WaitForPageToLoad()
        {
            PageActionHelper.Execute(Driver, () =>
            {
                Wait.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }, "Wait for page to load");
        }

        public string GetCurrentUrl()
        {
            return PageActionHelper.Execute(Driver, () => Driver.Url, "Get current URL");
        }

        public string GetPageTitle()
        {
            return PageActionHelper.Execute(Driver, () => Driver.Title, "Get page title");
        }

        public void TakeScreenshot(string screenshotName)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                string path = ScreenshotHelper.Capture(Driver, screenshotName);
                ExtentReportManager.GetTest().Info($"{Constants.ReportLabels.SCREENSHOT}: {screenshotName}")
                    .AddScreenCaptureFromPath(path);
            }, $"Take screenshot: {screenshotName}");
        }
        public void PressEnter(By locator)
        {
            var element = Driver.FindElement(locator);
            element.SendKeys(Keys.Enter);
            Console.WriteLine($"⏎ Pressed Enter on element: {locator}");
        }

        public void DebugPageElements()
        {
            PageActionHelper.Execute(Driver, () =>
            {
                var test = ExtentReportManager.GetTest();
                test.Info($"=== {Constants.ReportLabels.DEBUG_SECTION} ===");
                test.Info($"Current URL: {Driver.Url}");
                test.Info($"Page Title: {Driver.Title}");

                var inputs = Driver.FindElements(By.TagName("input"));
                test.Info($"Found {inputs.Count} input fields:");
                foreach (var input in inputs)
                {
                    test.Info($"Input: type={input.GetAttribute("type") ?? "null"}, id={input.GetAttribute("id") ?? "null"}, name={input.GetAttribute("name") ?? "null"}, placeholder={input.GetAttribute("placeholder") ?? "null"}");
                }

                var buttons = Driver.FindElements(By.TagName("button"));
                test.Info($"Found {buttons.Count} buttons:");
                foreach (var button in buttons)
                {
                    test.Info($"Button: text='{button.Text ?? "null"}', type={button.GetAttribute("type") ?? "null"}, class={button.GetAttribute("class") ?? "null"}");
                }

                TakeScreenshot("Debug_Page_Elements");
                test.Info($"=== END {Constants.ReportLabels.DEBUG_SECTION} ===");
            }, "Debug page elements");
        }
    }
}