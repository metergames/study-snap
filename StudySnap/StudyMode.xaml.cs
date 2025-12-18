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
                this.Close();
                return;
            }

            // Initialize the study session with the provided deck
            _session = new StudySession(deck);

            // Set initial UI state
            txtbDeckTitle.Text = $"Deck: {deck.Name}";
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
            txtbBackText.Text = _isAnswerRevealed ? currentCard.Back : "???";

            // Step 3: Update progress display in Footer (0 based so add 1)
            txtbCardCounter.Text = $"Card {_session.CurrentCardIndex + 1} of {_session.TotalCards}";

            CorrectRun.Text = _session.CorrectCount.ToString();
            IncorrectRun.Text = (_session.CurrentCardIndex - _session.CorrectCount).ToString();
            ScoreRun.Text = $"{_session.CalculateScore():F0}%";

            // Step 4: Reset the Card State (Hide answer)
            _isAnswerRevealed = false;
            txtbBackText.Visibility = Visibility.Hidden;
            ClickToRevealText.Visibility = Visibility.Visible;
        }
        private void FinishSession()
        {
            StudySessionResult result = _session.CreateSessionResult();
            string message = $"Session Complete!\n\n" +
                             $"Score: {result.Score:F0}%\n" +
                             $"Correct: {result.CorrectCount}\n" +
                             $"Total: {result.TotalCards}";
            MessageBox.Show(message, "Summary", MessageBoxButton.OK, MessageBoxImage.Information);
            // TODO: use DataRepository to save the 'result' to your history file.
            this.Close();
        }
        private void ProcessAnswer(bool isCorrect)
        {
            /// TO DISCUSS:
            // Optional:  Do we force the user to reveal answer if incorrect? 

            if (!_isAnswerRevealed)
            {
                // If you want to force reveal first, uncomment this:
                // Card_MouseDown(null, null);
                // return;
            }

            _session.RecordAnswer(isCorrect); // Logic from
            UpdateUI();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessAnswer(true);
        }

        private void WrongButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessAnswer(false);
        }
        // Logic: reveal the answer when the user clicks on the card
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (!_isAnswerRevealed)
            {
                _isAnswerRevealed = true;
                txtbBackText.Visibility = Visibility.Visible;
                ClickToRevealText.Visibility = Visibility.Hidden; 
            }
        }

        private void EndSession_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to end the session?", "End Session", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}
