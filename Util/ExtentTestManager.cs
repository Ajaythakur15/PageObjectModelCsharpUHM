using AventStack.ExtentReports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjectModelCsharp.Util
{
    public class TestResult
    {
        public string TestName { get; set; } = "";
        public string Status { get; set; } = "Started";
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public string? ScreenshotPath { get; set; }
    }

    public static class ExtentTestManager
    {
        private static readonly Dictionary<string, ExtentTest> _tests = new();
        public static readonly List<TestResult> TestResults = new();

        public static int TotalTests => TestResults.Count;
        public static int PassedTests => TestResults.Count(r => r.Status == "Passed");
        public static int FailedTests => TestResults.Count(r => r.Status == "Failed");
        public static int SkippedTests => TestResults.Count(r => r.Status == "Skipped");
        public static DateTime StartTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// Creates and registers a new test in the Extent report and summary list.
        /// </summary>
        public static ExtentTest CreateTest(string testName)
        {
            var test = ExtentReportManager.GetExtent().CreateTest(testName);
            _tests[testName] = test;

            TestResults.Add(new TestResult { TestName = testName });
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
        /// Marks a test as passed and logs it in the report and summary.
        /// </summary>
        public static void MarkTestAsPassed(string testName)
        {
            if (_tests.TryGetValue(testName, out var test))
            {
                test.Pass("✅ Test passed");
            }

            UpdateResult(testName, "Passed");
        }

        /// <summary>
        /// Marks a test as failed and logs the failure message and screenshot.
        /// </summary>
        public static void MarkTestAsFailed(string testName, string message, string? screenshotPath = null)
        {
            if (_tests.TryGetValue(testName, out var test))
            {
                test.Fail($"❌ Test failed: {message}");
                if (!string.IsNullOrWhiteSpace(screenshotPath))
                {
                    test.AddScreenCaptureFromPath(screenshotPath);
                }
            }

            UpdateResult(testName, "Failed", screenshotPath);
        }

        /// <summary>
        /// Marks a test as skipped and logs the reason.
        /// </summary>
        public static void MarkTestAsSkipped(string testName, string reason)
        {
            if (_tests.TryGetValue(testName, out var test))
            {
                test.Skip($"⏭️ Test skipped: {reason}");
            }

            UpdateResult(testName, "Skipped");
        }

        /// <summary>
        /// Updates the test result metadata.
        /// </summary>
        private static void UpdateResult(string testName, string status, string? screenshotPath = null)
        {
            var result = TestResults.FirstOrDefault(r => r.TestName == testName);
            if (result != null)
            {
                result.Status = status;
                result.Duration = DateTime.Now - StartTime;
                result.ScreenshotPath = screenshotPath;
            }
        }
    }
}