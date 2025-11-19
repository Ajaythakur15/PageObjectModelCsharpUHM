namespace PageObjectModelCsharp.Base
{
    public static class Constants
    {
        public static class Timeouts
        {
            public const int SHORT_TIMEOUT = 5;
            public const int MEDIUM_TIMEOUT = 15;
            public const int LONG_TIMEOUT = 30;
            public const int IMPLICIT_WAIT = 10;
        }

        public static class TestCategories
        {
            public const string SMOKE = "Smoke";
            public const string REGRESSION = "Regression";
            public const string LOGIN = "Login";
            public const string HOME = "Home";
            public const string DOC_SIGNING = "DocSigning";
            public const string DEBUG = "Debug";
        }

        public static class ErrorMessages
        {
            public const string LOGIN_ERROR = "Invalid username or password";
            public const string ELEMENT_NOT_FOUND = "Element not found within the specified timeout";
            public const string TIMEOUT_EXCEEDED = "Operation timed out";
            public const string PAGE_NOT_LOADED = "Page did not load completely";
        }

        public static class Selectors
        {
            public const string LOGIN_BUTTON = "#loginBtn";
            public const string USERNAME_FIELD = "#username";
            public const string PASSWORD_FIELD = "#password";
            public const string LOADER_SPINNER = ".loading-spinner";
        }

        public static class Roles
        {
            public const string ADMIN = "Admin";
            public const string USER = "User";
            public const string GUEST = "Guest";
            public const string AUDITOR = "Auditor";
        }

        public static class ReportLabels
        {
            public const string SCREENSHOT = "Screenshot";
            public const string DEBUG_SECTION = "Debug Info";
            public const string TEST_SUMMARY = "Test Summary";
            public const string ATTACHMENT = "Attachment";
        }
    }
}