/* Ryan Morov
 * 2492176
 * Project - Flashcard Study App
 */
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

namespace StudySnap.Models
{
    /// <summary>
    /// Service for extracting text content from various document formats (PDF, DOCX, TXT).
    /// </summary>
    public class DocumentTextExtractor
    {
        /// <summary>
        /// Maximum characters allowed for direct sending to API without summarization.
        /// </summary>
        public const int MaxCharsDirectSend = 25000;

        /// <summary>
        /// Maximum total characters accepted from a document. Larger documents will be truncated.
        /// </summary>
        public const int MaxCharsTotalAccepted = 250000;

        /// <summary>
        /// Characters per chunk when splitting large documents for summarization.
        /// </summary>
        public const int CharsPerChunk = 8000;

        /// <summary>
        /// Minimum text length to consider a PDF as having extractable text (not scanned).
        /// </summary>
        private const int MinPdfTextLength = 50;

        /// <summary>
        /// Supported file extensions for document extraction.
        /// </summary>
        public static readonly string[] SupportedExtensions = { ".pdf", ".docx", ".txt" };

        /// <summary>
        /// Gets the file filter string for OpenFileDialog.
        /// </summary>
        public static string FileFilter => "Documents (*.pdf;*.docx;*.txt)|*.pdf;*.docx;*.txt|PDF Files (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx|Text Files (*.txt)|*.txt";

        /// <summary>
        /// Extracts text content from a document file asynchronously.
        /// </summary>
        /// <param name="filePath">Path to the document file</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>Extracted and normalized text content</returns>
        /// <exception cref="ArgumentException">Thrown when file path is invalid or extension is unsupported</exception>
        /// <exception cref="FileNotFoundException">Thrown when the file does not exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when extraction fails (e.g., scanned PDF)</exception>
        public async Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("The specified file was not found.", filePath);

            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (!IsExtensionSupported(extension))
                throw new ArgumentException($"Unsupported file type: {extension}. Please upload PDF, DOCX, or TXT files.");

            cancellationToken.ThrowIfCancellationRequested();

            string rawText = extension switch
            {
                ".pdf" => await ExtractFromPdfAsync(filePath, cancellationToken),
                ".docx" => await ExtractFromDocxAsync(filePath, cancellationToken),
                ".txt" => await ExtractFromTxtAsync(filePath, cancellationToken),
                _ => throw new ArgumentException($"Unsupported file type: {extension}")
            };

            return NormalizeText(rawText);
        }

        /// <summary>
        /// Checks if the given file extension is supported.
        /// </summary>
        /// <param name="extension">File extension including the dot (e.g., ".pdf")</param>
        /// <returns>True if the extension is supported</returns>
        public static bool IsExtensionSupported(string extension)
        {
            return SupportedExtensions.Contains(extension.ToLowerInvariant());
        }

        /// <summary>
        /// Extracts text from a PDF file using PdfPig.
        /// </summary>
        private async Task<string> ExtractFromPdfAsync(string filePath, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var textBuilder = new StringBuilder();

                using (var document = PdfDocument.Open(filePath))
                {
                    foreach (var page in document.GetPages())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string pageText = page.Text;
                        if (!string.IsNullOrWhiteSpace(pageText))
                        {
                            textBuilder.AppendLine(pageText);
                            textBuilder.AppendLine(); // Add separator between pages
                        }
                    }
                }

                string result = textBuilder.ToString().Trim();

                // Check if the PDF appears to be scanned (no extractable text)
                if (result.Length < MinPdfTextLength)
                {
                    throw new InvalidOperationException(
                        "This PDF appears to be scanned or contains no selectable text. " +
                        "OCR (Optical Character Recognition) is not implemented. " +
                        "Please use a PDF with selectable text or convert your document first.");
                }

                return result;
            }, cancellationToken);
        }

        /// <summary>
        /// Extracts text from a DOCX file using OpenXml.
        /// </summary>
        private async Task<string> ExtractFromDocxAsync(string filePath, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var textBuilder = new StringBuilder();

                using (var document = WordprocessingDocument.Open(filePath, false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var body = document.MainDocumentPart?.Document?.Body;
                    if (body == null)
                        return string.Empty;

                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string paragraphText = paragraph.InnerText;
                        if (!string.IsNullOrWhiteSpace(paragraphText))
                        {
                            textBuilder.AppendLine(paragraphText);
                        }
                    }
                }

                return textBuilder.ToString().Trim();
            }, cancellationToken);
        }

        /// <summary>
        /// Extracts text from a plain text file.
        /// </summary>
        private async Task<string> ExtractFromTxtAsync(string filePath, CancellationToken cancellationToken)
        {
            return await File.ReadAllTextAsync(filePath, cancellationToken);
        }

        /// <summary>
        /// Normalizes extracted text by cleaning up whitespace and formatting.
        /// </summary>
        /// <param name="text">Raw text to normalize</param>
        /// <returns>Normalized text</returns>
        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Replace all line endings with \n
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // Collapse multiple spaces into single space (but preserve newlines)
            text = Regex.Replace(text, @"[^\S\n]+", " ");

            // Collapse more than 2 consecutive newlines into exactly 2
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            // Trim each line
            var lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Trim();
            }
            text = string.Join("\n", lines);

            // Final trim
            return text.Trim();
        }
    }
}