/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
using System.IO;
using System.Text.Json;

namespace StudySnap.Models
{
    /// <summary>
    /// Class handling persistence of application data, using JSON to store decks and study session results.
    /// </summary>
    public class DataRepository
    {
        /// <summary>
        /// Loads a list of decks from a JSON file.
        /// </summary>
        /// <param name="filePath">Path to the deck data file</param>
        /// <returns>List of Deck objects. Returns an empty list if the file is missing or contains invalid data</returns>
        /// <exception cref="ArgumentException">If filePath is null or empty</exception>
        public List<Deck> LoadDecks(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.");

            if (!File.Exists(filePath)) // If file doesn't exist, load new, empty list of decks
                return new List<Deck>();

            try
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<Deck>>(jsonData) ?? new List<Deck>(); // If null is returned from file, load new, empty list of decks
            }
            catch (Exception)
            {
                List<Deck> emptyList = new List<Deck>();
                SaveDecks(emptyList, filePath);
                return emptyList;
            }
        }

        /// <summary>
        /// Saves a list of decks to a file in JSON format.
        /// </summary>
        /// <param name="decks">List of Deck objects to save</param>
        /// <param name="filePath">Destination file path</param>
        /// <exception cref="ArgumentException">If deck list is null, or invalid file path is specified</exception>
        public void SaveDecks(List<Deck> decks, string filePath)
        {
            if (decks == null)
                throw new ArgumentException("Cannot save a null deck list.");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.");

            if (!Directory.Exists("Data"))
                Directory.CreateDirectory("Data");

            string jsonData = JsonSerializer.Serialize(decks);
            File.WriteAllText(filePath, jsonData);
        }

        /// <summary>
        /// Loads the history of study session results from a specified JSON file.
        /// </summary>
        /// <param name="filePath">Path to the results data file</param>
        /// <returns>List of StudySessionResult objects</returns>
        /// <exception cref="ArgumentException">If filePath is null or empty</exception>
        public List<StudySessionResult> LoadSessionResults(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.");

            if (!File.Exists(filePath))
                return new List<StudySessionResult>();

            try
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<StudySessionResult>>(jsonData) ?? new List<StudySessionResult>();
            }
            catch (Exception)
            {
                List<StudySessionResult> emptyList = new List<StudySessionResult>();
                SaveSessionResults(emptyList, filePath);
                return emptyList;
            }
        }

        /// <summary>
        /// Saves the history of study sesion results to a JSON file.
        /// </summary>
        /// <param name="results">List of StudySessionResult objects to save</param>
        /// <param name="filePath">Destination file path</param>
        /// <exception cref="ArgumentException">If results or filePath are null</exception>
        public void SaveSessionResults(List<StudySessionResult> results, string filePath)
        {
            if (results == null)
                throw new ArgumentException("Cannot save null results.");

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.");

            string jsonData = JsonSerializer.Serialize(results);
            File.WriteAllText(filePath, jsonData);
        }
    }
}
