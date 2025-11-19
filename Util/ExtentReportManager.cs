using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PageObjectModelCsharp.Util.Helpers;

namespace PageObjectModelCsharp.Util
{
    public static class ExtentReportManager
    {
        private static ExtentReports _extent = new();
        [ThreadStatic]
        private static ExtentTest? _currentTest;

        private static string _reportEmail = "default";

        public static ExtentReports GetExtent() => _extent;

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
            _extent.AddSystemInfo("Browser", PropertyReader.GetPropertyValue("browser") ?? "Unknown");
            _extent.AddSystemInfo("Executed By", _reportEmail);

            Console.WriteLine($"📄 ExtentReport will be saved to: {reportPath}");
        }

        public static void CreateTest(string testName)
        {
            _currentTest = _extent.CreateTest(testName);
            ExtentTestManager.CreateTest(testName);
        }

        public static ExtentTest GetTest()
        {
            if (_currentTest is null)
                throw new InvalidOperationException("ExtentTest is not initialized. Call CreateTest() first.");
            return _currentTest;
        }

        public static void LogFailure(string testName, string message, string screenshotPath)
        {
            GetTest().Fail($"{testName} failed: {message}")
                     .AddScreenCaptureFromPath(screenshotPath);
            ExtentTestManager.MarkTestAsFailed(testName, message, screenshotPath);
        }

        public static async Task<(string zipPath, string htmlPath)> FlushReport()
        {
            _extent?.Flush();

            string reportDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports", _reportEmail);
            string? latestReport = Directory.GetFiles(reportDir, "ExecutionReport_*.html")
                                            .OrderByDescending(File.GetCreationTime)
                                            .FirstOrDefault();

            if (latestReport is not null)
            {
                int totalTests = ExtentTestManager.TotalTests;
                int passedTests = ExtentTestManager.PassedTests;
                int failedTests = ExtentTestManager.FailedTests;
                int skippedTests = ExtentTestManager.SkippedTests;
                TimeSpan duration = DateTime.Now - ExtentTestManager.StartTime;

                var rows = ExtentTestManager.TestResults.Select(result =>
                {
                    string durationFormatted = "-";
                    try
                    {
                        if (result.Duration is TimeSpan ts)
                            durationFormatted = ts.ToString(@"mm\:ss");
                        else if (TimeSpan.TryParse(result.Duration.ToString(), out var parsed))
                            durationFormatted = parsed.ToString(@"mm\:ss");
                    }
                    catch { durationFormatted = "-"; }

                    string screenshotCell = string.IsNullOrWhiteSpace(result.ScreenshotPath)
                        ? "-"
                        : $"<a href='{result.ScreenshotPath}'>View</a>";

                    return $@"
<tr>
  <td>{result.TestName}</td>
  <td>{result.Status}</td>
  <td>{durationFormatted}</td>
  <td>{screenshotCell}</td>
</tr>";
                });

                string summaryHtml = $@"
<html>
<head>
  <style>
    body {{ font-family: Arial, sans-serif; }}
    table {{ border-collapse: collapse; width: 100%; }}
    th, td {{ border: 1px solid #ddd; padding: 8px; }}
    th {{ background-color: #f2f2f2; }}
  </style>
</head>
<body>
  <h2>✅ Test Execution Summary</h2>
  <p><strong>Total:</strong> {totalTests} | <strong>Passed:</strong> {passedTests} | <strong>Failed:</strong> {failedTests} | <strong>Skipped:</strong> {skippedTests} | <strong>Duration:</strong> {duration:mm\\:ss}</p>
  <table>
    <tr><th>Test Name</th><th>Status</th><th>Duration</th><th>Screenshot</th></tr>
    {string.Join("", rows)}
  </table>
  <p>📎 <strong>Report attached:</strong> {Path.GetFileName(latestReport)}</p>
</body>
</html>";

                string apiKey = PropertyReader.GetPropertyValue("smtp_password");
                string recipient = PropertyReader.GetPropertyValue("email_to");

                string zipPath = ReportZipper.ZipReportFolder(reportDir);
                Console.WriteLine($"📁 Zipped report path: {zipPath}");
                Console.WriteLine($"📎 File exists: {File.Exists(zipPath)}");

                await SendGridMailer.SendEmailWithAttachmentAsync(apiKey, recipient, summaryHtml, zipPath);

                try
                {
                    System.Diagnostics.Process.Start("chrome.exe", latestReport);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not open report in browser: {ex.Message}");
                }

                return (zipPath, latestReport);
            }
            else
            {
                Console.WriteLine("⚠️ No report file found to send.");
                return (string.Empty, string.Empty);
            }
        }
    }
}