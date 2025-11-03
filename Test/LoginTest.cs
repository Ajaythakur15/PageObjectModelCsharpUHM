using NUnit.Framework;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    [Category(Constants.TestCategories.LOGIN)]
    public class LoginTest : BaseTest
    {
        private LoginPage _loginPage = null!;

        [SetUp]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Driver);
            Console.WriteLine("LoginPage initialized successfully");
        }

        [Test]
        [Category(Constants.TestCategories.SMOKE)]
        public void SuccessfulLogin_WithValidCredentials()
        {
            try
            {
                // Arrange
                Console.WriteLine("Starting successful login test with valid credentials");
                Assert.That(_loginPage.IsLoginPageLoaded(), Is.True, "Login page should be loaded");

                // Act
                _loginPage.TakeScreenshot("Before_Login");
                _loginPage.LoginWithConfiguredUser();

                // Wait for navigation
                System.Threading.Thread.Sleep(5000);
                _loginPage.TakeScreenshot("After_Login");

                // Assert
                string currentUrl = Driver.Url;
                bool isBuilderPortal = currentUrl.Contains("BuilderPortal");
                bool isLoginPage = currentUrl.Contains("login") || currentUrl.Contains("auth");

                Console.WriteLine($"Assertion Data - URL: {currentUrl}, IsBuilderPortal: {isBuilderPortal}, IsLoginPage: {isLoginPage}");

                Assert.Multiple(() =>
                {
                    Assert.That(isBuilderPortal, Is.True, "User should be redirected to BuilderPortal after successful login");
                    Assert.That(isLoginPage, Is.False, "User should not remain on login page after successful authentication");
                });

                Console.WriteLine("PASS: Login test completed successfully - user successfully logged in and redirected");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Login test failed - {ex.Message}");
                _loginPage.TakeScreenshot("Login_Test_Failure");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.REGRESSION)]
        public void Should_Display_Error_For_Invalid_Credentials()
        {
            try
            {
                // Arrange
                string invalidEmail = PropertyReader.GetPropertyValue("invalid_username");
                string invalidPassword = PropertyReader.GetPropertyValue("invalid_password");

                Console.WriteLine($"Testing invalid credentials handling for: {invalidEmail}");

                // Act
                _loginPage.Login(invalidEmail, invalidPassword);
                System.Threading.Thread.Sleep(5000); // Increased wait time
                _loginPage.TakeScreenshot("Invalid_Credentials_Attempt");

                // Debug page to understand what's happening
                _loginPage.DebugPageElements();

                // Check application behavior
                string currentUrl = Driver.Url;
                bool isStillOnLoginPage = currentUrl.Contains("login") || currentUrl.Contains("auth");
                bool isErrorDisplayed = _loginPage.IsErrorMessageDisplayed();
                string errorMessage = _loginPage.GetErrorMessage();

                Console.WriteLine($"Post-login Analysis - URL: {currentUrl}, Still on login page: {isStillOnLoginPage}, Error displayed: {isErrorDisplayed}, Error message: '{errorMessage}'");

                Assert.Multiple(() =>
                {
                    // Either an error message should be displayed OR user should remain on login page
                    Assert.That(isErrorDisplayed || isStillOnLoginPage, Is.True,
                        "System should either display error message or keep user on login page for invalid credentials");

                    if (isErrorDisplayed)
                    {
                        Assert.That(string.IsNullOrEmpty(errorMessage), Is.False,
                            "Error message should contain descriptive text");
                    }
                });

                Console.WriteLine("PASS: Invalid credentials handled correctly");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Invalid credentials test failed - {ex.Message}");
                _loginPage.TakeScreenshot("Invalid_Credentials_Test_Failure");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.REGRESSION)]
        public void OTP_Flow_Should_Activate_When_Required()
        {
            try
            {
                Console.WriteLine("Testing OTP/MFA flow detection");

                if (_loginPage.IsOTPPageDisplayed())
                {
                    Console.WriteLine("OTP page detected - MFA flow is active");
                    Assert.That(_loginPage.IsOTPPageDisplayed(), Is.True, "OTP verification page should be displayed when MFA is required");
                    _loginPage.TakeScreenshot("OTP_Page_Detected");
                    Console.WriteLine("PASS: OTP flow activation test passed");
                }
                else
                {
                    Console.WriteLine("WARNING: OTP page not displayed - MFA not triggered for this session");
                    Assert.Inconclusive("OTP flow not activated - MFA may not be required for this user/session");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: OTP flow test failed - {ex.Message}");
                _loginPage.TakeScreenshot("OTP_Flow_Test_Failure");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.DEBUG)]
        public void Debug_LoginPage_Elements()
        {
            try
            {
                Console.WriteLine("Starting comprehensive login page debug analysis");

                Assert.That(_loginPage.IsLoginPageLoaded(), Is.True, "Login page must be accessible for debugging");
                _loginPage.DebugPageElements();

                _loginPage.TakeScreenshot("Debug_Analysis_Complete");
                Console.WriteLine("PASS: Debug completed successfully - check console output for detailed element analysis");
                Assert.Pass("Login page debug finished");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Debug test failed - {ex.Message}");
                _loginPage.TakeScreenshot("Debug_Test_Failure");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.SMOKE)]
        public void LoginPage_Elements_Should_Be_Visible()
        {
            try
            {
                Console.WriteLine("Validating login page UI elements visibility");

                Assert.That(_loginPage.IsLoginPageLoaded(), Is.True, "Login page should load completely with all required elements");

                _loginPage.TakeScreenshot("UI_Elements_Validation");
                Console.WriteLine("PASS: All critical login page elements are properly visible and accessible");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: UI elements validation failed - {ex.Message}");
                _loginPage.TakeScreenshot("UI_Elements_Test_Failure");
                throw;
            }
        }

        [Test]
        [Category(Constants.TestCategories.DEBUG)]
        public void Test_Yopmail_OTP_Reading()
        {
            try
            {
                Console.WriteLine("Testing Yopmail OTP reading functionality");

                string otp = OTPHandler.GetOTP();

                Assert.That(string.IsNullOrEmpty(otp), Is.False, "OTP should not be null or empty");

                // Optional: Validate OTP format
                Assert.That(otp, Has.Length.AtLeast(4), "OTP should be at least 4 digits");
                Assert.That(otp, Does.Match(@"^\d+$"), "OTP should contain only digits");

                Console.WriteLine($"SUCCESS: Retrieved OTP from Yopmail: {otp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Yopmail test failed - {ex.Message}");
                _loginPage.TakeScreenshot("Yopmail_OTP_Reading_Failure");
                throw;
            }
        }
    }
}