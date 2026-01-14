/* Ryan Morov
 * 2492176
 * Project - Flashcard Study App
 */
namespace StudySnap.Models
{
    /// <summary>
    /// Utility class for splitting large text content into manageable chunks.
    /// </summary>
    public static class TextChunker
    {
        /// <summary>
        /// Splits text into chunks of approximately the specified maximum character count.
        /// Attempts to split at paragraph boundaries when possible.
        /// </summary>
        /// <param name="text">The text to split</param>
        /// <param name="maxCharsPerChunk">Maximum characters per chunk</param>
        /// <returns>List of text chunks</returns>
        public static List<string> ChunkTextByChars(string text, int maxCharsPerChunk)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            if (text.Length <= maxCharsPerChunk)
                return new List<string> { text };

            var chunks = new List<string>();
            var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            var currentChunk = new System.Text.StringBuilder();

            foreach (var paragraph in paragraphs)
            {
                // If adding this paragraph would exceed the limit
                if (currentChunk.Length + paragraph.Length + 2 > maxCharsPerChunk)
                {
                    // Save current chunk if it has content
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                    }

                    // If the paragraph itself is too large, split it by sentences
                    if (paragraph.Length > maxCharsPerChunk)
                    {
                        var sentenceChunks = SplitLargeParagraph(paragraph, maxCharsPerChunk);
                        chunks.AddRange(sentenceChunks);
                    }
                    else
                    {
                        currentChunk.Append(paragraph);
                    }
                }
                else
                {
                    if (currentChunk.Length > 0)
                        currentChunk.Append("\n\n");
                    currentChunk.Append(paragraph);
                }
            }

            // Don't forget the last chunk
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }

            return chunks;
        }

        /// <summary>
        /// Splits a large paragraph into smaller chunks, attempting to break at sentence boundaries.
        /// </summary>
        private static List<string> SplitLargeParagraph(string paragraph, int maxChars)
        {
            var chunks = new List<string>();

            // Try to split by sentences (period, exclamation, question mark followed by space)
            var sentences = System.Text.RegularExpressions.Regex.Split(
                paragraph,
                @"(?<=[.!?])\s+");

            var currentChunk = new System.Text.StringBuilder();

            foreach (var sentence in sentences)
            {
                if (currentChunk.Length + sentence.Length + 1 > maxChars)
                {
                    if (currentChunk.Length > 0)
                    {
                        chunks.Add(currentChunk.ToString().Trim());
                        currentChunk.Clear();
                    }

                    // If a single sentence is too large, just add it (it will be truncated by the API)
                    if (sentence.Length > maxChars)
                    {
                        // Split by hard character limit as last resort
                        for (int i = 0; i < sentence.Length; i += maxChars)
                        {
                            int length = Math.Min(maxChars, sentence.Length - i);
                            chunks.Add(sentence.Substring(i, length).Trim());
                        }
                    }
                    else
                    {
                        currentChunk.Append(sentence);
                    }
                }
                else
                {
                    if (currentChunk.Length > 0)
                        currentChunk.Append(' ');
                    currentChunk.Append(sentence);
                }
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }

            return chunks;
        }
    }
}