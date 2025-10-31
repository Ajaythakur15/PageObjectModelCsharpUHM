using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PageObjectModelCsharp.Util
{
    public static class PropertyReader
    {
        private static readonly Dictionary<string, string> _properties = new Dictionary<string, string>();
        private static bool _isInitialized = false;
        private static readonly object _lockObject = new object();

        public static string GetPropertyValue(string propertyName)
        {
            InitializeProperties();

            if (_properties.TryGetValue(propertyName, out string? value) && value != null)
            {
                return value;
            }

            throw new KeyNotFoundException($"Property '{propertyName}' not found in App.properties");
        }

        public static string GetPropertyValue(string propertyName, string defaultValue)
        {
            InitializeProperties();

            if (_properties.TryGetValue(propertyName, out string? value) && value != null)
            {
                return value;
            }

            return defaultValue;
        }

        private static void InitializeProperties()
        {
            if (_isInitialized) return;

            lock (_lockObject)
            {
                if (_isInitialized) return;

                try
                {
                    var propertiesFilePath = GetPropertiesFilePath();

                    if (!File.Exists(propertiesFilePath))
                    {
                        throw new FileNotFoundException($"App.properties file not found at: {propertiesFilePath}");
                    }

                    var lines = File.ReadAllLines(propertiesFilePath)
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Where(line => !line.TrimStart().StartsWith("#"))
                        .Where(line => line.Contains('='));

                    foreach (var line in lines)
                    {
                        var parts = line.Split(new[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            var key = parts[0].Trim();
                            var value = parts[1].Trim();
                            _properties[key] = value;
                        }
                    }

                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to initialize properties from App.properties", ex);
                }
            }
        }

        private static string GetPropertiesFilePath()
        {
            // Try multiple locations
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "config", "App.properties"),
                Path.Combine(AppContext.BaseDirectory, "config", "App.properties"),
                Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", "config", "App.properties"),
                Path.Combine(Directory.GetCurrentDirectory(), "App.properties"),
                Path.Combine(AppContext.BaseDirectory, "App.properties"),
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    Console.WriteLine($"Found properties file at: {path}");
                    return path;
                }
                Console.WriteLine($"Checking path: {path} - Exists: {File.Exists(path)}");
            }

            // Return the expected path in output directory
            return Path.Combine(Directory.GetCurrentDirectory(), "config", "App.properties");
        }

        public static bool ContainsProperty(string propertyName)
        {
            InitializeProperties();
            return _properties.ContainsKey(propertyName);
        }

        public static void ReloadProperties()
        {
            lock (_lockObject)
            {
                _properties.Clear();
                _isInitialized = false;
                InitializeProperties();
            }
        }
    }
}