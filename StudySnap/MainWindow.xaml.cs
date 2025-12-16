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

            if (_decks.Count == 0)
            {
                lstbDecks.ItemsSource = null;
                lblNoDecks.Visibility = Visibility.Visible;
                WelcomeSection.Visibility = Visibility.Visible;
            }
            else
            {
                lblNoDecks.Visibility = Visibility.Collapsed;
                WelcomeSection.Visibility = Visibility.Collapsed;
                lstbDecks.ItemsSource = _decks;
            }
        }

        private void CreateDeckClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked create deck");
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
    }
}