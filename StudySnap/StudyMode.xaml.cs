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
using System.Windows.Media.Animation;
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
        private bool _isAnimating = false;
        private Storyboard _currentStoryboard;

        /// <summary>
        /// Constructor that takes a Deck object to initialize the study session.
        /// </summary>
        /// <param name="deck">The deck to be studied</param>
        public StudyMode(Deck deck)
        {
            InitializeComponent();

            // Update clip geometry when window size changes for proper rounded corners
            this.SizeChanged += StudyMode_SizeChanged;

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
        /// Updates the clip geometry when the window is resized to maintain rounded corners.
        /// </summary>
        private void StudyMode_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var border = this.Content as Border;
            if (border != null)
            {
                border.Clip = new RectangleGeometry
                {
                    RadiusX = 15,
                    RadiusY = 15,
                    Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height)
                };
            }
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

            // Step 2: Reset the card to front side before showing new card content
            ResetCardToFront();

            // Calculate percentage
            int percentage = (int)((double)(_session.CurrentCardIndex + 1) / _session.TotalCards * 100);

            // Step 3: Get the current card and update UI elements
            Flashcard currentCard = _session.GetNextCard();
            txtbFrontText.Text = currentCard.Front;
            txtbBackText.Text = currentCard.Back;
            WinFormsCircularBar.Value = percentage;

            // Step 4: Update progress display in Footer (0 based so add 1)
            txtbCardCounter.Text = $"Card {_session.CurrentCardIndex + 1} of {_session.TotalCards}";

            CorrectRun.Text = _session.CorrectCount.ToString();
            IncorrectRun.Text = (_session.CurrentCardIndex - _session.CorrectCount).ToString();
            ScoreRun.Text = $"{_session.CalculateCurrentScore():F0}%";

            // Step 5: Disable answer buttons until card is revealed
            BtnRight.IsEnabled = false;
            BtnWrong.IsEnabled = false;
        }

        /// <summary>
        /// Plays the flip animation to reveal the back of the card.
        /// </summary>
        private void FlipToBack()
        {
            if (_isAnimating) return;

            _isAnimating = true;
            var storyboard = (Storyboard)FindResource("FlipToBack");
            
            // Clone to avoid issues with reusing the storyboard
            _currentStoryboard = storyboard.Clone();
            
            _currentStoryboard.Completed += (s, e) =>
            {
                _isAnimating = false;
                _isAnswerRevealed = true;
                BtnRight.IsEnabled = true;
                BtnWrong.IsEnabled = true;
            };
            _currentStoryboard.Begin(this);
        }

        /// <summary>
        /// Plays the flip animation to show the front of the card again.
        /// </summary>
        private void FlipToFront()
        {
            if (_isAnimating) return;

            _isAnimating = true;
            var storyboard = (Storyboard)FindResource("FlipToFront");
            
            // Clone to avoid issues with reusing the storyboard
            _currentStoryboard = storyboard.Clone();
            
            _currentStoryboard.Completed += (s, e) =>
            {
                _isAnimating = false;
                _isAnswerRevealed = false;
            };
            _currentStoryboard.Begin(this);
        }

        /// <summary>
        /// Resets the card to show the front side without animation.
        /// </summary>
        private void ResetCardToFront()
        {
            // Remove the currently running storyboard to release its hold on animated properties
            if (_currentStoryboard != null)
            {
                _currentStoryboard.Remove(this);
                _currentStoryboard = null;
            }
            
            // Reset state flags
            _isAnswerRevealed = false;
            _isAnimating = false;
            
            // Force immediate visibility states (not animated)
            CardFront.Visibility = Visibility.Visible;
            CardBack.Visibility = Visibility.Collapsed;
            
            // Reset transforms to initial state
            CardFrontTransform.ScaleX = 1;
            CardBackTransform.ScaleX = 0;
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
            AdvanceToNextCard();
        }

        /// <summary>
        /// Handles the Click event for the "I got it wrong" button.
        /// Records an incorrect answer and advances to the next card.
        /// </summary>
        private void WrongButton_Click(object sender, RoutedEventArgs e)
        {
            _session.RecordAnswer(false);
            AdvanceToNextCard();
        }

        /// <summary>
        /// Advances to the next card, flipping back to front first if the answer is revealed.
        /// </summary>
        private void AdvanceToNextCard()
        {
            if (_isAnswerRevealed && !_isAnimating)
            {
                // Flip back to front first, then update UI after animation completes
                _isAnimating = true;
                var storyboard = (Storyboard)FindResource("FlipToFront");
                _currentStoryboard = storyboard.Clone();

                _currentStoryboard.Completed += (s, e) =>
                {
                    _isAnimating = false;
                    _isAnswerRevealed = false;
                    _currentStoryboard = null;
                    UpdateUI();
                };
                _currentStoryboard.Begin(this);
            }
            else
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Handles the MouseDown event on the flashcard area.
        /// Toggles the flip animation to reveal/hide the answer.
        /// </summary>
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isAnimating) return;

            if (!_isAnswerRevealed)
            {
                FlipToBack();
            }
            else
            {
                FlipToFront();
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
