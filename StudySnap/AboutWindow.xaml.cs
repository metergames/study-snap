/* Ryan Morov
 * 2492176
 * Project - Flashcard Study App
 */
using System.Windows;

namespace StudySnap
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// Displays application information, credits, and copyright.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event for the Close button.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}