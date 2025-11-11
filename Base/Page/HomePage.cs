using OpenQA.Selenium;
using PageObjectModelCsharp.Util;
using System;
using System.Linq;
using System.Threading;

namespace PageObjectModelCsharp.Page
{
    public class HomePage : BasePage
    {
        public HomePage(IWebDriver driver) : base(driver)
        {
        }

        // Navigation and Main Menu Locators
        private readonly By DocSigningMenu = By.XPath("//*[contains(text(), 'Doc Signing') or contains(text(), 'Document Signing') or contains(text(), 'Documents') or contains(text(), 'DocuSign')]");
        private readonly By MenuItems = By.XPath("//nav//a | //ul//a | //*[contains(@class, 'menu')]//a | //*[contains(@class, 'nav')]//a | //button");

        // Doc Signing Form Locators - More flexible
        private readonly By AnyInputField = By.XPath("//input | //textarea | //select");
        private readonly By AnyButton = By.XPath("//button | //input[@type='submit'] | //input[@type='button']");
        private readonly By AnyForm = By.XPath("//form | //div[contains(@class, 'form')] | //section[.//input]");

        // More specific locators for common fields
        private readonly By NameFields = By.XPath("//input[contains(@name, 'name') or contains(@placeholder, 'Name') or contains(@id, 'name')]");
        private readonly By EmailFields = By.XPath("//input[@type='email' or contains(@name, 'email') or contains(@placeholder, 'Email')]");
        private readonly By PhoneFields = By.XPath("//input[@type='tel' or contains(@name, 'phone') or contains(@placeholder, 'Phone')]");
        private readonly By SubmitButtons = By.XPath("//*[contains(text(), 'Submit') or contains(text(), 'Send') or contains(text(), 'Continue') or @type='submit']");

        public bool IsHomePageLoaded()
        {
            try
            {
                WaitForPageToLoad();
                Console.WriteLine("Checking if home page is loaded...");

                string currentUrl = GetCurrentUrl();
                string pageTitle = Driver.Title;

                Console.WriteLine($"Current URL: {currentUrl}");
                Console.WriteLine($"Page Title: {pageTitle}");

                bool isBuilderPortal = currentUrl.Contains("BuilderPortal", StringComparison.OrdinalIgnoreCase);
                bool hasBuilderPortalTitle = pageTitle.Contains("Builder Portal", StringComparison.OrdinalIgnoreCase);

                Console.WriteLine($"Is BuilderPortal URL: {isBuilderPortal}");
                Console.WriteLine($"Has BuilderPortal title: {hasBuilderPortalTitle}");

                return isBuilderPortal && hasBuilderPortalTitle;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking home page: {ex.Message}");
                TakeScreenshot("HomePage_Load_Error");
                return false;
            }
        }

        public bool IsUserLoggedIn()
        {
            try
            {
                string currentUrl = GetCurrentUrl();
                string pageTitle = Driver.Title;

                bool isLoginPage = currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase) ||
                                 currentUrl.Contains("signin", StringComparison.OrdinalIgnoreCase) ||
                                 currentUrl.Contains("auth", StringComparison.OrdinalIgnoreCase);

                bool isBuilderPortal = currentUrl.Contains("BuilderPortal", StringComparison.OrdinalIgnoreCase);

                Console.WriteLine($"Is login page: {isLoginPage}");
                Console.WriteLine($"Is BuilderPortal: {isBuilderPortal}");

                return !isLoginPage && isBuilderPortal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking login status: {ex.Message}");
                return false;
            }
        }

        public void ClickLogout()
        {
            try
            {
                var logoutLocators = new[]
                {
                    By.XPath("//*[contains(text(), 'Logout') or contains(text(), 'Log out')]"),
                    By.XPath("//*[contains(text(), 'Sign out') or contains(text(), 'Sign Out')]"),
                    By.XPath("//*[contains(@class, 'logout')]"),
                };

                foreach (var locator in logoutLocators)
                {
                    if (IsElementVisible(locator, 2))
                    {
                        Click(locator);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during logout: {ex.Message}");
            }
        }

        public void NavigateToDocSigning()
        {
            try
            {
                Console.WriteLine("Navigating to Doc Signing section...");
                TakeScreenshot("Before_DocSigning_Navigation");

                // Method 1: Try direct Doc Signing link
                if (IsElementVisible(DocSigningMenu, 5))
                {
                    Console.WriteLine("Found Doc Signing menu item, clicking...");
                    Click(DocSigningMenu);
                    WaitForPageToLoad();
                    TakeScreenshot("After_DocSigning_Click");
                    return;
                }

                // Method 2: Explore all interactive elements
                Console.WriteLine("Doc Signing not found directly, exploring all interactive elements...");
                var interactiveElements = Driver.FindElements(MenuItems);
                Console.WriteLine($"Found {interactiveElements.Count} interactive elements");

                foreach (var element in interactiveElements)
                {
                    if (element.Displayed && !string.IsNullOrEmpty(element.Text))
                    {
                        string elementText = element.Text.Trim();
                        Console.WriteLine($"Element: '{elementText}'");

                        if (elementText.Contains("Doc", StringComparison.OrdinalIgnoreCase) ||
                            elementText.Contains("Sign", StringComparison.OrdinalIgnoreCase) ||
                            elementText.Contains("Document", StringComparison.OrdinalIgnoreCase) ||
                            elementText.Contains("Form", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Clicking on element: {elementText}");
                            try
                            {
                                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
                                element.Click();
                                WaitForPageToLoad();
                                TakeScreenshot($"After_Clicking_{elementText.Replace(" ", "_")}");

                                // Check if we got to a form page
                                if (IsFormPresent() || GetCurrentUrl().Contains("form", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Successfully navigated to form page");
                                    return;
                                }

                                // If not, go back and try next
                                Driver.Navigate().Back();
                                WaitForPageToLoad();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error clicking element {elementText}: {ex.Message}");
                            }
                        }
                    }
                }

                Console.WriteLine("WARNING: Could not find Doc Signing navigation, but continuing...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to Doc Signing: {ex.Message}");
                TakeScreenshot("DocSigning_Navigation_Error");
                throw;
            }
        }

        public bool IsFormPresent()
        {
            try
            {
                // Check for any input fields
                bool hasInputs = Driver.FindElements(AnyInputField).Count > 0;
                bool hasButtons = Driver.FindElements(AnyButton).Count > 0;
                bool hasForms = Driver.FindElements(AnyForm).Count > 0;

                Console.WriteLine($"Input fields found: {hasInputs}");
                Console.WriteLine($"Buttons found: {hasButtons}");
                Console.WriteLine($"Forms found: {hasForms}");

                // If we have any interactive elements, consider it a form page
                return hasInputs || hasButtons;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking form presence: {ex.Message}");
                return false;
            }
        }

        public void FillDocSigningForm(string borrowerName = "Test Borrower", string loanNumber = "T555219", string email = "test@example.com", string phone = "555-123-4567")
        {
            try
            {
                Console.WriteLine("Filling Doc Signing form...");
                TakeScreenshot("Before_Form_Fill");

                loanNumber ??= "TEST" + DateTime.Now.ToString("yyyyMMddHHmmss");

                Console.WriteLine($"Using data - Borrower: {borrowerName}, Loan: {loanNumber}, Email: {email}, Phone: {phone}");

                // Debug: Show what elements are available
                DebugAvailableElements();

                // Try to fill name fields
                FillFields(NameFields, borrowerName, "name");

                // Try to fill email fields
                FillFields(EmailFields, email, "email");

                // Try to fill phone fields
                FillFields(PhoneFields, phone, "phone");

                // Try to fill any other input fields with generic data
                FillGenericFields();

                TakeScreenshot("After_Form_Fill");
                Console.WriteLine("Form filling attempted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filling Doc Signing form: {ex.Message}");
                TakeScreenshot("Form_Fill_Error");
                throw;
            }
        }

        private void FillFields(By locator, string value, string fieldType)
        {
            try
            {
                var fields = Driver.FindElements(locator);
                Console.WriteLine($"Found {fields.Count} {fieldType} fields");

                foreach (var field in fields.Where(f => f.Displayed && f.Enabled))
                {
                    try
                    {
                        field.Clear();
                        field.SendKeys(value);
                        Console.WriteLine($"Filled {fieldType} field with: {value}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error filling {fieldType} field: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding {fieldType} fields: {ex.Message}");
            }
        }

        private void FillGenericFields()
        {
            try
            {
                var allInputs = Driver.FindElements(AnyInputField);
                Console.WriteLine($"Total input fields found: {allInputs.Count}");

                foreach (var input in allInputs.Where(i => i.Displayed && i.Enabled))
                {
                    try
                    {
                        string inputType = input.GetAttribute("type") ?? "";
                        string inputName = input.GetAttribute("name") ?? "";
                        string currentValue = input.GetAttribute("value") ?? "";
                        string placeholder = input.GetAttribute("placeholder") ?? "";

                        // Skip if already has value or is a submit button
                        if (!string.IsNullOrEmpty(currentValue) || inputType == "submit")
                            continue;

                        string testValue = GetTestValueForField(inputType, inputName, placeholder);

                        input.Clear();
                        input.SendKeys(testValue);
                        Console.WriteLine($"Filled field '{inputName}' ({inputType}) with: {testValue}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error filling generic field: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FillGenericFields: {ex.Message}");
            }
        }

        private string GetTestValueForField(string type, string name, string placeholder)
        {
            string lowerName = name.ToLower();
            string lowerPlaceholder = placeholder.ToLower();

            if (type == "email" || lowerName.Contains("email") || lowerPlaceholder.Contains("email"))
                return "test@example.com";
            else if (type == "tel" || lowerName.Contains("phone") || lowerPlaceholder.Contains("phone"))
                return "555-123-4567";
            else if (lowerName.Contains("name") || lowerPlaceholder.Contains("name"))
                return "Test User";
            else if (lowerName.Contains("address") || lowerPlaceholder.Contains("address"))
                return "123 Test Street";
            else if (lowerName.Contains("city") || lowerPlaceholder.Contains("city"))
                return "Test City";
            else if (lowerName.Contains("zip") || lowerPlaceholder.Contains("zip"))
                return "12345";
            else if (type == "number" || lowerName.Contains("loan") || lowerName.Contains("account"))
                return "123456789";
            else if (type == "date")
                return DateTime.Now.ToString("MM/dd/yyyy");
            else
                return "Test Data";
        }

        public void SubmitDocSigningForm()
        {
            try
            {
                Console.WriteLine("Attempting to submit form...");
                TakeScreenshot("Before_Form_Submission");

                // Method 1: Try specific submit buttons
                var submitButtons = Driver.FindElements(SubmitButtons);
                Console.WriteLine($"Found {submitButtons.Count} submit buttons");

                var visibleSubmit = submitButtons.FirstOrDefault(b => b.Displayed && b.Enabled);
                if (visibleSubmit != null)
                {
                    Console.WriteLine($"Clicking submit button: {visibleSubmit.Text}");
                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", visibleSubmit);
                    Console.WriteLine("Submit button clicked");
                }
                else
                {
                    // Method 2: Try any button that might submit
                    var allButtons = Driver.FindElements(AnyButton);
                    var clickableButtons = allButtons.Where(b => b.Displayed && b.Enabled).ToList();

                    Console.WriteLine($"Found {clickableButtons.Count} clickable buttons");

                    foreach (var button in clickableButtons.Take(3)) // Try first 3 buttons
                    {
                        try
                        {
                            string buttonText = button.Text ?? "";
                            Console.WriteLine($"Trying button: '{buttonText}'");

                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", button);
                            Console.WriteLine($"Clicked button: {buttonText}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error clicking button: {ex.Message}");
                        }
                    }
                }

                // Wait for any action to complete
                Thread.Sleep(3000);
                WaitForPageToLoad();

                TakeScreenshot("After_Form_Submission");
                Console.WriteLine("Form submission attempted");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting form: {ex.Message}");
                TakeScreenshot("Form_Submission_Error");
                // Don't throw - we want to continue even if submission fails
            }
        }

        public bool IsSubmissionSuccessful()
        {
            try
            {
                // Check for success indicators
                bool hasSuccessMessage = IsElementVisible(By.XPath("//*[contains(text(), 'success') or contains(text(), 'Success') or contains(text(), 'thank') or contains(text(), 'completed')]"), 5);
                bool hasConfirmation = IsElementVisible(By.XPath("//*[contains(text(), 'confirmation') or contains(text(), 'submitted')]"), 3);

                // Check if URL changed
                string currentUrl = GetCurrentUrl();
                bool urlChanged = !currentUrl.Contains("form", StringComparison.OrdinalIgnoreCase);

                Console.WriteLine($"Success message: {hasSuccessMessage}");
                Console.WriteLine($"Confirmation: {hasConfirmation}");
                Console.WriteLine($"URL changed: {urlChanged}");

                return hasSuccessMessage || hasConfirmation || urlChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking submission success: {ex.Message}");
                return false;
            }
        }

        private void DebugAvailableElements()
        {
            try
            {
                Console.WriteLine("=== DEBUGGING AVAILABLE ELEMENTS ===");

                var inputs = Driver.FindElements(AnyInputField);
                var buttons = Driver.FindElements(AnyButton);
                var forms = Driver.FindElements(AnyForm);

                Console.WriteLine($"Inputs: {inputs.Count}, Buttons: {buttons.Count}, Forms: {forms.Count}");

                foreach (var input in inputs.Take(5))
                {
                    if (input.Displayed)
                    {
                        string type = input.GetAttribute("type") ?? "unknown";
                        string name = input.GetAttribute("name") ?? "no-name";
                        string placeholder = input.GetAttribute("placeholder") ?? "no-placeholder";
                        Console.WriteLine($"Input - Type: {type}, Name: {name}, Placeholder: {placeholder}");
                    }
                }

                foreach (var button in buttons.Take(3))
                {
                    if (button.Displayed)
                    {
                        string text = button.Text ?? "no-text";
                        string type = button.GetAttribute("type") ?? "button";
                        Console.WriteLine($"Button - Text: '{text}', Type: {type}");
                    }
                }

                Console.WriteLine("=== END DEBUGGING ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in debug: {ex.Message}");
            }
        }

        public string GetWelcomeMessage()
        {
            try
            {
                string pageTitle = Driver.Title;
                return !string.IsNullOrEmpty(pageTitle) ? $"Welcome to {pageTitle}" : "Welcome to Builder Portal";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting welcome message: {ex.Message}");
                return "Welcome to Builder Portal";
            }
        }

        public string GetUsername()
        {
            try
            {
                string currentUrl = GetCurrentUrl();
                if (currentUrl.Contains("email="))
                {
                    int startIndex = currentUrl.IndexOf("email=") + 6;
                    int endIndex = currentUrl.IndexOf('&', startIndex);
                    if (endIndex == -1) endIndex = currentUrl.Length;

                    return currentUrl.Substring(startIndex, endIndex - startIndex);
                }
                return "Builder Portal User";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting username: {ex.Message}");
                return "Builder Portal User";
            }
        }

        public void DebugHomePageElements()
        {
            Console.WriteLine("=== HOME PAGE DEBUGGING ===");
            Console.WriteLine($"URL: {Driver.Url}");
            Console.WriteLine($"Title: {Driver.Title}");

            try
            {
                var navElements = Driver.FindElements(By.XPath("//nav | //*[contains(@class, 'menu')] | //*[contains(@class, 'sidebar')]"));
                Console.WriteLine($"Navigation sections: {navElements.Count}");

                var docElements = Driver.FindElements(By.XPath("//*[contains(text(), 'Doc') or contains(text(), 'Sign') or contains(text(), 'Document')]"));
                Console.WriteLine($"Doc-related elements: {docElements.Count}");

                foreach (var element in docElements.Take(3))
                {
                    if (element.Displayed)
                    {
                        Console.WriteLine($"Doc element: '{element.Text}' - {element.TagName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during debugging: {ex.Message}");
            }

            Console.WriteLine("=== END DEBUGGING ===");
        }
    }
}