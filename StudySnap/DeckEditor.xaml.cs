/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
using StudySnap.Models;
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

namespace StudySnap
{
    /// <summary>
    /// Interaction logic for DeckEditor.xaml
    /// Manages the editing of a specific deck, including adding, removing and modifying flashcards.
    /// </summary>
    public partial class DeckEditor : Window
    {
        private Deck _currentDeck;
        private Flashcard _selectedCard;
        private bool _unsavedChanges = false;

        /// <summary>
        /// Gets a value indicating whether the user has requested to delete the deck.
        /// </summary>
        public bool WillDelete { get; private set; } = false;

        /// <summary>
        /// Gets or sets the list of available icons for the deck.
        /// </summary>
        public List<string> IconsList { get; set; }

        public DeckEditor(Deck deck)
        {
            InitializeComponent();
            _currentDeck = deck;

            IconsList = App.AvailableIcons;

            this.DataContext = deck; // Make deck data viewable directly from XAML

            LoadCards();
        }

        /// <summary>
        /// Refreshes the list of cards displayed in the ListBox.
        /// Toggles the visibility of the "No Cards" label based on the count.
        /// </summary>
        private void LoadCards()
        {
            if (_currentDeck != null)
            {
                lstbCards.ItemsSource = null;
                lstbCards.ItemsSource = _currentDeck.Cards;

                if (_currentDeck.Cards.Count == 0)
                    lblNoCards.Visibility = Visibility.Visible;
                else lblNoCards.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the Click event for the Add New Card button.
        /// Clears the input form to prepare for a new entry.
        /// </summary>
        private void AddNewCardClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            txtFront.Focus();
        }

        /// <summary>
        /// Handles the SelectionChanged event for the Cards ListBox.
        /// Populates the input fields with the selected card's data for editing.
        /// </summary>
        private void CardSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstbCards.SelectedIndex >= 0)
            {
                Flashcard card = lstbCards.SelectedItem as Flashcard;

                _selectedCard = card;
                txtFront.Text = card.Front;
                txtBack.Text = card.Back;
                lblPreview.Text = card.Front;

                btnSaveCard.Content = "Save Card"; // Save button action - Overwrite

                btnDeleteCard.IsEnabled = true;
                imgTrash.Source = new BitmapImage(new Uri("/Images/trash-fill-red.png", UriKind.Relative));
            }
        }

        /// <summary>
        /// Handles the Click event for the Clear button.
        /// Resets the form fields and selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        /// <summary>
        /// Resets the UI input fields, clears the current selection, and disables the delete button.
        /// </summary>
        private void ClearForm()
        {
            _selectedCard = null;
            lstbCards.SelectedIndex = -1;
            txtFront.Text = "";
            txtBack.Text = "";
            lblPreview.Text = "";

            btnSaveCard.Content = "Add Card"; // Save button action - Add

            btnDeleteCard.IsEnabled = false;
            imgTrash.Source = new BitmapImage(new Uri("/Images/trash-fill-gray.png", UriKind.Relative));
        }

        /// <summary>
        /// Handles the TextChanged event for the Front input TextBox.
        /// Updates the live preview text.
        /// </summary>
        private void FrontTextChanged(object sender, TextChangedEventArgs e)
        {
            lblPreview.Text = txtFront.Text;
        }

        /// <summary>
        /// Handles the logic to save a new card or update an existing one.
        /// Validates input before modification.
        /// </summary>
        private void SaveCard(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFront.Text.Trim()) || string.IsNullOrWhiteSpace(txtBack.Text.Trim()))
            {
                MessageBox.Show("Both front and back are required", "Validation Error");
                return;
            }

            if (_selectedCard == null) // Add card
            {
                Flashcard newCard = new Flashcard(txtFront.Text, txtBack.Text);
                _currentDeck.AddCard(newCard);
            }
            else // Edit existing card
            {
                _selectedCard.Front = txtFront.Text;
                _selectedCard.Back = txtBack.Text;
            }

            _unsavedChanges = true;

            LoadCards();
            ClearForm();
        }

        /// <summary>
        /// Handles the Click event for the "Back to Dashboard" button.
        /// Attempts to close the window without saving (unless prompted).
        /// </summary>
        private void BackToDashboardClick(object sender, RoutedEventArgs e)
        {
            AttemptClose(false);
        }

        /// <summary>
        /// Logic to handle window closing.
        /// Sets the DialogResult based on whether the data is being saved or discarded.
        /// </summary>
        /// <param name="isSaving">If set to true, the changes are accepted and the window closes with a true DialogResult</param>
        private void AttemptClose(bool isSaving)
        {
            if (isSaving)
            {
                this.DialogResult = true; // Communication with MainWindow
                this.Close();
            }
            else
            {
                if (!ConfirmDiscardChanges())
                    return;

                this.DialogResult = false;
                this.Close();
            }
        }

        /// <summary>
        /// Handles the Click event for the "Save Deck" button.
        /// Closes the window and indicates that changes should be persisted.
        /// </summary>
        private void SaveDeckClick(object sender, RoutedEventArgs e)
        {
            AttemptClose(true);
        }

        /// <summary>
        /// Handles the window's closing event.
        /// Prevents closing if there are unsaved changes and the user chooses not to discard them.
        /// </summary>
        private void DeckEditorWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != null) // If already handled closing
                return;

            if (!ConfirmDiscardChanges())
                e.Cancel = true;
        }

        /// <summary>
        /// Checks for unsaved changes and prompts the user for confirmation if necessary.
        /// </summary>
        /// <returns>True if there are no changes or the user confirms discarding them, false if otherwise</returns>
        private bool ConfirmDiscardChanges()
        {
            if (!_unsavedChanges)
                return true;

            return MessageBox.Show("You have unsaved changes. Are you sure you want to discard them?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Deletes the currently selected card from the deck after user confirmation.
        /// </summary>
        private void DeleteCard(object sender, RoutedEventArgs e)
        {
            if (_selectedCard == null)
                return;

            if (MessageBox.Show("Are you sure you want to delete this card?", "Delete Card", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _currentDeck.RemoveCard(_selectedCard);

                _unsavedChanges = true;

                LoadCards();
                ClearForm();
            }
        }

        /// <summary>
        /// Handles the deletion of the entire deck.
        /// Sets a flag indicating the deck should be removed and closes the editor.
        /// </summary>
        private void DeleteDeckClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete the {_currentDeck.Name} deck?\n\nThis will also permanently delete all study records and statistics for this deck.", "Delete Deck", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                WillDelete = true;
                DialogResult = false;
                this.Close();
            }
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
