/* Ryan Morov
 * 2492176
 * Project - Flashcard Study App
 */
using Microsoft.Win32;
using StudySnap.Models;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StudySnap
{
    /// <summary>
    /// Interaction logic for AIGenerateWindow.xaml
    /// Handles AI-powered flashcard generation using OpenAI with support for document uploads.
    /// </summary>
    public partial class AIGenerateWindow : Window
    {
        private readonly OpenAIService _openAIService;
        private readonly DocumentTextExtractor _textExtractor;
        private AppSettings _settings;
        private readonly string _deckName;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isLargeDocument = false;
        private string? _loadedFilePath;
        private bool _isEditingApiKey = false;

        /// <summary>
        /// Gets the list of generated flashcards after successful generation.
        /// </summary>
        public List<Flashcard> GeneratedFlashcards { get; private set; } = new List<Flashcard>();

        public AIGenerateWindow(string deckName)
        {
            InitializeComponent();
            _openAIService = new OpenAIService();
            _textExtractor = new DocumentTextExtractor();
            _settings = AppSettings.Load();
            _deckName = deckName;

            UpdateApiKeyUI();
            UpdateTitle();

            // Subscribe to text changed for character count
            txtTopicContent.TextChanged += (s, e) => UpdateCharacterCount();
        }

        /// <summary>
        /// Updates the window title to include the deck name.
        /// </summary>
        private void UpdateTitle()
        {
            if (!string.IsNullOrWhiteSpace(_deckName))
            {
                txtHeaderSubtitle.Text = $"Generate flashcards for \"{_deckName}\"";
            }
        }

        /// <summary>
        /// Updates the visibility of the API key section based on whether a key is saved.
        /// </summary>
        private void UpdateApiKeyUI()
        {
            if (_settings.HasApiKey && !_isEditingApiKey)
            {
                // Key exists and not editing - hide the section, show change button
                apiKeySection.Visibility = Visibility.Collapsed;
                btnChangeApiKey.Visibility = Visibility.Visible;
            }
            else
            {
                // No key or editing - show the section, hide change button
                apiKeySection.Visibility = Visibility.Visible;
                btnChangeApiKey.Visibility = Visibility.Collapsed;
                
                // Show cancel button only when editing existing key
                btnCancelApiKey.Visibility = _isEditingApiKey ? Visibility.Visible : Visibility.Collapsed;
                
                // Clear the text box (never show existing key)
                txtApiKey.Text = string.Empty;
            }
        }

        /// <summary>
        /// Updates the character count display.
        /// </summary>
        private void UpdateCharacterCount()
        {
            int charCount = txtTopicContent.Text.Length;
            txtCharCount.Text = $"{charCount:N0} characters";

            // Update large document warning
            _isLargeDocument = charCount > DocumentTextExtractor.MaxCharsDirectSend;
            largeDocWarning.Visibility = _isLargeDocument ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Handles the Click event for the Save API Key button.
        /// Validates and persists the API key.
        /// </summary>
        private void SaveApiKeyClick(object sender, RoutedEventArgs e)
        {
            string apiKey = txtApiKey.Text.Trim();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                MessageBox.Show("Please enter a valid API key.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!apiKey.StartsWith("sk-"))
            {
                MessageBox.Show("Invalid API key format. OpenAI keys typically start with 'sk-'.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _settings.OpenAIApiKey = apiKey;
            _settings.Save();

            _isEditingApiKey = false;
            UpdateApiKeyUI();
            MessageBox.Show("API key saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Handles the Click event for the Change API Key button.
        /// Shows the API key section for editing.
        /// </summary>
        private void ChangeApiKeyClick(object sender, RoutedEventArgs e)
        {
            _isEditingApiKey = true;
            UpdateApiKeyUI();
            txtApiKey.Focus();
        }

        /// <summary>
        /// Handles the Click event for the Cancel API Key button.
        /// Cancels editing and hides the API key section.
        /// </summary>
        private void CancelApiKeyClick(object sender, RoutedEventArgs e)
        {
            _isEditingApiKey = false;
            UpdateApiKeyUI();
        }

        #region File Upload - Drag and Drop

        /// <summary>
        /// Handles the DragEnter event for the drop zone.
        /// </summary>
        private void DropZone_DragEnter(object sender, DragEventArgs e)
        {
            if (IsValidFileDrop(e))
            {
                dropZone.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#135BEC"));
                dropZone.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EEF2FF"));
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the DragLeave event for the drop zone.
        /// </summary>
        private void DropZone_DragLeave(object sender, DragEventArgs e)
        {
            ResetDropZoneAppearance();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the DragOver event for the drop zone.
        /// </summary>
        private void DropZone_DragOver(object sender, DragEventArgs e)
        {
            if (IsValidFileDrop(e))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Drop event for the drop zone.
        /// </summary>
        private async void DropZone_Drop(object sender, DragEventArgs e)
        {
            ResetDropZoneAppearance();

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return;

            // Take only the first file if multiple are dropped
            string filePath = files[0];

            if (files.Length > 1)
            {
                MessageBox.Show("Multiple files detected. Only the first file will be processed.",
                    "Multiple Files", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            await ProcessFileAsync(filePath);
        }

        /// <summary>
        /// Checks if the dragged data contains a valid file.
        /// </summary>
        private bool IsValidFileDrop(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return false;

            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0)
                return false;

            string extension = Path.GetExtension(files[0]).ToLowerInvariant();
            return DocumentTextExtractor.IsExtensionSupported(extension);
        }

        /// <summary>
        /// Resets the drop zone to its default appearance.
        /// </summary>
        private void ResetDropZoneAppearance()
        {
            dropZone.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1D5DB"));
            dropZone.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB"));
        }

        #endregion

        #region File Upload - Browse

        /// <summary>
        /// Handles the Click event for the Browse button.
        /// Opens a file dialog to select a document.
        /// </summary>
        private async void BrowseFileClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select a Document",
                Filter = DocumentTextExtractor.FileFilter,
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await ProcessFileAsync(openFileDialog.FileName);
            }
        }

        #endregion

        #region File Processing

        /// <summary>
        /// Processes the selected file and extracts text content.
        /// </summary>
        private async Task ProcessFileAsync(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            if (!DocumentTextExtractor.IsExtensionSupported(extension))
            {
                MessageBox.Show("Unsupported file type. Please upload PDF, DOCX, or TXT files.",
                    "Unsupported File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cancel any existing extraction
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            SetExtractionState(true, "Extracting text...");

            try
            {
                string extractedText = await _textExtractor.ExtractTextAsync(filePath, cancellationToken);

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    MessageBox.Show("No text could be extracted from this file. The file may be empty or contain only images.",
                        "Extraction Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SetExtractionState(false);
                    return;
                }

                // Check if text exceeds maximum limit
                if (extractedText.Length > DocumentTextExtractor.MaxCharsTotalAccepted)
                {
                    var result = MessageBox.Show(
                        $"This document is very large ({extractedText.Length:N0} characters). " +
                        $"It will be truncated to {DocumentTextExtractor.MaxCharsTotalAccepted:N0} characters.\n\n" +
                        "Do you want to continue?",
                        "Large Document", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                    {
                        SetExtractionState(false);
                        return;
                    }

                    extractedText = extractedText[..DocumentTextExtractor.MaxCharsTotalAccepted];
                }

                // Update UI with extracted text
                txtTopicContent.Text = extractedText;
                _loadedFilePath = filePath;

                // Update drop zone to show loaded file
                string fileName = Path.GetFileName(filePath);
                dropZoneDefaultContent.Visibility = Visibility.Collapsed;
                dropZoneLoadedContent.Visibility = Visibility.Visible;
                txtLoadedFileName.Text = fileName;

                UpdateCharacterCount();

                MessageBox.Show($"Successfully extracted {extractedText.Length:N0} characters from \"{fileName}\".",
                    "Extraction Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (OperationCanceledException)
            {
                // User cancelled - do nothing
            }
            catch (InvalidOperationException ex)
            {
                // Scanned PDF or similar issue
                MessageBox.Show(ex.Message, "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting text:\n\n{ex.Message}",
                    "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetExtractionState(false);
            }
        }

        /// <summary>
        /// Handles the Click event for the Cancel Extraction button.
        /// </summary>
        private void CancelExtractionClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Updates the UI to show/hide extraction state.
        /// </summary>
        private void SetExtractionState(bool isExtracting, string? statusMessage = null)
        {
            extractionStatusPanel.Visibility = isExtracting ? Visibility.Visible : Visibility.Collapsed;

            if (statusMessage != null)
            {
                txtExtractionStatus.Text = statusMessage;
            }

            btnBrowseFile.IsEnabled = !isExtracting;
            dropZone.AllowDrop = !isExtracting;
            btnGenerate.IsEnabled = !isExtracting;
        }

        #endregion

        #region Large Document Summarization

        /// <summary>
        /// Summarizes a large document using a chunking approach.
        /// Splits the document into chunks, summarizes each chunk, then combines the summaries.
        /// </summary>
        private async Task<string> SummarizeLargeDocumentAsync(string fullText, bool useAdvancedModel, CancellationToken cancellationToken)
        {
            // Split the text into manageable chunks
            var chunks = TextChunker.ChunkTextByChars(fullText, DocumentTextExtractor.CharsPerChunk);

            if (chunks.Count == 0)
                return string.Empty;

            // If only one chunk and it's small enough, return as-is
            if (chunks.Count == 1 && chunks[0].Length <= DocumentTextExtractor.MaxCharsDirectSend)
                return chunks[0];

            var summaries = new List<string>();
            int currentChunk = 0;

            foreach (var chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                currentChunk++;
                txtLoadingMessage.Text = $"Summarizing chunk {currentChunk} of {chunks.Count}...";

                try
                {
                    string summary = await _openAIService.SummarizeToStudyNotesAsync(
                        _settings.OpenAIApiKey,
                        chunk,
                        useAdvancedModel,
                        cancellationToken);

                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        summaries.Add(summary);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error summarizing chunk {currentChunk}: {ex.Message}");
                }
            }

            if (summaries.Count == 0)
                return string.Empty;

            // Combine all summaries
            var combinedSummary = new StringBuilder();
            for (int i = 0; i < summaries.Count; i++)
            {
                if (i > 0)
                    combinedSummary.AppendLine("\n---\n");
                combinedSummary.Append(summaries[i]);
            }

            string result = combinedSummary.ToString();

            // If combined summary is still too large, summarize again
            if (result.Length > DocumentTextExtractor.MaxCharsDirectSend)
            {
                txtLoadingMessage.Text = "Creating final summary...";

                try
                {
                    result = await _openAIService.SummarizeToStudyNotesAsync(
                        _settings.OpenAIApiKey,
                        result,
                        useAdvancedModel,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error creating final summary: {ex.Message}");
                    result = result[..DocumentTextExtractor.MaxCharsDirectSend];
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Handles the Click event for the Cancel button.
        /// </summary>
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handles the Click event for the Generate button.
        /// </summary>
        private async void GenerateClick(object sender, RoutedEventArgs e)
        {
            if (!_settings.HasApiKey)
            {
                MessageBox.Show("Please save your OpenAI API key first.", "API Key Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string topicContent = txtTopicContent.Text.Trim();
            if (string.IsNullOrWhiteSpace(topicContent))
            {
                MessageBox.Show("Please enter some topic content or upload a document to generate flashcards from.",
                    "Content Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int requestedCount = int.Parse(((System.Windows.Controls.ComboBoxItem)cmbCardCount.SelectedItem).Content.ToString()!);
            bool useAdvancedModel = rbAdvanced.IsChecked == true;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            SetLoadingState(true);

            try
            {
                string textForGeneration;

                if (topicContent.Length > DocumentTextExtractor.MaxCharsDirectSend)
                {
                    txtLoadingMessage.Text = "Summarizing large document...";

                    textForGeneration = await SummarizeLargeDocumentAsync(
                        topicContent, useAdvancedModel, cancellationToken);

                    if (string.IsNullOrWhiteSpace(textForGeneration))
                    {
                        MessageBox.Show("Failed to summarize the document. Please try with smaller content.",
                            "Summarization Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        SetLoadingState(false);
                        return;
                    }

                    txtLoadingMessage.Text = "Generating flashcards from summary...";
                }
                else
                {
                    textForGeneration = topicContent;
                    txtLoadingMessage.Text = "Generating flashcards...";
                }

                string contextualTopic = string.IsNullOrWhiteSpace(_deckName)
                    ? textForGeneration
                    : $"Topic: {_deckName}\n\n{textForGeneration}";

                GeneratedFlashcards = await _openAIService.GenerateFlashcardsAsync(
                    _settings.OpenAIApiKey,
                    contextualTopic,
                    useAdvancedModel,
                    requestedCount
                );

                if (GeneratedFlashcards.Count == 0)
                {
                    MessageBox.Show("No flashcards were generated. Please try with different content.",
                        "No Results", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetLoadingState(false);
                    return;
                }

                int actualCount = GeneratedFlashcards.Count;
                string countMessage = actualCount == requestedCount
                    ? $"Successfully generated {actualCount} flashcards!"
                    : $"Generated {actualCount} flashcards (requested {requestedCount}).\n\nNote: AI may generate a slightly different number based on content complexity.";

                MessageBox.Show(countMessage, "Generation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (OperationCanceledException)
            {
                // User cancelled
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating flashcards:\n\n{ex.Message}",
                    "Generation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                SetLoadingState(false);
            }
        }

        /// <summary>
        /// Updates the UI to show/hide loading state.
        /// </summary>
        private void SetLoadingState(bool isLoading)
        {
            loadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            btnGenerate.IsEnabled = !isLoading;
            txtTopicContent.IsEnabled = !isLoading;
            rbBasic.IsEnabled = !isLoading;
            rbAdvanced.IsEnabled = !isLoading;
            cmbCardCount.IsEnabled = !isLoading;
            btnBrowseFile.IsEnabled = !isLoading;
            dropZone.AllowDrop = !isLoading;
            btnChangeApiKey.IsEnabled = !isLoading;

            btnCancel.Content = isLoading ? "Stop" : "Cancel";
        }

        /// <summary>
        /// Allow window to be dragged around by holding anywhere.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}