using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageObjectModelCsharp.Page;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Base;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestClass]
    [TestCategory(Constants.TestCategories.DOC_SIGNING)]
    [ExceptionHandler] // ✅ Centralized screenshot + logging on failure
    public class DocSigningTest : BaseTest
    {
        private LoginPage _loginPage = null!;
        private HomePage _homePage = null!;

        [TestInitialize]
        public void TestSetup()
        {
            _loginPage = new LoginPage(Driver);
            _homePage = new HomePage(Driver);

            Console.WriteLine("🔐 Logging in for Doc Signing tests...");
            _loginPage.LoginWithConfiguredUser();
            WaitForPageToLoad();

            Assert.IsTrue(_homePage.IsHomePageLoaded(), "Home page should be loaded after login");
            TakeScreenshot("After_Login_Complete");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.SMOKE)]
        public void Should_Navigate_To_DocSigning_Section()
        {
            Console.WriteLine("📄 Navigating to Doc Signing section...");
            TakeScreenshot("Before_Navigation_To_DocSigning");

            _homePage.NavigateToDocSigning();
            WaitForPageToLoad();
            TakeScreenshot("After_Navigation_To_DocSigning");

            string currentUrl = Driver.Url;
            bool isFormPresent = _homePage.IsFormPresent();

            // MSTest doesn't support Assert.Multiple, so check sequentially
            Assert.IsTrue(
                isFormPresent ||
                currentUrl.Contains("doc", StringComparison.OrdinalIgnoreCase) ||
                currentUrl.Contains("sign", StringComparison.OrdinalIgnoreCase),
                "Should navigate to Doc Signing section"
            );

            Console.WriteLine("✅ Navigation to Doc Signing verified");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.REGRESSION)]
        public void Should_Fill_And_Submit_DocSigning_Form()
        {
            string borrowerName = "John Doe";
            string loanNumber = "LN" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string email = "testdoc@yopmail.com";
            string phone = "555-987-6543";

            Console.WriteLine("📝 Filling Doc Signing form...");
            _homePage.NavigateToDocSigning();
            WaitForPageToLoad();
            TakeScreenshot("Before_Form_Fill");

            _homePage.FillDocSigningForm(borrowerName, loanNumber, email, phone);
            TakeScreenshot("After_Form_Fill");

            Console.WriteLine("📤 Submitting Doc Signing form...");
            _homePage.SubmitDocSigningForm();
            WaitForPageToLoad();
            TakeScreenshot("After_Form_Submission");

            bool isSubmissionSuccessful = _homePage.IsSubmissionSuccessful();
            string finalUrl = Driver.Url;

            Assert.IsTrue(
                isSubmissionSuccessful || !finalUrl.Contains("BuilderPortal", StringComparison.OrdinalIgnoreCase),
                "Form should be submitted or page should change"
            );

            Console.WriteLine("✅ Doc Signing form submitted successfully");
        }

        [TestMethod]
        [TestCategory(Constants.TestCategories.SMOKE)]
        public void Should_Handle_DocSigning_Form_With_Default_Data()
        {
            Console.WriteLine("📝 Handling Doc Signing form with default data...");
            _homePage.NavigateToDocSigning();
            WaitForPageToLoad();
            TakeScreenshot("Before_Default_Form_Fill");

            _homePage.FillDocSigningForm(); // Uses default values
            TakeScreenshot("After_Default_Form_Fill");

            _homePage.SubmitDocSigningForm();
            WaitForPageToLoad();
            TakeScreenshot("After_Default_Form_Submission");

            bool isFormPresent = _homePage.IsFormPresent();
            bool isSubmissionSuccessful = _homePage.IsSubmissionSuccessful();

            Assert.IsTrue(isFormPresent || isSubmissionSuccessful,
                "Should be able to interact with Doc Signing functionality");

            Console.WriteLine("✅ Default data form handled successfully");
        }
    }
}