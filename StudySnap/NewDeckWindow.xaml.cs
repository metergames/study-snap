/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
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
    /// Interaction logic for NewDeckWindow.xaml
    /// Handles the creation of a new deck by capturing user input for the deck name and icon.
    /// </summary>
    public partial class NewDeckWindow : Window
    {
        public string DeckName { get; private set; }
        public string SelectedIcon { get; private set; }

        public NewDeckWindow()
        {
            InitializeComponent();
            LoadIcons();
            txtbDeckName.Focus();
        }

        /// <summary>
        /// Populates the icon selection dropdown with the available icons defined in the application.
        /// Selects the first icon by default.
        /// </summary>
        private void LoadIcons()
        {
            cmbIcons.ItemsSource = App.AvailableIcons;
            cmbIcons.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the Click event for the "Cancel" button.
        /// Closes the window and sets the DialogResult to false, indicating no deck was created.
        /// </summary>
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event for the "Create" button.
        /// Validates the input, assigns the deck properties, and closes the window with a true DialogResult.
        /// </summary>
        private void CreateClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbDeckName.Text))
            {
                MessageBox.Show("Please enter a deck name.");
                return;
            }

            DeckName = txtbDeckName.Text.Trim();
            SelectedIcon = cmbIcons.SelectedItem as string;

            this.DialogResult = true;
            this.Close();
        }
    }
}
