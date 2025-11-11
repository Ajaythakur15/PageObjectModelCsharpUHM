using System;
using System.Collections.Generic;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;
using PageObjectModelCsharp.Base;

namespace PageObjectModelCsharp.Page
{
    public class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;

        protected BasePage(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            var timeout = GetTimeoutFromConfig();
            Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeout));

            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Constants.Timeouts.IMPLICIT_WAIT);
        }

        private int GetTimeoutFromConfig()
        {
            try
            {
                return int.Parse(PropertyReader.GetPropertyValue("timeout", "30"));
            }
            catch
            {
                return Constants.Timeouts.MEDIUM_TIMEOUT;
            }
        }

        protected void WaitForElementToBeVisible(By by, int? customTimeout = null)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                var wait = customTimeout.HasValue
                    ? new WebDriverWait(Driver, TimeSpan.FromSeconds(customTimeout.Value))
                    : Wait;

                wait.Until(driver => driver.FindElement(by).Displayed);
            }, $"Wait for element to be visible: {by}");
        }

        protected void WaitForElementToBeClickable(By by, int? customTimeout = null)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                var wait = customTimeout.HasValue
                    ? new WebDriverWait(Driver, TimeSpan.FromSeconds(customTimeout.Value))
                    : Wait;

                wait.Until(driver =>
                {
                    var element = driver.FindElement(by);
                    return element.Displayed && element.Enabled;
                });
            }, $"Wait for element to be clickable: {by}");
        }

        protected IWebElement GetElement(By by, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () =>
            {
                WaitForElementToBeVisible(by, customTimeout);
                return Driver.FindElement(by);
            }, $"Get element: {by}");
        }

        protected IList<IWebElement> GetElements(By by, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () =>
            {
                WaitForElementToBeVisible(by, customTimeout);
                return Driver.FindElements(by);
            }, $"Get elements: {by}");
        }

        protected void Click(By by, int? customTimeout = null)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                WaitForElementToBeClickable(by, customTimeout);
                GetElement(by, customTimeout).Click();
            }, $"Click element: {by}");
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
            return PageActionHelper.Execute(Driver, () =>
            {
                return GetElement(by, customTimeout).Text;
            }, $"Get text from element: {by}");
        }

        protected string? GetAttribute(By by, string attributeName, int? customTimeout = null)
        {
            return PageActionHelper.Execute(Driver, () =>
            {
                return GetElement(by, customTimeout).GetAttribute(attributeName);
            }, $"Get attribute '{attributeName}' from {by}");
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

        protected void WaitForPageToLoad()
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
                ExtentReportManager.GetTest().Info($"Screenshot: {screenshotName}")
                    .AddScreenCaptureFromPath(path);
            }, $"Take screenshot: {screenshotName}");
        }

        public void DebugPageElements()
        {
            PageActionHelper.Execute(Driver, () =>
            {
                ExtentReportManager.GetTest().Info("=== DEBUG: Finding All Elements ===");
                ExtentReportManager.GetTest().Info($"Current URL: {Driver.Url}");
                ExtentReportManager.GetTest().Info($"Page Title: {Driver.Title}");

                var inputs = Driver.FindElements(By.TagName("input"));
                ExtentReportManager.GetTest().Info($"Found {inputs.Count} input fields:");
                foreach (var input in inputs)
                {
                    string type = input.GetAttribute("type") ?? "null";
                    string id = input.GetAttribute("id") ?? "null";
                    string name = input.GetAttribute("name") ?? "null";
                    string placeholder = input.GetAttribute("placeholder") ?? "null";
                    ExtentReportManager.GetTest().Info($"Input: type={type}, id={id}, name={name}, placeholder={placeholder}");
                }

                var buttons = Driver.FindElements(By.TagName("button"));
                ExtentReportManager.GetTest().Info($"Found {buttons.Count} buttons:");
                foreach (var button in buttons)
                {
                    string text = button.Text ?? "null";
                    string type = button.GetAttribute("type") ?? "null";
                    string @class = button.GetAttribute("class") ?? "null";
                    ExtentReportManager.GetTest().Info($"Button: text='{text}', type={type}, class={@class}");
                }

                TakeScreenshot("Debug_Page_Elements");
                ExtentReportManager.GetTest().Info("=== DEBUG END ===");
            }, "Debug page elements");
        }
    }
}