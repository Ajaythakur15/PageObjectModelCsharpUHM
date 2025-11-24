using OpenQA.Selenium;
using PageObjectModelCsharp.Base;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;
using System;
using System.Linq;
using System.Threading;

namespace PageObjectModelCsharp.Page
{
    public class LoginPage : BasePage
    {
        private readonly By EmailTextBox = By.XPath("//input[@id='username' or @name='username' or @type='email' or contains(@placeholder, 'email')]");
        private readonly By PasswordTextBox = By.XPath("//input[@id='password' or @name='password' or @type='password']");
        private readonly By OTPInput = By.XPath("//input[@name='otp' or contains(@placeholder, 'Enter the code') or @type='text']");
        private readonly By ContinueButton = By.XPath("//button[contains(text(), 'Continue')]");
        private readonly By OTPPageHeader = By.XPath("//*[contains(text(), 'Verify Your Identity') or contains(text(), 'Enter the code')]");
        private readonly By ResendOTPButton = By.XPath("//button[contains(text(), 'Resend') or @id='resendOtpBtn']");

        private readonly By[] buttonLocators = new[]
        {
            By.XPath("//button[@class='c7ae0cd73 c91fa6616 ca65675d0 cbf0234ce cfa6fb59c']"),
            By.XPath("//button[contains(@class, 'c7ae0cd73')]"),
            By.XPath("//button[contains(text(), 'Sign In') or contains(text(), 'Login') or contains(text(), 'Continue') or @type='submit']"),
            By.CssSelector("button[type='submit']"),
            By.XPath("//input[@type='submit']"),
            By.XPath("//button"),
            By.XPath("//*[@role='button']")
        };

        public LoginPage(IWebDriver driver) : base(driver) { }

        public void EnterEmail(string email)
        {
            WaitForPageToLoad();
            PageActionHelper.Execute(Driver, () =>
            {
                Console.WriteLine($"📧 Entering email: {email}");
                SendKeys(EmailTextBox, email);
            }, "Enter email");
        }

        public void EnterPassword(string password)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                Console.WriteLine($"🔒 Entering password: {new string('*', password.Length)}");
                SendKeys(PasswordTextBox, password);
            }, "Enter password");
        }

        public void ClickSignIn()
        {
            TakeScreenshot("Before_SignIn_Click");

            foreach (var locator in buttonLocators)
            {
                var elements = Driver.FindElements(locator);
                if (elements.Count > 0)
                {
                    var button = elements.LastOrDefault(e => e.Displayed && e.Enabled) ?? elements.Last();
                    if (button != null)
                    {
                        PageActionHelper.Execute(Driver, () =>
                        {
                            Console.WriteLine($"🖱️ Clicking button: '{button.Text}'");
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
                            button.Click();
                        }, $"Click Sign In button via locator: {locator}");
                        return;
                    }
                }
            }

            PageActionHelper.Execute(Driver, () =>
            {
                Console.WriteLine("⚠️ Trying JavaScript click fallback...");
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
            }, "Fallback JavaScript click");

            DebugPageElements();
            throw new Exception(Constants.ErrorMessages.ELEMENT_NOT_FOUND);
        }

        public bool IsOTPPageDisplayed()
        {
            return IsElementVisible(OTPPageHeader, Constants.Timeouts.MEDIUM_TIMEOUT);
        }

        public void EnterOTP(string otp)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                if (IsElementVisible(OTPInput, Constants.Timeouts.MEDIUM_TIMEOUT))
                {
                    Console.WriteLine($"🔐 Entering OTP: {otp}");
                    SendKeys(OTPInput, otp);
                }
                else
                {
                    throw new Exception("OTP input field not found");
                }
            }, "Enter OTP");
        }

        public void ClickContinue()
        {
            PageActionHelper.Execute(Driver, () =>
            {
                if (IsElementVisible(ContinueButton))
                {
                    Click(ContinueButton);
                }
                else
                {
                    throw new Exception("Continue button not found");
                }
            }, "Click Continue");
        }

        public void HandleMFAChallenge()
        {
            if (IsOTPPageDisplayed())
            {
                TakeScreenshot("Before_OTP_Entry");

                string otp = OTPHandler.GetOTP();
                if (string.IsNullOrEmpty(otp))
                    throw new Exception("Failed to retrieve OTP");

                EnterOTP(otp);
                ClickContinue();

                Thread.Sleep(Constants.Timeouts.SHORT_TIMEOUT * 1000);
                TakeScreenshot("After_OTP_Entry");
            }
        }

        public void LoginWithOTP(string email, string password)
        {
            EnterEmail(email);
            EnterPassword(password);
            ClickSignIn();
            HandleMFAChallenge();
            WaitForPageToLoad();
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
            Thread.Sleep(Constants.Timeouts.SHORT_TIMEOUT * 1000);

            var errorLocators = new[]
            {
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
                By.XPath("//*[contains(@class, 'c')]")
            };

            foreach (var locator in errorLocators)
            {
                var elements = Driver.FindElements(locator);
                var visibleElement = elements.FirstOrDefault(e => e.Displayed && !string.IsNullOrEmpty(e.Text));
                if (visibleElement != null)
                {
                    string errorText = visibleElement.Text.Trim();
                    Console.WriteLine($"❌ Found error message: '{errorText}'");
                    return errorText;
                }
            }

            return string.Empty;
        }

        public bool IsErrorMessageDisplayed()
        {
            return !string.IsNullOrEmpty(GetErrorMessage());
        }

        public bool AreLoginElementsVisible()
        {
            return IsElementVisible(EmailTextBox)
                && IsElementVisible(PasswordTextBox)
                && buttonLocators.Any(locator => Driver.FindElements(locator).Any(e => e.Displayed && e.Enabled));
        }

        public void LoginWithEnterKey(string email, string password)
        {
            EnterEmail(email);
            EnterPassword(password);
            PressEnter(PasswordTextBox);
        }

        public void LoginWithTrimmedUsername(string email, string password)
        {
            string spacedEmail = $"  {email}  "; // simulate user input with spaces
            string trimmedEmail = spacedEmail.Trim(); // clean before sending
            LoginWithOTP(trimmedEmail, password);
        }

        public bool IsEmailFormatErrorDisplayed()
        {
            string error = GetErrorMessage();
            return error.Contains("valid email", StringComparison.OrdinalIgnoreCase)
                || error.Contains("invalid email", StringComparison.OrdinalIgnoreCase)
                || error.Contains("email format", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsGenericLoginErrorDisplayed()
        {
            string error = GetErrorMessage();
            return error.Contains("invalid", StringComparison.OrdinalIgnoreCase)
                || error.Contains("credentials", StringComparison.OrdinalIgnoreCase)
                || error.Contains("incorrect", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsRequiredFieldErrorDisplayed()
        {
            string error = GetErrorMessage();
            return error.Contains("required", StringComparison.OrdinalIgnoreCase)
                || error.Contains("missing", StringComparison.OrdinalIgnoreCase);
        }

        public void ClickResendOTP()
        {
            PageActionHelper.Execute(Driver, () =>
            {
                if (IsElementVisible(ResendOTPButton))
                {
                    Console.WriteLine("🔄 Clicking Resend OTP");
                    Click(ResendOTPButton);
                }
                else
                {
                    throw new Exception("Resend OTP button not found");
                }
            }, "Click Resend OTP");
        }

        public bool IsOTPExpiredMessageDisplayed()
        {
            string error = GetErrorMessage();
            return error.Contains("expired", StringComparison.OrdinalIgnoreCase)
                || error.Contains("timeout", StringComparison.OrdinalIgnoreCase)
                || error.Contains("resend", StringComparison.OrdinalIgnoreCase);
        }
    }
}