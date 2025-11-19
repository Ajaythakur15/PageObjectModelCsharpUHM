using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PageObjectModelCsharp.Util
{
    /// <summary>
    /// Handles email delivery via SendGrid, with optional HTML content and file attachments.
    /// </summary>
    public static class SendGridMailer
    {
        private static readonly string SenderEmail = "ajaykumar.singh@techprocompsoft.com";
        private static readonly string SenderName = "Ajay Kumar Singh";
        private static readonly string SubjectLine = "✅ Automation Execution Summary";

        /// <summary>
        /// Sends a basic HTML email via SendGrid.
        /// </summary>
        public static async Task SendEmailAsync(string apiKey, string toEmail, string htmlContent)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(SenderEmail, SenderName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, SubjectLine, "See HTML version", htmlContent);
            await SendAndLogAsync(client, msg);
        }

        /// <summary>
        /// Sends an HTML email with a file attachment via SendGrid.
        /// </summary>
        public static async Task SendEmailWithAttachmentAsync(string apiKey, string toEmail, string htmlContent, string attachmentPath)
        {
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(SenderEmail, SenderName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, SubjectLine, "See HTML version", htmlContent);

            if (File.Exists(attachmentPath))
            {
                try
                {
                    byte[] fileBytes = await File.ReadAllBytesAsync(attachmentPath);
                    string base64 = Convert.ToBase64String(fileBytes);
                    string fileName = Path.GetFileName(attachmentPath);

                    msg.AddAttachment(fileName, base64);
                    Console.WriteLine($"📎 Attached file: {fileName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed to read attachment: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Attachment not found: {attachmentPath}");
            }

            await SendAndLogAsync(client, msg);
        }

        /// <summary>
        /// Sends the email and logs the response status and body.
        /// </summary>
        private static async Task SendAndLogAsync(SendGridClient client, SendGridMessage msg)
        {
            try
            {
                var response = await client.SendEmailAsync(msg);
                Console.WriteLine($"📬 SendGrid API Status: {response.StatusCode}");

                string responseBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine("📄 SendGrid Response Body:");
                Console.WriteLine(responseBody);

                if ((int)response.StatusCode >= 400)
                {
                    Console.WriteLine("❌ Email failed. Check sender identity, API key, or content.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Exception during SendGrid email send:");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}