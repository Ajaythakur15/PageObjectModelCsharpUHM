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
            var errorLocators = new[]
            {
                By.XPath("//*[contains(@class, 'error')]"),
                By.XPath("//*[contains(text(), 'error') or contains(text(), 'invalid')]"),
                By.XPath("//*[@role='alert']")
            };

            foreach (var locator in errorLocators)
            {
                try
                {
                    var elements = Driver.FindElements(locator);
                    var visibleElement = elements.FirstOrDefault(e => e.Displayed);
                    if (visibleElement != null)
                    {
                        return visibleElement.Text;
                    }
                }
                catch
                {
                    // Continue to next locator
                }
            }

            return string.Empty;
        }

        public bool IsErrorMessageDisplayed()
        {
            return !string.IsNullOrEmpty(GetErrorMessage());
        }
    }
}