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
    /// </summary>
    public partial class StudyMode : Window
    {
        private StudySession _session;
        private bool _isAnswerRevealed = false;

        // Constructor that takes a Deck object to initialize the study session
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

        // Updates the UI elements based on the current state of the study session
        private void UpdateUI()
        {
            // Step 1: Check if the session is complete
            if (!_session.HasMoreCards())
            {
                FinishSession();
                return;
            }

            // Step 2: Get the current card and update UI elements
            Flashcard currentCard = _session.GetNextCard();
            txtbFrontText.Text = currentCard.Front;
            txtbBackText.Text = currentCard.Back;

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
        
        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            _session.RecordAnswer(true);
            UpdateUI();
        }

        private void WrongButton_Click(object sender, RoutedEventArgs e)
        {
            _session.RecordAnswer(false);
            UpdateUI();
        }

        // Logic: reveal the answer when the user clicks on the card
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

        private void EndSession_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to end the session?", "End Session", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DialogResult = true;
                this.Close();
            }
        }

        private void StudyModeWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != null) // If already handled closing
                return;

            if (MessageBox.Show("Are you sure you want to end the session?", "End Session", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
        }
    }
}
