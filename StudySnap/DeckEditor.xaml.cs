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

        public DeckEditor(Deck deck)
        {
            InitializeComponent();
            _currentDeck = deck;

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
        }

        private void FrontTextChanged(object sender, TextChangedEventArgs e)
        {
            lblPreview.Text = txtFront.Text;
        }

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

        private void BackToDashboardClick(object sender, RoutedEventArgs e)
        {
            AttemptClose(false);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
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
                if (_unsavedChanges)
                {
                    if (MessageBox.Show("You have unsaved changes. Are you sure you want to discard them?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                        return;
                }

                this.DialogResult = false;
                this.Close();
            }
        }

        private void SaveDeckClick(object sender, RoutedEventArgs e)
        {
            AttemptClose(true);
        }
    }
}
