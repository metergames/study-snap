/* Ryan Morov
 * 2492176
 * Project - Flashcard Study App
 */
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace StudySnap.Models
{
    /// <summary>
    /// Service for generating flashcards using OpenAI's API.
    /// </summary>
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        public OpenAIService()
        {
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Generates flashcards from the provided topic text using OpenAI.
        /// </summary>
        /// <param name="apiKey">OpenAI API key</param>
        /// <param name="topicText">Text describing the topic for flashcard generation</param>
        /// <param name="useAdvancedModel">If true, uses GPT-4o; otherwise uses GPT-4o-mini</param>
        /// <param name="requestedCount">The desired number of flashcards to generate</param>
        /// <returns>List of generated Flashcard objects</returns>
        public async Task<List<Flashcard>> GenerateFlashcardsAsync(string apiKey, string topicText, bool useAdvancedModel, int requestedCount)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required.");

            if (string.IsNullOrWhiteSpace(topicText))
                throw new ArgumentException("Topic text is required.");

            if (requestedCount < 1 || requestedCount > 50)
                throw new ArgumentException("Requested count must be between 1 and 50.");

            string model = useAdvancedModel ? "gpt-4o" : "gpt-4o-mini";

            string systemPrompt = $"You are a flashcard generation assistant. Your task is to create educational flashcards from the provided text.\n" +
                                  $"Generate EXACTLY {requestedCount} flashcards, no more and no less.\n" +
                                  $"Each flashcard should have a clear question (front) and a concise answer (back).\n" +
                                  $"Focus on key concepts, definitions, and important facts from the material.\n\n" +
                                  $"IMPORTANT: Respond ONLY with a valid JSON array containing exactly {requestedCount} flashcards in this format:\n" +
                                  @"[
    {""front"": ""Question 1"", ""back"": ""Answer 1""},
    {""front"": ""Question 2"", ""back"": ""Answer 2""}
]" +
                                  $"\n\nDo not include any other text, explanations, or markdown formatting.";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    },
                    new
                    {
                        role = "user",
                        content = $"Create exactly {requestedCount} flashcards from the following content:\n\n{topicText}"
                    }
                },
                temperature = 0.7,
                max_tokens = 2500
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.PostAsync(ApiUrl, httpContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"OpenAI API error: {response.StatusCode} - {responseContent}");
            }

            return ParseFlashcardsFromResponse(responseContent);
        }

        /// <summary>
        /// Summarizes a chunk of text into concise study notes.
        /// Used for processing large documents before flashcard generation.
        /// </summary>
        /// <param name="apiKey">OpenAI API key</param>
        /// <param name="chunkText">Text chunk to summarize</param>
        /// <param name="useAdvancedModel">If true, uses GPT-4o; otherwise uses GPT-4o-mini</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Summarized study notes</returns>
        public async Task<string> SummarizeToStudyNotesAsync(string apiKey, string chunkText, bool useAdvancedModel, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key is required.");

            if (string.IsNullOrWhiteSpace(chunkText))
                return string.Empty;

            string model = useAdvancedModel ? "gpt-4o" : "gpt-4o-mini";

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = """
                            You are a study notes assistant. Your task is to transform raw notes or text into clean, organized study notes.
                            
                            Guidelines:
                            - Use clear headings and bullet points
                            - Preserve key definitions, formulas, dates, and important facts
                            - Remove fluff, redundancy, and filler content
                            - Keep the summary concise but comprehensive
                            - Maintain the original meaning and context
                            
                            Respond with the summarized study notes only. No explanations or meta-commentary.
                            """
                    },
                    new
                    {
                        role = "user",
                        content = $"Summarize the following text into organized study notes:\n\n{chunkText}"
                    }
                },
                temperature = 0.5,
                max_tokens = 2000
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = httpContent;

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"OpenAI API error: {response.StatusCode} - {responseContent}");
            }

            return ParseTextFromResponse(responseContent);
        }

        /// <summary>
        /// Parses the OpenAI response and extracts flashcards.
        /// </summary>
        private List<Flashcard> ParseFlashcardsFromResponse(string responseJson)
        {
            var flashcards = new List<Flashcard>();

            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var content = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
                return flashcards;

            // Clean up potential markdown code blocks
            content = content.Trim();
            if (content.StartsWith("```json"))
                content = content[7..];
            if (content.StartsWith("```"))
                content = content[3..];
            if (content.EndsWith("```"))
                content = content[..^3];
            content = content.Trim();

            using var cardsDoc = JsonDocument.Parse(content);
            foreach (var cardElement in cardsDoc.RootElement.EnumerateArray())
            {
                var front = cardElement.GetProperty("front").GetString();
                var back = cardElement.GetProperty("back").GetString();

                if (!string.IsNullOrWhiteSpace(front) && !string.IsNullOrWhiteSpace(back))
                {
                    flashcards.Add(new Flashcard(front, back));
                }
            }

            return flashcards;
        }

        /// <summary>
        /// Parses the OpenAI response and extracts the text content.
        /// </summary>
        private string ParseTextFromResponse(string responseJson)
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            var content = root
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content?.Trim() ?? string.Empty;
        }
    }
}