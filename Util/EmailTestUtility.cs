using System;
using System.Net;
using System.Net.Mail;

namespace PageObjectModelCsharp.Util
{
    public static class EmailTestUtility
    {
        public static void SendTestEmail()
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("qa.ajay.thakur@gmail.com");
                mail.To.Add("tech.ajaythakur@gmail.com");
                mail.Subject = "SMTP Test Email";
                mail.Body = "This is a plain text test email from your automation framework.";
                mail.IsBodyHtml = false;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("qa.ajay.thakur@gmail.com", "quuoggfrxqsixibv");
                smtp.EnableSsl = true;

                smtp.Send(mail);
                Console.WriteLine("Email sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }
    }
}