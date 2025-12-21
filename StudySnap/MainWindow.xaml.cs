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
    /// Main dashboard for the application, handling deck management, navigatoin and statistics display.
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataRepository _repository;
        private List<Deck> _decks;

        /// <summary>
        /// Relative path to the file storing deck data.
        /// </summary>
        private const string DECK_FILE_PATH = @"Data\decks.json";

        /// <summary>
        /// Relative path to the file storing study session results.
        /// </summary>
        public const string RESULTS_FILE_PATH = @"Data\session_results.json";

        public MainWindow()
        {
            InitializeComponent();

            // Set up the data repository and load the initial list of decks.
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
                FilterBySearch();
                if (indexToSelect < lstbDecks.Items.Count)
                    lstbDecks.SelectedIndex = indexToSelect;
                else lstbDecks.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Handles the Click event for the "Create Deck" button.
        /// Opens a dialog to creates a new deck and saves it to the repository.
        /// </summary>
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
                newDeck.IconPath = newDeckWindow.SelectedIcon;

                _decks.Add(newDeck);

                _repository.SaveDecks(_decks, DECK_FILE_PATH);

                OpenDeckEditor(newDeck);

                RefreshDecks();
                lstbDecks.SelectedItem = newDeck;
            }
        }

        /// <summary>
        /// Opens the Deck Editor window for the specified deck.
        /// Handles logic for saving changes, deleting the deck, or updating the deck name in history.
        /// </summary>
        /// <param name="deck">The Deck object to edit</param>
        private void OpenDeckEditor(Deck deck)
        {
            string oldName = deck.Name;

            DeckEditor deckEditor = new DeckEditor(deck);
            deckEditor.Owner = this;

            this.Hide(); // Hide dashboard
            bool? saveData = deckEditor.ShowDialog();
            this.Show(); // Show dashboard

            if (deckEditor.WillDelete)
            {
                _decks.Remove(deck);
                DeleteDeckHistory(oldName);
                _repository.SaveDecks(_decks, DECK_FILE_PATH);
                RefreshDecks();
                return;
            }
            
            if (saveData == true)
            {
                if (deck.Name != oldName)
                    UpdateResultsNewDeckName(oldName, deck.Name);

                _repository.SaveDecks(_decks, DECK_FILE_PATH);
            }
        }

        /// <summary>
        /// Removes all study session results associated with a deleted deck.
        /// </summary>
        /// <param name="name">The name of the deck being deleted</param>
        private void DeleteDeckHistory(string name)
        {
            try
            {
                List<StudySessionResult> studyResults = _repository.LoadSessionResults(RESULTS_FILE_PATH);
                int removedCount = studyResults.RemoveAll(r => r.DeckName == name);

                if (removedCount > 0)
                    _repository.SaveSessionResults(studyResults, RESULTS_FILE_PATH);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting removed deck's history: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the deck name in the historical study results when a deck is renamed.
        /// </summary>
        /// <param name="oldName">The original name of the deck</param>
        /// <param name="newName">The new name of the deck</param>
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

        /// <summary>
        /// Handles the Click event for the "Start Study" button.
        /// Launches the study mode window for the currently selected deck.
        /// </summary>
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

        /// <summary>
        /// Handles the Click event for the "Edit Deck" button.
        /// Opens the editor for the currently selected deck.
        /// </summary>
        private void EditDeckClick(object sender, RoutedEventArgs e)
        {
            if (lstbDecks.SelectedItem != null)
            {
                Deck selectedDeck = lstbDecks.SelectedItem as Deck;
                OpenDeckEditor(selectedDeck);

                RefreshDecks(lstbDecks.SelectedIndex);
            }
        }

        /// <summary>
        /// Handles the Click event for the "Exit" button.
        /// Shuts down the application.
        /// </summary>
        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        /// <summary>
        /// Allow window to be dragged around by holding anywhere.
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        /// <summary>
        /// Handles the TextChanged event for the search box.
        /// Triggers the filtering of the deck list.
        /// </summary>
        private void SearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            FilterBySearch();
        }

        /// <summary>
        /// Filters the displayed decks based on the text entered in the search box.
        /// Updates the visibility of the "No Decks" label based on the search results.
        /// </summary>
        private void FilterBySearch()
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

        /// <summary>
        /// Handles the SelectionChanged event for the deck list.
        /// Enables or disables the "Start Study" button based on whether the selected deck has cards.
        /// </summary>
        private void DeckSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Deck currentDeck = lstbDecks.SelectedItem as Deck;
            if (currentDeck != null && currentDeck.Cards.Count > 0)
                btnStartStudy.IsEnabled = true;
            else btnStartStudy.IsEnabled = false;
        }
    }
}
