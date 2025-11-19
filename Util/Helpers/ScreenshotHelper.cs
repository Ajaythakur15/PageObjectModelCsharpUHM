using OpenQA.Selenium;
using System;
using System.IO;

namespace PageObjectModelCsharp.Util.Helpers
{
    /// <summary>
    /// Captures and saves screenshots for failed steps or visual validation.
    /// </summary>
    public static class ScreenshotHelper
    {
        /// <summary>
        /// Captures a screenshot and saves it to the Screenshots folder with a timestamped filename.
        /// </summary>
        /// <param name="driver">WebDriver instance.</param>
        /// <param name="testName">Name of the test or step.</param>
        /// <returns>Full path to the saved screenshot file.</returns>
        public static string Capture(IWebDriver driver, string testName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{testName}_{timestamp}.png";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");

            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string fullPath = Path.Combine(directory, fileName);
                Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                screenshot.SaveAsFile(fullPath); // Automatically infers PNG from extension

                Console.WriteLine($"📸 Screenshot saved: {fullPath}");
                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to capture screenshot: {ex.Message}");
                return string.Empty;
            }
        }
    }
}