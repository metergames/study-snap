/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
using StudySnap.Models;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StudySnap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataRepository _repository;
        private List<Deck> _decks;
        private const string DECK_FILE_PATH = "C:\\Users\\User\\Desktop\\decks.json";
        private const string RESULTS_FILE_PATH = "C:\\Users\\User\\Desktop\\session_results.json";

        public MainWindow()
        {
            InitializeComponent();

            _repository = new DataRepository();
            _decks = new List<Deck>();

            RefreshDecks();
        }

        /// <summary>
        /// Reloads the list of decks from the JSON file and updates the UI.
        /// If there are no decks, show the welcome section and a label stating there are no decks.
        /// </summary>
        private void RefreshDecks()
        {
            _decks = _repository.LoadDecks(DECK_FILE_PATH);
            List<StudySessionResult> results = _repository.LoadSessionResults(RESULTS_FILE_PATH);

            // Recalculate the stats for all decks
            foreach (Deck deck in _decks)
            {
                List<StudySessionResult> deckResults = results.Where(r => r.DeckName == deck.Name).ToList();

                if (deckResults.Count > 0)
                {
                    deck.LastStudiedDisplay = deckResults.Max(r => r.Date).ToString("MMM d, yyyy");
                    deck.BestScoreDisplay = $"{(int)deckResults.Max(r => r.Score)}%";
                    deck.AverageAccuracyDisplay = $"{(int)deckResults.Average(r => r.Score)}%";
                }
                else
                {
                    deck.LastStudiedDisplay = "-";
                    deck.BestScoreDisplay = "-";
                    deck.AverageAccuracyDisplay = "-";
                }
            }

            if (_decks.Count == 0) // Show no deck sections, hide deck details
            {
                lstbDecks.ItemsSource = null;
                lblNoDecks.Visibility = Visibility.Visible;
                WelcomeSection.Visibility = Visibility.Visible;
                DeckDetailSection.Visibility = Visibility.Collapsed;
            }
            else // Hide no decks sections, show deck details, populate list box with decks and select the first deck in the list
            {
                lblNoDecks.Visibility = Visibility.Collapsed;
                WelcomeSection.Visibility = Visibility.Collapsed;
                DeckDetailSection.Visibility = Visibility.Visible;
                lstbDecks.ItemsSource = _decks;
                lstbDecks.SelectedIndex = 0;
            }
        }

        private void CreateDeckClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked create deck");
        }

        private void StartStudyClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked start study");
        }

        private void EditDeckClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked edit deck");
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Allow window to be dragged around by holding anywhere.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_decks == null || _decks.Count == 0)
                return;

            string searchPrompt = txtbSearchBox.Text.ToLower().Trim();
            List<Deck> filteredDecks = _decks.Where(d => d.Name.ToLower().Contains(searchPrompt)).ToList();

            lstbDecks.ItemsSource = filteredDecks;

            if (filteredDecks.Count == 0)
            {
                lblNoDecks.Visibility = Visibility.Visible;
                DeckDetailSection.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblNoDecks.Visibility = Visibility.Collapsed;
                DeckDetailSection.Visibility = Visibility.Visible;

                lstbDecks.SelectedIndex = 0;
            }
        }
    }
}