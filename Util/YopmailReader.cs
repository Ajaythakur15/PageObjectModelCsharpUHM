using System;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace PageObjectModelCsharp.Util
{
    public static class YopmailReader
    {
        public static string? GetOTPFromYopmail(string email)
        {
            IWebDriver? driver = null;
            try
            {
                Console.WriteLine($"📩 Reading OTP from Yopmail: {email}");

                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--headless=new");
                chromeOptions.AddArgument("--no-sandbox");
                chromeOptions.AddArgument("--disable-dev-shm-usage");
                chromeOptions.AddArgument("--disable-gpu");
                chromeOptions.AddArgument("--window-size=1920,1080");
                chromeOptions.AddArgument("--disable-blink-features=AutomationControlled");
                chromeOptions.AddExcludedArgument("enable-automation");

                driver = new ChromeDriver(chromeOptions);
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                string emailName = email.Replace("@yopmail.com", "");
                string inboxUrl = $"http://www.yopmail.com/en/?{emailName}";
                Console.WriteLine($"🌐 Navigating to: {inboxUrl}");
                driver.Navigate().GoToUrl(inboxUrl);

                Thread.Sleep(5000); // Wait for inbox to load

                TryClickRefresh(driver);
                TrySwitchToFrame(driver, "ifinbox");

                var targetEmails = driver.FindElements(By.XPath(
                    "//div[contains(@class, 'm')]//*[contains(text(), 'MFA code') or contains(text(), 'login verification') or contains(text(), 'verification code')]"
                ));

                Console.WriteLine($"📨 Found {targetEmails.Count} target emails");

                if (targetEmails.Count > 0)
                {
                    targetEmails[0].Click();
                }
                else
                {
                    var allEmails = driver.FindElements(By.XPath("//div[contains(@class, 'm')]//button | //div[contains(@class, 'm')]//a"));
                    Console.WriteLine($"📬 Found {allEmails.Count} total emails");

                    if (allEmails.Count > 0)
                        allEmails[0].Click();
                    else
                        throw new Exception("No emails found in Yopmail inbox");
                }

                Thread.Sleep(3000);
                driver.SwitchTo().DefaultContent();
                TrySwitchToFrame(driver, "ifmail");

                Thread.Sleep(2000);
                string? otp = ExtractOTPFromPage(driver);

                if (!string.IsNullOrEmpty(otp))
                {
                    Console.WriteLine($"✅ Successfully extracted OTP: {otp}");
                    return otp;
                }

                throw new Exception("Could not find OTP in email content");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading from Yopmail: {ex.Message}");
                return null;
            }
            finally
            {
                driver?.Quit();
                driver?.Dispose();
            }
        }

        private static void TryClickRefresh(IWebDriver driver)
        {
            try
            {
                var refreshButton = driver.FindElement(By.Id("refresh"));
                refreshButton.Click();
                Thread.Sleep(3000);
            }
            catch
            {
                Console.WriteLine("⚠️ Refresh button not found or not clickable");
            }
        }

        private static void TrySwitchToFrame(IWebDriver driver, string frameName)
        {
            try
            {
                driver.SwitchTo().Frame(frameName);
                Console.WriteLine($"🔄 Switched to frame: {frameName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not switch to frame '{frameName}': {ex.Message}");
                driver.SwitchTo().DefaultContent();
            }
        }

        private static string? ExtractOTPFromPage(IWebDriver driver)
        {
            try
            {
                var elements = driver.FindElements(By.XPath("//*[contains(text(), 'Your code is:')]"));
                foreach (var element in elements)
                {
                    string text = element.Text;
                    Console.WriteLine($"🔍 Found element with code text: {text}");

                    var match = Regex.Match(text, @"Your code is:\s*(\d{6})");
                    if (match.Success)
                        return match.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in direct element scan: {ex.Message}");
            }

            try
            {
                string allText = driver.FindElement(By.TagName("body")).Text;
                Console.WriteLine($"📄 Full email text: {allText}");

                var match = Regex.Match(allText, @"Your code is:\s*(\d{6})");
                if (match.Success)
                    return match.Groups[1].Value;

                match = Regex.Match(allText, @"\b\d{6}\b");
                if (match.Success)
                    return match.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error in fallback text scan: {ex.Message}");
            }

            return null;
        }
    }
}