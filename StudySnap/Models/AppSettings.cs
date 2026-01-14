/* Ryan Morov
 * 2492176
 * Project - Flashcard Study App
 */
using System.IO;
using System.Text.Json;

namespace StudySnap.Models
{
    /// <summary>
    /// Handles persistent application settings, including API keys.
    /// </summary>
    public class AppSettings
    {
        private const string SettingsFilePath = "Data/settings.json";

        /// <summary>
        /// OpenAI API key for flashcard generation.
        /// </summary>
        public string OpenAIApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Loads settings from the settings file.
        /// </summary>
        /// <returns>AppSettings object with stored values or defaults</returns>
        public static AppSettings Load()
        {
            if (!File.Exists(SettingsFilePath))
                return new AppSettings();

            try
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        /// <summary>
        /// Saves the current settings to the settings file.
        /// </summary>
        public void Save()
        {
            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(SettingsFilePath, json);
        }

        /// <summary>
        /// Checks if an OpenAI API key has been configured.
        /// </summary>
        public bool HasApiKey => !string.IsNullOrWhiteSpace(OpenAIApiKey);
    }
}