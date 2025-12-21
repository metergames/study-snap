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
    /// </summary>
    public partial class DeckEditor : Window
    {
        private Deck _currentDeck;
        private Flashcard _selectedCard;
        private bool _unsavedChanges = false;

        public bool WillDelete { get; private set; } = false;
        public List<string> IconsList { get; set; }

        public DeckEditor(Deck deck)
        {
            InitializeComponent();
            _currentDeck = deck;

            IconsList = App.AvailableIcons;

            this.DataContext = deck; // Make deck data viewable directly from XAML

            LoadCards();
        }

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

        private void AddNewCardClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            txtFront.Focus();
        }

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

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

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

        private void FrontTextChanged(object sender, TextChangedEventArgs e)
        {
            lblPreview.Text = txtFront.Text;
        }

        private void SaveCard(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFront.Text.Trim()) || string.IsNullOrWhiteSpace(txtBack.Text.Trim()))
            {
                System.Windows.MessageBox.Show("Both front and back are required", "Validation Error");
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

        private void BackToDashboardClick(object sender, RoutedEventArgs e)
        {
            AttemptClose(false);
        }

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

        private void SaveDeckClick(object sender, RoutedEventArgs e)
        {
            AttemptClose(true);
        }

        private void DeckEditorWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DialogResult != null) // If already handled closing
                return;

            if (!ConfirmDiscardChanges())
                e.Cancel = true;
        }

        private bool ConfirmDiscardChanges()
        {
            if (!_unsavedChanges)
                return true;

            return System.Windows.MessageBox.Show("You have unsaved changes. Are you sure you want to discard them?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        private void DeleteCard(object sender, RoutedEventArgs e)
        {
            if (_selectedCard == null)
                return;

            if (System.Windows.MessageBox.Show("Are you sure you want to delete this card?", "Delete Card", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _currentDeck.RemoveCard(_selectedCard);

                _unsavedChanges = true;

                LoadCards();
                ClearForm();
            }
        }

        private void DeleteDeckClick(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show($"Are you sure you want to delete the {_currentDeck.Name} deck?\n\nThis will also permanently delete all study records and statistics for this deck.", "Delete Deck", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
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
