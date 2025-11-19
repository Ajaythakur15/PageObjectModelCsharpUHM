using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace PageObjectModelCsharp.Util
{
    public static class EmailSender
    {
        /// <summary>
        /// Sends an HTML summary email with optional report attachment.
        /// </summary>
        /// <param name="reportPath">Path to the zipped report file.</param>
        /// <param name="summaryHtml">HTML content for the email body.</param>
        public static void SendReport(string reportPath, string summaryHtml)
        {
            string smtpHost = PropertyReader.GetPropertyValue("smtp_host");
            int smtpPort = int.Parse(PropertyReader.GetPropertyValue("smtp_port"));
            string smtpUser = PropertyReader.GetPropertyValue("smtp_user");
            string smtpPass = PropertyReader.GetPropertyValue("smtp_password");
            string recipients = PropertyReader.GetPropertyValue("email_to");

            using MailMessage mail = new();
            mail.From = new MailAddress("ajaykumar.singh@techprocompsoft.com");
            mail.ReplyToList.Add("tech.ajaythakur@gmail.com");

            foreach (var to in recipients.Split(','))
            {
                string trimmed = to.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    mail.To.Add(trimmed);
            }

            mail.Subject = "📊 Automation Execution Summary";
            mail.Body = summaryHtml;
            mail.IsBodyHtml = true;

            if (File.Exists(reportPath))
            {
                mail.Attachments.Add(new Attachment(reportPath));
                Console.WriteLine("📎 Report attached: " + Path.GetFileName(reportPath));
            }
            else
            {
                Console.WriteLine("⚠️ Report file not found: " + reportPath);
            }

            using SmtpClient smtp = new(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            try
            {
                smtp.Send(mail);
                Console.WriteLine("✅ Email sent successfully to: " + recipients);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email sending failed: " + ex.Message);
            }
        }
    }
}