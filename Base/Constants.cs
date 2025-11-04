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
        }
    }
}