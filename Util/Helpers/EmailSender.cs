using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using PageObjectModelCsharp.Util; // For PropertyReader

namespace PageObjectModelCsharp.Util
{
    public static class EmailSender
    {
        public static void SendReport(string reportPath, string summaryHtml)
        {
            // ✅ Validate report file exists
            if (!File.Exists(reportPath))
                throw new FileNotFoundException($"Report file not found: {reportPath}");

            // ✅ Read SMTP and recipient settings from App.properties
            string smtpHost = PropertyReader.GetPropertyValue("smtp_host");
            int smtpPort = int.Parse(PropertyReader.GetPropertyValue("smtp_port"));
            string smtpUser = PropertyReader.GetPropertyValue("smtp_user");
            string smtpPass = PropertyReader.GetPropertyValue("smtp_password");
            string recipients = PropertyReader.GetPropertyValue("email_to");

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPass) || string.IsNullOrWhiteSpace(recipients))
            {
                throw new InvalidOperationException("SMTP configuration or recipient list is incomplete.");
            }

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(smtpUser);

                // ✅ Add recipients (comma-separated)
                foreach (var to in recipients.Split(','))
                {
                    string trimmed = to.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmed))
                        mail.To.Add(trimmed);
                }

                mail.Subject = "✅ Automation Execution Report";
                mail.Body = summaryHtml;
                mail.IsBodyHtml = true;

                // ✅ Attach the report
                mail.Attachments.Add(new Attachment(reportPath));

                using (SmtpClient smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpUser, smtpPass);
                    smtp.EnableSsl = true;

                    smtp.Send(mail);
                    Console.WriteLine("📧 Report emailed successfully to: " + recipients);
                }
            }
        }
    }
}