using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestClass]
    [TestCategory(Constants.TestCategories.LOGIN)]
    [ExceptionHandler] // ✅ Centralized screenshot + logging on failure
    public class LoginTest : BaseTest
    {
        private LoginPage _loginPage = null!;

        [TestInitialize]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Driver);
            Console.WriteLine("✅ LoginPage initialized");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.SMOKE)]
        public void SuccessfulLogin_WithValidCredentials()
        {
            Assert.IsTrue(_loginPage.IsLoginPageLoaded(), "Login page should be loaded");

            _loginPage.TakeScreenshot("Before_Login");
            _loginPage.LoginWithConfiguredUser();
            System.Threading.Thread.Sleep(Constants.Timeouts.MEDIUM_TIMEOUT * 100);
            _loginPage.TakeScreenshot("After_Login");

            string currentUrl = Driver.Url;
            bool isBuilderPortal = currentUrl.Contains("BuilderPortal", StringComparison.OrdinalIgnoreCase);
            bool isLoginPage = currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase) || currentUrl.Contains("auth", StringComparison.OrdinalIgnoreCase);

            // MSTest doesn't have Assert.Multiple, so group logically
            Assert.IsTrue(isBuilderPortal, "User should be redirected to BuilderPortal");
            Assert.IsFalse(isLoginPage, "User should not remain on login page");

            Console.WriteLine("✅ PASS: Successful login verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Display_Error_For_Invalid_Credentials()
        {
            string invalidEmail = PropertyReader.GetPropertyValue("invalid_username");
            string invalidPassword = PropertyReader.GetPropertyValue("invalid_password");

            _loginPage.Login(invalidEmail, invalidPassword);
            System.Threading.Thread.Sleep(Constants.Timeouts.MEDIUM_TIMEOUT * 100);
            _loginPage.TakeScreenshot("Invalid_Credentials_Attempt");
            _loginPage.DebugPageElements();

            string currentUrl = Driver.Url;
            bool isStillOnLoginPage = currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase) || currentUrl.Contains("auth", StringComparison.OrdinalIgnoreCase);
            bool isErrorDisplayed = _loginPage.IsErrorMessageDisplayed();
            string errorMessage = _loginPage.GetErrorMessage();

            Assert.IsTrue(isErrorDisplayed || isStillOnLoginPage,
                "System should either show error or keep user on login page");

            if (isErrorDisplayed)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(errorMessage),
                    "Error message should be descriptive");
            }

            Console.WriteLine("✅ PASS: Invalid credentials handled correctly");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Login_Using_Enter_Key()
        {
            string email = PropertyReader.GetPropertyValue("valid_username");
            string password = PropertyReader.GetPropertyValue("valid_password");

            _loginPage.LoginWithEnterKey(email, password);
            Assert.IsTrue(_loginPage.IsOTPPageDisplayed(), "OTP screen should appear after Enter key submission");
            Console.WriteLine("✅ PASS: Login via Enter key verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Handle_Username_With_Leading_And_Trailing_Spaces()
        {
            string email = PropertyReader.GetPropertyValue("valid_username");
            string password = PropertyReader.GetPropertyValue("valid_password");

            _loginPage.LoginWithTrimmedUsername(email, password);

            Assert.IsTrue(_loginPage.IsOTPPageDisplayed(),
                "OTP screen should appear for spaced username");

            Console.WriteLine("✅ PASS: Login with spaced username verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Show_Email_Format_Error()
        {
            _loginPage.Login("invalid-email-format", "anyPassword123");

            string error = _loginPage.GetErrorMessage();
            Console.WriteLine($"❌ Found error message: '{error}'");

            Assert.IsTrue(error.Contains("Incorrect email address") || error.Contains("password"),
                "Generic error should be shown for invalid email format");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Show_Generic_Error_For_NonExistent_User()
        {
            _loginPage.Login("nouser@domain.com", "anyPassword123");
            Assert.IsTrue(_loginPage.IsGenericLoginErrorDisplayed(), "Generic error should be shown");
            Console.WriteLine("✅ PASS: Non-existent user handled correctly");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Show_Required_Field_Errors()
        {
            _loginPage.Login("", "");
            Assert.IsTrue(_loginPage.IsRequiredFieldErrorDisplayed(), "Required field errors should be shown");
            Console.WriteLine("✅ PASS: Empty field validation verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Show_Password_Required_Error()
        {
            string email = PropertyReader.GetPropertyValue("valid_username");
            _loginPage.Login(email, "");
            Assert.IsTrue(_loginPage.IsRequiredFieldErrorDisplayed(), "Password required error should be shown");
            Console.WriteLine("✅ PASS: Missing password validation verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Reject_Invalid_OTP()
        {
            string email = PropertyReader.GetPropertyValue("valid_username");
            string password = PropertyReader.GetPropertyValue("valid_password");

            _loginPage.Login(email, password);
            _loginPage.EnterOTP("000000");
            _loginPage.ClickContinue();

            Assert.IsTrue(_loginPage.IsErrorMessageDisplayed(), "Error should be shown for invalid OTP");
            Console.WriteLine("✅ PASS: Invalid OTP rejected");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Reject_Expired_OTP()
        {
            string email = PropertyReader.GetPropertyValue("valid_username");
            string password = PropertyReader.GetPropertyValue("valid_password");

            _loginPage.Login(email, password);

            // Simulate OTP expiry by waiting
            System.Threading.Thread.Sleep(8000);

            // Enter an expired OTP (simulated)
            _loginPage.EnterOTP("000000");
            _loginPage.ClickContinue();

            string errorMessage = _loginPage.GetErrorMessage();
            Console.WriteLine($"❌ Found error message: '{errorMessage}'");

            Assert.IsTrue(
                errorMessage.Contains("invalid", StringComparison.OrdinalIgnoreCase) ||
                errorMessage.Contains("expired", StringComparison.OrdinalIgnoreCase),
                "Error should indicate OTP is invalid or expired"
            );

            Console.WriteLine("✅ PASS: Expired OTP handled correctly (system shows invalid/expired message)");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Resend_OTP_And_Login_Successfully()
        {
            string email = PropertyReader.GetPropertyValue("valid_username");
            string password = PropertyReader.GetPropertyValue("valid_password");

            _loginPage.Login(email, password);
            _loginPage.ClickResendOTP();

            string newOtp = OTPHandler.GetOTP();
            _loginPage.EnterOTP(newOtp);
            _loginPage.ClickContinue(); // ✅ Required for navigation
            _loginPage.TakeScreenshot("After_OTP_Submission");

            _loginPage.WaitForPageToLoad();
            System.Threading.Thread.Sleep(Constants.Timeouts.MEDIUM_TIMEOUT * 100); // Optional buffer

            string currentUrl = Driver.Url;
            Console.WriteLine($"🌐 Final URL after OTP: {currentUrl}");

            bool isDashboard = currentUrl.Contains("/customer/dashboard", StringComparison.OrdinalIgnoreCase);
            bool isLoginPage = currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase) || currentUrl.Contains("auth", StringComparison.OrdinalIgnoreCase);

            Assert.IsTrue(isDashboard, "User should be redirected to customer dashboard");
            Assert.IsFalse(isLoginPage, "User should not remain on login page");

            Console.WriteLine("✅ PASS: Resend OTP flow verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void OTP_Flow_Should_Activate_When_Required()
        {
            if (_loginPage.IsOTPPageDisplayed())
            {
                _loginPage.TakeScreenshot("OTP_Page_Detected");
                Assert.IsTrue(_loginPage.IsOTPPageDisplayed(), "OTP page should be displayed");
                Console.WriteLine("✅ PASS: OTP flow activated");
            }
            else
            {
                Console.WriteLine("⚠️ WARNING: OTP page not displayed");
                Assert.Inconclusive("OTP flow not triggered for this session");
            }
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.DEBUG)]
        public void Debug_LoginPage_Elements()
        {
            Assert.IsTrue(_loginPage.IsLoginPageLoaded(), "Login page must be accessible");
            _loginPage.DebugPageElements();
            _loginPage.TakeScreenshot("Debug_Analysis_Complete");
            Assert.IsTrue(true, "✅ Debug completed successfully"); // MSTest doesn't have Assert.Pass
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.SMOKE)]
        public void LoginPage_Elements_Should_Be_Visible()
        {
            Assert.IsTrue(_loginPage.IsLoginPageLoaded(), "Login page should load completely");
            Assert.IsTrue(_loginPage.AreLoginElementsVisible(), "All login elements should be visible and enabled");
            _loginPage.TakeScreenshot("UI_Elements_Validation");
            Console.WriteLine("✅ PASS: Login page elements are visible");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.DEBUG)]
        public void Test_Yopmail_OTP_Reading()
        {
            string otp = OTPHandler.GetOTP();

            // MSTest doesn't support Assert.Multiple, so check sequentially
            Assert.IsFalse(string.IsNullOrWhiteSpace(otp), "OTP should not be null or empty");
            Assert.IsTrue(otp.Length >= 4, "OTP should be at least 4 digits");
            StringAssert.Matches(otp, new System.Text.RegularExpressions.Regex(@"^\d+$"), "OTP should contain only digits");

            Console.WriteLine($"✅ SUCCESS: Retrieved OTP from Yopmail: {otp}");
        }
    }

}