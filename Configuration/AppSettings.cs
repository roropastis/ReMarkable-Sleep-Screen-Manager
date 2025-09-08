using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RemarkableSleepScreenManager.Configuration
{
    public class AppSettings
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ReMarkableSleepScreenManager",
            "settings.json");

        [JsonPropertyName("language")]
        public string Language { get; set; } = "en-US";

        [JsonPropertyName("lastConnection")]
        public ConnectionSettings LastConnection { get; set; } = new();

        [JsonPropertyName("windowSettings")]
        public WindowSettings WindowSettings { get; set; } = new();

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }

    public class ConnectionSettings
    {
        [JsonPropertyName("ipAddress")]
        public string IpAddress { get; set; } = "10.11.99.1";

        [JsonPropertyName("username")]
        public string Username { get; set; } = "root";

        [JsonPropertyName("rememberPassword")]
        public bool RememberPassword { get; set; } = false;
    }

    public class WindowSettings
    {
        [JsonPropertyName("width")]
        public double Width { get; set; } = 891;

        [JsonPropertyName("height")]
        public double Height { get; set; } = 853;

        [JsonPropertyName("left")]
        public double Left { get; set; } = double.NaN;

        [JsonPropertyName("top")]
        public double Top { get; set; } = double.NaN;

        [JsonPropertyName("windowState")]
        public string WindowState { get; set; } = "Normal";
    }
}
