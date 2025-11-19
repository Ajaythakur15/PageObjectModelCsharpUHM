using System;
using System.Text.RegularExpressions;

namespace PageObjectModelCsharp.Util
{
    /// <summary>
    /// Handles OTP retrieval and extraction logic for automation flows.
    /// </summary>
    public static class OTPHandler
    {
        /// <summary>
        /// Retrieves the OTP either from override config or Yopmail inbox.
        /// </summary>
        /// <returns>OTP string</returns>
        public static string GetOTP()
        {
            bool useOverride = bool.TryParse(PropertyReader.GetPropertyValue("use_otp_override", "false"), out var overrideFlag) && overrideFlag;

            if (useOverride)
            {
                string overrideOTP = PropertyReader.GetPropertyValue("otp_override", "123456");
                Console.WriteLine($"🔐 Using override OTP: {overrideOTP}");
                return overrideOTP;
            }

            string email = PropertyReader.GetPropertyValue("test_email", "asintest@yopmail.com");
            Console.WriteLine($"📩 Reading OTP from Yopmail: {email}");

            string? otp = YopmailReader.GetOTPFromYopmail(email);

            if (string.IsNullOrWhiteSpace(otp))
            {
                throw new Exception("❌ Failed to retrieve OTP from email. Please check your Yopmail inbox manually.");
            }

            Console.WriteLine($"✅ OTP retrieved: {otp}");
            return otp;
        }

        /// <summary>
        /// Extracts OTP from a given text using common patterns.
        /// </summary>
        /// <param name="text">Text content containing OTP</param>
        /// <returns>Extracted OTP or null</returns>
        public static string? ExtractOTPFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var patterns = new[]
            {
                @"\b\d{6}\b",                   // 6-digit OTP
                @"\b\d{4}\b",                   // 4-digit OTP
                @"code[\s:]*([0-9]{4,6})",      // "code: 123456"
                @"verification[\s:]*([0-9]{4,6})", // "verification: 123456"
                @"is[\s:]*([0-9]{4,6})"         // "your code is 123456"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string otp = match.Groups[1].Success ? match.Groups[1].Value : match.Value;
                    Console.WriteLine($"🔍 Extracted OTP: {otp} using pattern: {pattern}");
                    return otp;
                }
            }

            Console.WriteLine("⚠️ No OTP pattern matched in text");
            return null;
        }
    }
}