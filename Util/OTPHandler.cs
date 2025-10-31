using System;
using System.Text.RegularExpressions;

namespace PageObjectModelCsharp.Util
{
    public static class OTPHandler
    {
        public static string GetOTP()
        {
            // Check if we're using override OTP for testing
            var useOverride = bool.Parse(PropertyReader.GetPropertyValue("use_otp_override", "false"));
            if (useOverride)
            {
                var overrideOTP = PropertyReader.GetPropertyValue("otp_override", "123456");
                Console.WriteLine($"Using override OTP: {overrideOTP}");
                return overrideOTP;
            }

            // Read OTP from Yopmail
            var email = PropertyReader.GetPropertyValue("test_email", "asintest@yopmail.com");
            Console.WriteLine($"Reading OTP from Yopmail: {email}");

            string? otp = YopmailReader.GetOTPFromYopmail(email);

            if (string.IsNullOrEmpty(otp))
            {
                throw new Exception("Failed to retrieve OTP from email. Please check your Yopmail inbox manually.");
            }

            return otp;
        }

        public static string? ExtractOTPFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            // Common OTP patterns
            var patterns = new[]
            {
                @"\b\d{6}\b",           // 6-digit OTP
                @"\b\d{4}\b",           // 4-digit OTP
                @"code[\s:]*(\d+)",     // "code: 123456"
                @"verification[\s:]*(\d+)", // "verification: 123456"
                @"is[\s:]*(\d+)"        // "your code is 123456"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var otp = match.Groups[1].Success ? match.Groups[1].Value : match.Value;
                    Console.WriteLine($"Extracted OTP: {otp} using pattern: {pattern}");
                    return otp;
                }
            }

            Console.WriteLine("No OTP pattern found in text");
            return null;
        }
    }
}