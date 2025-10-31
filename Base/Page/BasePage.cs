using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PageObjectModelCsharp.Util;
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

            EnsureScreenshotDirectory();
        }

        private void EnsureScreenshotDirectory()
        {
            try
            {
                var screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
                if (!Directory.Exists(screenshotsDir))
                {
                    Directory.CreateDirectory(screenshotsDir);
                    Console.WriteLine($"Created screenshots directory: {screenshotsDir}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create screenshots directory: {ex.Message}");
            }
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
            var wait = customTimeout.HasValue
                ? new WebDriverWait(Driver, TimeSpan.FromSeconds(customTimeout.Value))
                : Wait;

            try
            {
                wait.Until(driver => driver.FindElement(by).Displayed);
                Console.WriteLine($"Element found and visible: {by}");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"Element not visible within timeout: {by}");
                throw;
            }
        }

        protected void WaitForElementToBeClickable(By by, int? customTimeout = null)
        {
            var wait = customTimeout.HasValue
                ? new WebDriverWait(Driver, TimeSpan.FromSeconds(customTimeout.Value))
                : Wait;

            wait.Until(driver =>
            {
                var element = driver.FindElement(by);
                return element.Displayed && element.Enabled;
            });
        }

        protected IWebElement GetElement(By by, int? customTimeout = null)
        {
            WaitForElementToBeVisible(by, customTimeout);
            return Driver.FindElement(by);
        }

        protected System.Collections.Generic.IList<IWebElement> GetElements(By by, int? customTimeout = null)
        {
            WaitForElementToBeVisible(by, customTimeout);
            return Driver.FindElements(by);
        }

        protected void Click(By by, int? customTimeout = null)
        {
            WaitForElementToBeClickable(by, customTimeout);
            var element = GetElement(by, customTimeout);
            element.Click();
        }

        protected void SendKeys(By by, string text, int? customTimeout = null)
        {
            var element = GetElement(by, customTimeout);
            element.Clear();
            element.SendKeys(text);
        }

        protected string GetText(By by, int? customTimeout = null)
        {
            var element = GetElement(by, customTimeout);
            return element.Text;
        }

        protected string? GetAttribute(By by, string attributeName, int? customTimeout = null)
        {
            var element = GetElement(by, customTimeout);
            return element.GetAttribute(attributeName);
        }

        protected bool IsElementVisible(By by, int? customTimeout = null)
        {
            try
            {
                WaitForElementToBeVisible(by, customTimeout ?? Constants.Timeouts.SHORT_TIMEOUT);
                Console.WriteLine($"Element is visible: {by}");
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine($"Element not visible: {by}");
                return false;
            }
        }

        protected bool IsElementClickable(By by, int? customTimeout = null)
        {
            try
            {
                WaitForElementToBeClickable(by, customTimeout ?? Constants.Timeouts.SHORT_TIMEOUT);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        protected void WaitForPageToLoad()
        {
            try
            {
                Wait.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                Console.WriteLine("Page loaded completely");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Page load wait failed: {ex.Message}");
            }
        }

        public string GetCurrentUrl()
        {
            var url = Driver.Url;
            Console.WriteLine($"Current URL: {url}");
            return url;
        }

        public string GetPageTitle()
        {
            var title = Driver.Title;
            Console.WriteLine($"Page Title: {title}");
            return title;
        }

        public void TakeScreenshot(string screenshotName)
        {
            try
            {
                var screenshotsDir = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
                var fileName = Path.Combine(screenshotsDir, $"{screenshotName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();

                screenshot.SaveAsFile(fileName);
                Console.WriteLine($"Screenshot saved: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }

        public void DebugPageElements()
        {
            Console.WriteLine("=== DEBUG: Finding All Elements ===");
            Console.WriteLine($"Current URL: {Driver.Url}");
            Console.WriteLine($"Page Title: {Driver.Title}");

            // Find all input fields
            var inputs = Driver.FindElements(By.TagName("input"));
            Console.WriteLine($"Found {inputs.Count} input fields:");
            foreach (var input in inputs)
            {
                try
                {
                    string type = input.GetAttribute("type") ?? "null";
                    string id = input.GetAttribute("id") ?? "null";
                    string name = input.GetAttribute("name") ?? "null";
                    string placeholder = input.GetAttribute("placeholder") ?? "null";
                    Console.WriteLine($"  Input: type={type}, id={id}, name={name}, placeholder={placeholder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Input: Error reading attributes - {ex.Message}");
                }
            }

            // Find all buttons
            var buttons = Driver.FindElements(By.TagName("button"));
            Console.WriteLine($"Found {buttons.Count} buttons:");
            foreach (var button in buttons)
            {
                try
                {
                    string text = button.Text ?? "null";
                    string type = button.GetAttribute("type") ?? "null";
                    string @class = button.GetAttribute("class") ?? "null";
                    Console.WriteLine($"  Button: text='{text}', type={type}, class={@class}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Button: Error reading attributes - {ex.Message}");
                }
            }

            TakeScreenshot("Debug_Page_Elements");
            Console.WriteLine("=== DEBUG END ===");
        }
    }
}