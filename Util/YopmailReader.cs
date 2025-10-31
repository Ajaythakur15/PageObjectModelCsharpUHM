using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Threading;

namespace PageObjectModelCsharp.Util
{
    public static class YopmailReader
    {
        public static string? GetOTPFromYopmail(string email)
        {
            IWebDriver? driver = null;
            try
            {
                Console.WriteLine($"Reading OTP from Yopmail: {email}");

                // Set up Chrome options
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

                // Navigate directly to the specific email inbox
                string emailName = email.Replace("@yopmail.com", "");
                string inboxUrl = $"http://www.yopmail.com/en/?{emailName}";
                Console.WriteLine($"Navigating to: {inboxUrl}");
                driver.Navigate().GoToUrl(inboxUrl);

                // Wait for page to load
                Thread.Sleep(5000);

                // Refresh the inbox
                try
                {
                    var refreshButton = driver.FindElement(By.Id("refresh"));
                    refreshButton.Click();
                    Thread.Sleep(3000);
                }
                catch
                {
                    Console.WriteLine("Refresh button not found or not clicked");
                }

                // Switch to ifinbox frame to see emails
                try
                {
                    driver.SwitchTo().Frame("ifinbox");
                    Console.WriteLine("Switched to ifinbox frame");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not switch to ifinbox frame: {ex.Message}");
                    driver.SwitchTo().DefaultContent();
                }

                // Look for the specific email about "MFA code" or "login verification"
                var targetEmails = driver.FindElements(By.XPath(
                    "//div[contains(@class, 'm')]//*[contains(text(), 'MFA code') or contains(text(), 'login verification') or contains(text(), 'verification code')]"
                ));

                Console.WriteLine($"Found {targetEmails.Count} target emails");

                if (targetEmails.Count == 0)
                {
                    // If no specific emails found, click the first email
                    var allEmails = driver.FindElements(By.XPath("//div[contains(@class, 'm')]//button | //div[contains(@class, 'm')]//a"));
                    Console.WriteLine($"Found {allEmails.Count} total emails");

                    if (allEmails.Count > 0)
                    {
                        allEmails[0].Click();
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        throw new Exception("No emails found in Yopmail inbox");
                    }
                }
                else
                {
                    targetEmails[0].Click();
                    Thread.Sleep(3000);
                }

                // Switch to email content frame
                driver.SwitchTo().DefaultContent();

                try
                {
                    driver.SwitchTo().Frame("ifmail");
                    Console.WriteLine("Switched to ifmail frame");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not switch to ifmail frame: {ex.Message}");
                    throw new Exception("Cannot access email content");
                }

                // Wait for email content
                Thread.Sleep(2000);

                // Extract OTP using multiple methods
                string? otp = ExtractOTPFromPage(driver);

                if (!string.IsNullOrEmpty(otp))
                {
                    Console.WriteLine($"Successfully extracted OTP: {otp}");
                    return otp;
                }
                else
                {
                    throw new Exception("Could not find OTP in email content");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from Yopmail: {ex.Message}");
                return null;
            }
            finally
            {
                driver?.Quit();
                driver?.Dispose();
            }
        }

        private static string? ExtractOTPFromPage(IWebDriver driver)
        {
            // Method 1: Look for the specific text pattern
            try
            {
                var elementsWithCode = driver.FindElements(By.XPath("//*[contains(text(), 'Your code is:')]"));
                foreach (var element in elementsWithCode)
                {
                    string text = element.Text;
                    Console.WriteLine($"Found element with code text: {text}");

                    var match = Regex.Match(text, @"Your code is:\s*(\d{6})");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in method 1: {ex.Message}");
            }

            // Method 2: Get all text and search for pattern
            try
            {
                string allText = driver.FindElement(By.TagName("body")).Text;
                Console.WriteLine($"All text content: {allText}");

                var match = Regex.Match(allText, @"Your code is:\s*(\d{6})");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                // Fallback: look for any 6-digit number
                match = Regex.Match(allText, @"\b\d{6}\b");
                if (match.Success)
                {
                    return match.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in method 2: {ex.Message}");
            }

            return null;
        }

        private static string? ExtractOTPFromEmailContent(string emailContent)
        {
            if (string.IsNullOrEmpty(emailContent))
                return null;

            Console.WriteLine($"Searching for OTP in email content...");

            // Specific patterns for UnionHome Mortgage OTP
            var patterns = new[]
            {
                @"Your code is:\s*(\d{6})",                    // "Your code is: 329332"
                @"code is:\s*(\d{6})",                         // "code is: 329332"  
                @"is:\s*(\d{6})",                              // "is: 329332"
                @"verification code[\s:]*(\d{6})",             // "verification code: 329332"
                @"MFA code[\s:]*(\d{6})",                      // "MFA code: 329332"
                @"\b\d{6}\b",                                  // standalone 6-digit number
                @"[\s:](\d{6})[\s\.]"                          // " 329332 " or ":329332."
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(emailContent, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var otp = match.Groups[1].Success ? match.Groups[1].Value : match.Value;
                    Console.WriteLine($"Found OTP using pattern '{pattern}': {otp}");
                    return otp.Trim();
                }
            }

            Console.WriteLine("No OTP pattern matched in email content");
            return null;
        }
    }
}