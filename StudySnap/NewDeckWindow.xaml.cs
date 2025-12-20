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

        private void LoadIcons()
        {
            cmbIcons.ItemsSource = App.AvailableIcons;
            cmbIcons.SelectedIndex = 0;
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

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
