using OpenQA.Selenium;
using System;
using System.IO;

namespace PageObjectModelCsharp.Util.Helpers
{
    public static class ScreenshotHelper
    {
        public static string Capture(IWebDriver driver, string testName)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{testName}_{timestamp}.png";
            string directory = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string fullPath = Path.Combine(directory, fileName);
            Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            screenshot.SaveAsFile(fullPath); // PNG inferred from extension

            return fullPath;
        }
    }
}