using System;
using System.Collections.Generic;

namespace PageObjectModelCsharp.Util
{
    public static class ConfigValidator
    {
        private static readonly List<string> RequiredKeys = new()
        {
            "baseUrl",
            "browser",
            "headless",
            "windowBehavior",
            "valid_username",
            "valid_password",
            "smtp_host",
            "smtp_port",
            "smtp_user",
            "smtp_password",
            "email_to",
            "testEmail",
            "otp_retry_count",
            "otp_retry_delay_seconds"

        };

        public static void ValidateAll()
        {
            Console.WriteLine("🔍 Validating required config keys...");

            foreach (var key in RequiredKeys)
            {
                string value = PropertyReader.GetPropertyValue(key, string.Empty);
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception($"❌ Missing or empty config key: '{key}'");
                }
                Console.WriteLine($"✅ {key} = {value}");
            }

            Console.WriteLine("✅ All required config keys are valid");
        }
    }
}