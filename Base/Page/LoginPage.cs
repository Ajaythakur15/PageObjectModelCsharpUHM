using OpenQA.Selenium;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;
using System;
using System.Linq;

namespace PageObjectModelCsharp.Page
{
    public class LoginPage : BasePage
    {
        private readonly By EmailTextBox = By.XPath("//input[@id='username' or @name='username' or @type='email' or contains(@placeholder, 'email')]");
        private readonly By PasswordTextBox = By.XPath("//input[@id='password' or @name='password' or @type='password']");
        private readonly By OTPInput = By.XPath("//input[@name='otp' or contains(@placeholder, 'Enter the code') or @type='text']");
        private readonly By ContinueButton = By.XPath("//button[contains(text(), 'Continue')]");
        private readonly By OTPPageHeader = By.XPath("//*[contains(text(), 'Verify Your Identity') or contains(text(), 'Enter the code')]");

        public LoginPage(IWebDriver driver) : base(driver) { }

        public void EnterEmail(string email)
        {
            WaitForPageToLoad();
            PageActionHelper.Execute(Driver, () =>
            {
                Console.WriteLine($"Entering email: {email}");
                SendKeys(EmailTextBox, email);
            }, "Enter email");
        }

        public void EnterPassword(string password)
        {
            PageActionHelper.Execute(Driver, () =>
            {
                Console.WriteLine($"Entering password: {new string('*', password.Length)}");
                SendKeys(PasswordTextBox, password);
            }, "Enter password");
        }

        public void ClickSignIn()
        {
            TakeScreenshot("Before_SignIn_Click");

            var buttonLocators = new[]
            {
                By.XPath("//button[@class='c7ae0cd73 c91fa6616 ca65675d0 cbf0234ce cfa6fb59c']"),
                By.XPath("//button[contains(@class, 'c7ae0cd73')]"),
                By.XPath("//button[contains(text(), 'Sign In') or contains(text(), 'Login') or contains(text(), 'Continue') or @type='submit']"),
                By.CssSelector("button[type='submit']"),
                By.XPath("//input[@type='submit']"),
                By.XPath("//button"),
                By.XPath("//*[@role='button']")
            };

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
                            Console.WriteLine($"Clicking button: '{button.Text}'");
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
                            button.Click();
                        }, $"Click Sign In button via locator: {locator}");
                        return;
                    }
                }
            }

            PageActionHelper.Execute(Driver, () =>
            {
                Console.WriteLine("Trying JavaScript click fallback...");
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
            throw new Exception("No sign-in button could be found or clicked on the page");
        }

        public bool IsOTPPageDisplayed()
        {
            return IsElementVisible(OTPPageHeader, 10);
        }

        public void EnterOTP(string otp)
        {
            PageActionHelper.Execute(Driver, () =>
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

                System.Threading.Thread.Sleep(3000);
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
            System.Threading.Thread.Sleep(2000);

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
                    Console.WriteLine($"Found error message: '{errorText}'");
                    return errorText;
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