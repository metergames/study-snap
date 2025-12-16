/* Ryan Morov & Felipe Mesa Paredes
 * 2492176 & 2466265
 * Project - Flashcard Study App
 */
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateDeckClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked create deck");
        }

        private void ExitButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked exit button");
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