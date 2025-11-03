using OpenQA.Selenium;
using PageObjectModelCsharp.Util;
using System;
using System.Linq;

namespace PageObjectModelCsharp.Page
{
    public class LoginPage : BasePage
    {
        // Input field locators
        private readonly By EmailTextBox = By.XPath("//input[@id='username' or @name='username' or @type='email' or contains(@placeholder, 'email')]");
        private readonly By PasswordTextBox = By.XPath("//input[@id='password' or @name='password' or @type='password']");

        // OTP page locators
        private readonly By OTPInput = By.XPath("//input[@name='otp' or contains(@placeholder, 'Enter the code') or @type='text']");
        private readonly By ContinueButton = By.XPath("//button[contains(text(), 'Continue')]");
        private readonly By OTPPageHeader = By.XPath("//*[contains(text(), 'Verify Your Identity') or contains(text(), 'Enter the code')]");

        public LoginPage(IWebDriver driver) : base(driver)
        {
        }

        public void EnterEmail(string email)
        {
            WaitForPageToLoad();
            Console.WriteLine($"Entering email: {email}");
            SendKeys(EmailTextBox, email);
        }

        public void EnterPassword(string password)
        {
            Console.WriteLine($"Entering password: {new string('*', password.Length)}");
            SendKeys(PasswordTextBox, password);
        }

        public void ClickSignIn()
        {
            Console.WriteLine("Looking for Sign In button...");

            // Take screenshot before clicking
            TakeScreenshot("Before_SignIn_Click");

            // Method 1: Try multiple button locators
            var buttonLocators = new[]
            {
                By.XPath("//button[@class='c7ae0cd73 c91fa6616 ca65675d0 cbf0234ce cfa6fb59c']"),
                By.XPath("//button[contains(@class, 'c7ae0cd73')]"),
                By.XPath("//button[contains(text(), 'Sign In') or contains(text(), 'Login') or contains(text(), 'Continue') or @type='submit']"),
                By.CssSelector("button[type='submit']"),
                By.XPath("//input[@type='submit']"),
                By.XPath("//button"), // Any button
                By.XPath("//*[@role='button']") // Any element with button role
            };

            foreach (var locator in buttonLocators)
            {
                try
                {
                    var elements = Driver.FindElements(locator);
                    if (elements.Count > 0)
                    {
                        Console.WriteLine($"Found {elements.Count} elements with locator: {locator}");

                        // Try to find the most likely button (usually the last one or one that's visible)
                        var button = elements.LastOrDefault(e => e.Displayed && e.Enabled) ?? elements.Last();

                        if (button != null)
                        {
                            Console.WriteLine($"Attempting to click button with text: '{button.Text}', type: '{button.GetAttribute("type")}', class: '{button.GetAttribute("class")}'");

                            // Scroll into view and click
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
                            button.Click();

                            Console.WriteLine("Successfully clicked sign in button");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed with locator {locator}: {ex.Message}");
                }
            }

            // Method 2: Use JavaScript to find and click any submit button
            try
            {
                Console.WriteLine("Trying JavaScript click...");
                var result = ((IJavaScriptExecutor)Driver).ExecuteScript(@"
                    var buttons = document.querySelectorAll('button[type=""submit""], input[type=""submit""], button');
                    for (var i = 0; i < buttons.length; i++) {
                        if (buttons[i].offsetParent !== null && buttons[i].disabled === false) {
                            buttons[i].click();
                            return 'Clicked button: ' + (buttons[i].textContent || buttons[i].value || buttons[i].className);
                        }
                    }
                    return 'No clickable button found';
                ");
                Console.WriteLine($"JavaScript click result: {result}");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JavaScript click failed: {ex.Message}");
            }

            // Method 3: Use the base class DebugPageElements
            base.DebugPageElements();

            throw new Exception("No sign-in button could be found or clicked on the page");
        }

        // OTP-related methods
        public bool IsOTPPageDisplayed()
        {
            return IsElementVisible(OTPPageHeader, 10);
        }

        public void EnterOTP(string otp)
        {
            if (IsElementVisible(OTPInput, 10))
            {
                Console.WriteLine($"Entering OTP: {otp}");
                SendKeys(OTPInput, otp);
            }
            else
            {
                throw new Exception("OTP input field not found");
            }
        }

        public void ClickContinue()
        {
            if (IsElementVisible(ContinueButton))
            {
                Click(ContinueButton);
            }
            else
            {
                throw new Exception("Continue button not found");
            }
        }

        public void HandleMFAChallenge()
        {
            if (IsOTPPageDisplayed())
            {
                Console.WriteLine("MFA Challenge detected. Handling OTP verification...");

                TakeScreenshot("Before_OTP_Entry");

                // Get OTP
                string otp = OTPHandler.GetOTP();

                if (string.IsNullOrEmpty(otp))
                {
                    throw new Exception("Failed to retrieve OTP");
                }

                Console.WriteLine($"Entering OTP: {otp}");
                EnterOTP(otp);
                ClickContinue();

                Console.WriteLine("OTP entered. Waiting for verification...");
                System.Threading.Thread.Sleep(3000);

                TakeScreenshot("After_OTP_Entry");
            }
        }

        public void LoginWithOTP(string email, string password)
        {
            Console.WriteLine("Starting login process with MFA handling...");

            EnterEmail(email);
            EnterPassword(password);
            ClickSignIn();
            HandleMFAChallenge();

            WaitForPageToLoad();
            Console.WriteLine("Login process completed");
        }

        public void Login(string email, string password)
        {
            EnterEmail(email);
            EnterPassword(password);
            ClickSignIn();
        }

        public void LoginWithConfiguredUser()
        {
            var email = PropertyReader.GetPropertyValue("valid_username");
            var password = PropertyReader.GetPropertyValue("valid_password");
            LoginWithOTP(email, password);
        }

        public bool IsLoginPageLoaded()
        {
            return IsElementVisible(EmailTextBox) && IsElementVisible(PasswordTextBox);
        }

        public string GetErrorMessage()
        {
            // Wait a bit for error to potentially appear
            System.Threading.Thread.Sleep(2000);

            var errorLocators = new[]
            {
                // Common error message patterns
                By.XPath("//*[contains(@class, 'error')]"),
                By.XPath("//*[contains(@class, 'alert')]"),
                By.XPath("//*[contains(@class, 'danger')]"),
                By.XPath("//*[contains(@class, 'warning')]"),
                By.XPath("//*[contains(@class, 'invalid')]"),
                By.XPath("//*[contains(text(), 'error') or contains(text(), 'Error')]"),
                By.XPath("//*[contains(text(), 'invalid') or contains(text(), 'Invalid')]"),
                By.XPath("//*[contains(text(), 'incorrect') or contains(text(), 'Incorrect')]"),
                By.XPath("//*[contains(text(), 'wrong') or contains(text(), 'Wrong')]"),
                By.XPath("//*[contains(text(), 'failed') or contains(text(), 'Failed')]"),
                By.XPath("//*[@role='alert']"),
                By.XPath("//*[contains(@id, 'error')]"),
                By.XPath("//div[contains(@class, 'message')]"),
                // Add more specific locators based on your application
                By.XPath("//*[contains(@class, 'c')]") // Generic class pattern
            };

            foreach (var locator in errorLocators)
            {
                try
                {
                    var elements = Driver.FindElements(locator);
                    var visibleElement = elements.FirstOrDefault(e => e.Displayed && !string.IsNullOrEmpty(e.Text));
                    if (visibleElement != null && !string.IsNullOrWhiteSpace(visibleElement.Text))
                    {
                        string errorText = visibleElement.Text.Trim();
                        Console.WriteLine($"Found error message with locator {locator}: '{errorText}'");
                        return errorText;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with locator {locator}: {ex.Message}");
                    // Continue to next locator
                }
            }

            Console.WriteLine("No error message elements found with any locator");
            return string.Empty;
        }

        public bool IsErrorMessageDisplayed()
        {
            return !string.IsNullOrEmpty(GetErrorMessage());
        }

        public void DebugErrorElements()
        {
            Console.WriteLine("=== DEBUGGING ERROR ELEMENTS ===");

            // Get all visible elements that might contain text
            var allElements = Driver.FindElements(By.XPath("//*[text() != '']"));
            Console.WriteLine($"Found {allElements.Count} elements with text");

            foreach (var element in allElements.Where(e => e.Displayed))
            {
                string text = element.Text.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    string tagName = element.TagName;
                    string classes = element.GetAttribute("class");
                    Console.WriteLine($"Element: <{tagName} class='{classes}'> - Text: '{text}'");
                }
            }

            Console.WriteLine("=== END DEBUGGING ===");
        }
    }
}