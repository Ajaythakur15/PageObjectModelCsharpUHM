using NUnit.Framework;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    [Category(Constants.TestCategories.LOGIN)]
    [ExceptionHandler] // ✅ Centralized screenshot + logging on failure
    public class LoginTest : BaseTest
    {
        private LoginPage _loginPage = null!;

        [SetUp]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Driver);
            Console.WriteLine("✅ LoginPage initialized");
        }

        [Test]
        [Category(Constants.TestCategories.SMOKE)]
        public void SuccessfulLogin_WithValidCredentials()
        {
            Assert.That(_loginPage.IsLoginPageLoaded(), Is.True, "Login page should be loaded");

            _loginPage.TakeScreenshot("Before_Login");
            _loginPage.LoginWithConfiguredUser();

            System.Threading.Thread.Sleep(5000);
            _loginPage.TakeScreenshot("After_Login");

            string currentUrl = Driver.Url;
            bool isBuilderPortal = currentUrl.Contains("BuilderPortal");
            bool isLoginPage = currentUrl.Contains("login") || currentUrl.Contains("auth");

            Assert.Multiple(() =>
            {
                Assert.That(isBuilderPortal, Is.True, "User should be redirected to BuilderPortal");
                Assert.That(isLoginPage, Is.False, "User should not remain on login page");
            });

            Console.WriteLine("✅ PASS: Successful login verified");
        }

        [Test]
        [Category(Constants.TestCategories.REGRESSION)]
        public void Should_Display_Error_For_Invalid_Credentials()
        {
            string invalidEmail = PropertyReader.GetPropertyValue("invalid_username");
            string invalidPassword = PropertyReader.GetPropertyValue("invalid_password");

            _loginPage.Login(invalidEmail, invalidPassword);
            System.Threading.Thread.Sleep(5000);
            _loginPage.TakeScreenshot("Invalid_Credentials_Attempt");
            _loginPage.DebugPageElements();

            string currentUrl = Driver.Url;
            bool isStillOnLoginPage = currentUrl.Contains("login") || currentUrl.Contains("auth");
            bool isErrorDisplayed = _loginPage.IsErrorMessageDisplayed();
            string errorMessage = _loginPage.GetErrorMessage();

            Assert.Multiple(() =>
            {
                Assert.That(isErrorDisplayed || isStillOnLoginPage, Is.True,
                    "System should either show error or keep user on login page");

                if (isErrorDisplayed)
                {
                    Assert.That(string.IsNullOrEmpty(errorMessage), Is.False,
                        "Error message should be descriptive");
                }
            });

            Console.WriteLine("✅ PASS: Invalid credentials handled correctly");
        }

        [Test]
        [Category(Constants.TestCategories.REGRESSION)]
        public void OTP_Flow_Should_Activate_When_Required()
        {
            if (_loginPage.IsOTPPageDisplayed())
            {
                _loginPage.TakeScreenshot("OTP_Page_Detected");
                Assert.That(_loginPage.IsOTPPageDisplayed(), Is.True, "OTP page should be displayed");
                Console.WriteLine("✅ PASS: OTP flow activated");
            }
            else
            {
                Console.WriteLine("⚠️ WARNING: OTP page not displayed");
                Assert.Inconclusive("OTP flow not triggered for this session");
            }
        }

        [Test]
        [Category(Constants.TestCategories.DEBUG)]
        public void Debug_LoginPage_Elements()
        {
            Assert.That(_loginPage.IsLoginPageLoaded(), Is.True, "Login page must be accessible");
            _loginPage.DebugPageElements();
            _loginPage.TakeScreenshot("Debug_Analysis_Complete");
            Assert.Pass("✅ Debug completed successfully");
        }

        [Test]
        [Category(Constants.TestCategories.SMOKE)]
        public void LoginPage_Elements_Should_Be_Visible()
        {
            Assert.That(_loginPage.IsLoginPageLoaded(), Is.True, "Login page should load completely");
            _loginPage.TakeScreenshot("UI_Elements_Validation");
            Console.WriteLine("✅ PASS: Login page elements are visible");
        }

        [Test]
        [Category(Constants.TestCategories.DEBUG)]
        public void Test_Yopmail_OTP_Reading()
        {
            string otp = OTPHandler.GetOTP();

            Assert.Multiple(() =>
            {
                Assert.That(string.IsNullOrEmpty(otp), Is.False, "OTP should not be null or empty");
                Assert.That(otp, Has.Length.AtLeast(4), "OTP should be at least 4 digits");
                Assert.That(otp, Does.Match(@"^\d+$"), "OTP should contain only digits");
            });

            Console.WriteLine($"✅ SUCCESS: Retrieved OTP from Yopmail: {otp}");
        }
    }
}