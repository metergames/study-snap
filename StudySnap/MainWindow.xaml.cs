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
        private const string DECK_FILE_PATH =   "C:\\Users\\felip\\Downloads\\decks.json";
        public const string RESULTS_FILE_PATH = "C:\\Users\\felip\\Downloads\\session_results.json";

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
        private void RefreshDecks(int indexToSelect = 0)
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
                if (indexToSelect < lstbDecks.Items.Count)
                    lstbDecks.SelectedIndex = indexToSelect;
                else lstbDecks.SelectedIndex = 0;
            }
        }

        private void CreateDeckClick(object sender, RoutedEventArgs e)
        {
            NewDeckWindow newDeckWindow = new NewDeckWindow();
            newDeckWindow.Owner = this;

            this.Opacity = 0.4;
            bool? setName = newDeckWindow.ShowDialog();
            this.Opacity = 1;

            if (setName == true)
            {
                Deck newDeck = new Deck(newDeckWindow.DeckName);
                _decks.Add(newDeck);

                _repository.SaveDecks(_decks, DECK_FILE_PATH);

                OpenDeckEditor(newDeck);

                RefreshDecks();
                lstbDecks.SelectedItem = newDeck;
            }
        }

        private void OpenDeckEditor(Deck deck)
        {
            string oldName = deck.Name;

            DeckEditor deckEditor = new DeckEditor(deck);
            deckEditor.Owner = this;

            this.Hide(); // Hide dashboard
            bool? saveData = deckEditor.ShowDialog();
            this.Show(); // Show dashboard

            if (saveData == true)
            {
                if (deck.Name != oldName)
                    UpdateResultsNewDeckName(oldName, deck.Name);

                _repository.SaveDecks(_decks, DECK_FILE_PATH);
            }
        }

        private void UpdateResultsNewDeckName(string oldName, string newName)
        {
            try
            {
                List<StudySessionResult> studyResults = _repository.LoadSessionResults(RESULTS_FILE_PATH);
                bool modifiedData = false;

                foreach (StudySessionResult result in studyResults)
                {
                    if (result.DeckName == oldName)
                    {
                        result.DeckName = newName;
                        modifiedData = true;
                    }
                }

                if (modifiedData)
                    _repository.SaveSessionResults(studyResults, RESULTS_FILE_PATH);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating history: {ex.Message}");
            }
        }

        private void StartStudyClick(object sender, RoutedEventArgs e)
        {
            if (lstbDecks.SelectedItem != null)
            {
                Deck selectedDeck = lstbDecks.SelectedItem as Deck;
                StudyMode studyWindow = new StudyMode(selectedDeck);
                studyWindow.Owner = this;

                this.Hide(); // Hide dashboard
                studyWindow.ShowDialog();
                this.Show(); // Show dashboard

                RefreshDecks(lstbDecks.SelectedIndex);
            }
        }

        private void EditDeckClick(object sender, RoutedEventArgs e)
        {
            if (lstbDecks.SelectedItem != null)
            {
                Deck selectedDeck = lstbDecks.SelectedItem as Deck;
                OpenDeckEditor(selectedDeck);

                RefreshDecks(lstbDecks.SelectedIndex);
            }
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

        private void DeckSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Deck currentDeck = lstbDecks.SelectedItem as Deck;
            if (currentDeck != null && currentDeck.Cards.Count > 0)
                btnStartStudy.IsEnabled = true;
            else btnStartStudy.IsEnabled = false;
        }
    }
}