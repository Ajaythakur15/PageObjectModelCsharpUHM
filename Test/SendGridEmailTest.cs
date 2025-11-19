using NUnit.Framework;
using PageObjectModelCsharp.Util;
using PageObjectModelCsharp.Util.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    public class SendGridEmailTest
    {
        [Test]
        public async Task SendBasicEmailSummary()
        {
            string apiKey = PropertyReader.GetPropertyValue("smtp_password");
            string recipient = PropertyReader.GetPropertyValue("email_to");

            string htmlContent = @"
<html>
<head><style>body { font-family: Arial; }</style></head>
<body>
  <h2>✅ SendGrid Email Test</h2>
  <p>This is a test email sent via SendGrid REST API.</p>
</body>
</html>";

            // Match folder name used in SetReportEmail()
            string reportEmail = PropertyReader.GetPropertyValue("testEmail", "default@user.com");
            string reportFolder = reportEmail.Replace("@", "_at_").Replace(".", "_").Trim();
            string reportDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports", reportFolder);

            if (!Directory.Exists(reportDir))
            {
                Directory.CreateDirectory(reportDir);
                File.WriteAllText(Path.Combine(reportDir, "DummyReport.html"), htmlContent);
                Console.WriteLine($"📁 Created missing folder: {reportDir}");
            }

            string zipPath = ReportZipper.ZipReportFolder(reportDir);
            Console.WriteLine($"📦 Zipped report folder: {zipPath}");

            await SendGridMailer.SendEmailWithAttachmentAsync(apiKey, recipient, htmlContent, zipPath);
            Console.WriteLine("✅ Email sent successfully with attachment");
        }
    }
}