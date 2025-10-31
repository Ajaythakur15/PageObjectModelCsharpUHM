using NUnit.Framework;
using OpenQA.Selenium;
using System;

namespace PageObjectModelCsharp.Test
{
    [TestFixture]
    public class DebugTest : BaseTest
    {
        [Test]
        public void Find_Login_Button()
        {
            Console.WriteLine("=== FINDING LOGIN BUTTON ===");
            Console.WriteLine($"Current URL: {Driver.Url}");
            Console.WriteLine($"Page Title: {Driver.Title}");

            // Find ALL buttons on the page
            var buttons = Driver.FindElements(By.TagName("button"));
            Console.WriteLine($"Found {buttons.Count} buttons:");

            int buttonCount = 1;
            foreach (var button in buttons)
            {
                string text = button.Text;
                string className = button.GetAttribute("class");
                string type = button.GetAttribute("type");

                Console.WriteLine($"BUTTON #{buttonCount}:");
                Console.WriteLine($"  Text: '{text}'");
                Console.WriteLine($"  Class: '{className}'");
                Console.WriteLine($"  Type: '{type}'");
                Console.WriteLine("---");
                buttonCount++;
            }

            // Find ALL input elements that could be submit buttons
            var inputs = Driver.FindElements(By.TagName("input"));
            Console.WriteLine($"Found {inputs.Count} input elements:");

            foreach (var input in inputs)
            {
                string type = input.GetAttribute("type");
                if (type == "submit" || type == "button")
                {
                    string id = input.GetAttribute("id");
                    string name = input.GetAttribute("name");
                    string value = input.GetAttribute("value");

                    Console.WriteLine($"SUBMIT INPUT:");
                    Console.WriteLine($"  Type: '{type}'");
                    Console.WriteLine($"  ID: '{id}'");
                    Console.WriteLine($"  Name: '{name}'");
                    Console.WriteLine($"  Value: '{value}'");
                    Console.WriteLine("---");
                }
            }

            // Take screenshot (this will work after Step 1)
            TakeScreenshot("Debug_Page");

            Assert.Pass("Check console output for button details");
        }
    }
}