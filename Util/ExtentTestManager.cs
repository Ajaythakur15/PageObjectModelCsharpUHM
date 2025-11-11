using AventStack.ExtentReports;
using System;
using System.Collections.Generic;

namespace PageObjectModelCsharp.Util
{
    public static class ExtentTestManager
    {
        private static readonly Dictionary<string, ExtentTest> _tests = new();

        public static int TotalTests { get; private set; } = 0;
        public static int PassedTests { get; private set; } = 0;
        public static int FailedTests { get; private set; } = 0;
        public static DateTime StartTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// Creates and registers a new test in the Extent report.
        /// </summary>
        public static ExtentTest CreateTest(string testName)
        {
            TotalTests++;
            var test = ExtentReportManager.GetExtent().CreateTest(testName);
            _tests[testName] = test;
            return test;
        }

        /// <summary>
        /// Retrieves an existing test by name. Returns null if not found.
        /// </summary>
        public static ExtentTest? GetTest(string testName)
        {
            return _tests.TryGetValue(testName, out var test) ? test : null;
        }

        /// <summary>
        /// Marks a test as passed and logs it in the report.
        /// </summary>
        public static void MarkTestAsPassed(string testName)
        {
            if (_tests.TryGetValue(testName, out var test))
            {
                PassedTests++;
                test.Pass("Test passed");
            }
        }

        /// <summary>
        /// Marks a test as failed and logs the failure message.
        /// </summary>
        public static void MarkTestAsFailed(string testName, string message)
        {
            if (_tests.TryGetValue(testName, out var test))
            {
                FailedTests++;
                test.Fail(message);
            }
        }
    }
}