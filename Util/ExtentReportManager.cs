using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;
using System.Linq;

namespace PageObjectModelCsharp.Util
{
    public static class ExtentReportManager
    {
        private static ExtentReports _extent = new();
        [ThreadStatic]
        private static ExtentTest? _currentTest;

        private static string _reportEmail = "default";

        public static ExtentReports GetExtent()
        {
            return _extent;
        }

        public static void SetReportEmail(string email)
        {
            _reportEmail = email.Replace("@", "_at_").Replace(".", "_").Trim();
        }

        public static void InitReport()
        {
            string reportDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports", _reportEmail);
            Directory.CreateDirectory(reportDir);

            string reportPath = Path.Combine(reportDir, $"ExecutionReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            var sparkReporter = new ExtentSparkReporter(reportPath);

            sparkReporter.Config.DocumentTitle = "Automation Execution Report";
            sparkReporter.Config.ReportName = $"Test Execution Report - {_reportEmail}";

            _extent = new ExtentReports();
            _extent.AttachReporter(sparkReporter);

            _extent.AddSystemInfo("Environment", "UAT");
            _extent.AddSystemInfo("Browser", PropertyReader.GetPropertyValue("browser"));
            _extent.AddSystemInfo("Executed By", _reportEmail);

            Console.WriteLine($"📄 ExtentReport will be saved to: {reportPath}");
        }

        public static void CreateTest(string testName)
        {
            _currentTest = _extent.CreateTest(testName);
        }

        public static ExtentTest GetTest()
        {
            if (_currentTest is null)
                throw new InvalidOperationException("ExtentTest is not initialized. Call CreateTest() first.");
            return _currentTest;
        }

        public static void LogFailure(string testName, string message, string screenshotPath)
        {
            GetTest().Fail($"❌ {testName} failed: {message}")
                     .AddScreenCaptureFromPath(screenshotPath);
        }

        public static void FlushReport()
        {
            _extent.Flush();

            string reportDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports", _reportEmail);
            var latestReport = Directory.GetFiles(reportDir, "ExecutionReport_*.html")
                                        .OrderByDescending(File.GetCreationTime)
                                        .FirstOrDefault();

            if (latestReport is not null)
            {
                int totalTests = ExtentTestManager.TotalTests;
                int passedTests = ExtentTestManager.PassedTests;
                int failedTests = ExtentTestManager.FailedTests;
                TimeSpan duration = DateTime.Now - ExtentTestManager.StartTime;

                string summaryHtml = $@"
<html>
<head>
  <style>
    body {{ font-family: Arial, sans-serif; }}
    .summary {{ background-color: #f9f9f9; padding: 15px; border: 1px solid #ddd; }}
    .summary h2 {{ color: #2e6c80; }}
    .summary ul {{ list-style-type: none; padding-left: 0; }}
    .summary li {{ margin-bottom: 5px; }}
  </style>
</head>
<body>
  <div class='summary'>
    <h2>✅ Test Execution Summary</h2>
    <ul>
      <li><strong>Total Tests:</strong> {totalTests}</li>
      <li><strong>Passed:</strong> {passedTests}</li>
      <li><strong>Failed:</strong> {failedTests}</li>
      <li><strong>Duration:</strong> {duration:mm\\:ss} minutes</li>
    </ul>
    <p>📎 <strong>Report attached:</strong> {Path.GetFileName(latestReport)}</p>
  </div>
</body>
</html>";

                EmailSender.SendReport(latestReport, summaryHtml);
                System.Diagnostics.Process.Start("chrome.exe", latestReport);
            }
        }
    }
}