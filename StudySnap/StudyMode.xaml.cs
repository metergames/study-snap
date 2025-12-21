/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StudySnap.Models;

namespace StudySnap
{
    /// <summary>
    /// Interaction logic for StudyMode.xaml
    /// Manages the active study session, displaying flashcards and recording user responses.
    /// </summary>
    public partial class StudyMode : Window
    {
        private StudySession _session;
        private bool _isAnswerRevealed = false;

        /// <summary>
        /// Constructor that takes a Deck object to initialize the study session.
        /// </summary>
        /// <param name="deck">The deck to be studied</param>
        public StudyMode(Deck deck)
        {
            InitializeComponent();

            if (deck == null || deck.Cards.Count == 0)
            {
                MessageBox.Show("The selected deck is empty or invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                this.Close();
                return;
            }

            // Initialize the study session with the provided deck
            _session = new StudySession(deck);

            // Set initial UI state
            txtbDeckTitle.Text = $"Deck: {deck.Name}";

            if (!string.IsNullOrWhiteSpace(deck.IconPath))
                imgDeckIcon.Source = new BitmapImage(new Uri(deck.IconPath, UriKind.Relative));

            UpdateUI();
        }

        /// <summary>
        /// Updates the UI elements based on the current state of the study session.
        /// Checks if the session is complete, updates the progress indicators, and refreshes the current card view.
        /// </summary>
        private void UpdateUI()
        {
            // Step 1: Check if the session is complete
            if (!_session.HasMoreCards())
            {
                // Update progress display in Footer before finishing
                txtbCardCounter.Text = $"Card {_session.CurrentCardIndex} of {_session.TotalCards}";
                CorrectRun.Text = _session.CorrectCount.ToString();
                IncorrectRun.Text = (_session.CurrentCardIndex - _session.CorrectCount).ToString();
                ScoreRun.Text = $"{_session.CalculateCurrentScore():F0}%";
                // Finish the session
                FinishSession();
                return;
            }
            // Check for percentage
            int percentage = (int)((double)(_session.CurrentCardIndex + 1) / _session.TotalCards * 100);

            // Step 2: Get the current card and update UI elements
            Flashcard currentCard = _session.GetNextCard();
            txtbFrontText.Text = currentCard.Front;
            txtbBackText.Text = currentCard.Back;
            WinFormsCircularBar.Value = percentage;

            // Step 3: Update progress display in Footer (0 based so add 1)
            txtbCardCounter.Text = $"Card {_session.CurrentCardIndex + 1} of {_session.TotalCards}";

            CorrectRun.Text = _session.CorrectCount.ToString();
            IncorrectRun.Text = (_session.CurrentCardIndex - _session.CorrectCount).ToString();
            ScoreRun.Text = $"{_session.CalculateCurrentScore():F0}%";

            // Step 4: Update the visibility of answer elements
            BtnRight.IsEnabled = false;
            BtnWrong.IsEnabled = false;

            // Step 4: Reset the Card State (Hide answer)
            _isAnswerRevealed = false;
            txtbBackText.Visibility = Visibility.Hidden;
            ClickToRevealText.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Finalizes the study session.
        /// Creates a result object, saves it to the repository, displays a summary to the user, and closes the window.
        /// </summary>
        private void FinishSession()
        {
            StudySessionResult result = _session.CreateSessionResult();
            DataRepository repository = new DataRepository();
            string path = MainWindow.RESULTS_FILE_PATH;

            try
            {
                List<StudySessionResult> history = repository.LoadSessionResults(path);

                history.Add(result);

                repository.SaveSessionResults(history, path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save progress: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            string message = $"Session Complete!\n\n" +
                             $"Score: {result.Score:F0}%\n" +
                             $"Correct: {result.CorrectCount}\n" +
                             $"Total: {result.TotalCards}";
            MessageBox.Show(message, "Summary", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event for the "I got it right" button.
        /// Records a correct answer and advances to the next card.
        /// </summary>
        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            _session.RecordAnswer(true);
            UpdateUI();
        }

        /// <summary>
        /// Handles the Click event for the "I got it wrong" button.
        /// Records an incorrect answer and advances to the next card.
        /// </summary>
        private void WrongButton_Click(object sender, RoutedEventArgs e)
        {
            _session.RecordAnswer(false);
            UpdateUI();
        }

        /// <summary>
        /// Handles the MouseDown event on the flashcard area.
        /// Reveals the answer (back of the card) and enables the response buttons.
        /// </summary>
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isAnswerRevealed)
            {
                _isAnswerRevealed = true;
                txtbBackText.Visibility = Visibility.Visible;
                ClickToRevealText.Visibility = Visibility.Hidden;
                // Enable the answer buttons
                BtnRight.IsEnabled = true;
                BtnWrong.IsEnabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event for the "End Session" button.
        /// Prompts the user for confirmation before closing the session prematurely.
        /// </summary>
        private void EndSession_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to end the session?", "End Session", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DialogResult = true;
                this.Close();
            }
        }

        /// <summary>
        /// Handles the window closing event.
        /// Ensures the user confirms ending the session if the window is closed via system controls.
        /// </summary>
        private void StudyModeWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != null) // If already handled closing
                return;

            if (MessageBox.Show("Are you sure you want to end the session?", "End Session", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
        }

        /// <summary>
        /// Allow window to be dragged around by holding anywhere.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
