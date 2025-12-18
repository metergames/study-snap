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

        public DeckEditor(Deck deck)
        {
            InitializeComponent();
            _currentDeck = deck;
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
            MessageBox.Show($"Adding card to {_currentDeck.Name}");
        }

        private void CardSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstbCards.SelectedItem is Flashcard card)
            {
                _selectedCard = card;
                txtFront.Text = card.Front;
                txtBack.Text = card.Back;
                lblPreview.Text = card.Front;
            }
        }
    }
}
