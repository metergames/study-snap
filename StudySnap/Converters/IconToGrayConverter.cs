using System.Globalization;
using System.Windows.Data;

namespace StudySnap.Converters
{
    /// <summary>
    /// Helper class to convert the deck icons from blue to gray (for nice UI).
    /// </summary>
    public class IconToGrayConverter : IValueConverter
    {
        /// <summary>
        /// If valid data, converts the value from blue icon to gray icon (by replacing the text in the path).
        /// </summary>
        /// <param name="value">Deck icon path</param>
        /// <param name="targetType">Unused</param>
        /// <param name="parameter">Unused</param>
        /// <param name="culture">Unused</param>
        /// <returns>Path for the gray variant of the icon, or if invalid input, returns said input</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
                return path.Replace("-blue", "-gray");

            return value;
        }

        /// <summary>
        /// Needed for the inheritance from IValueConverter, but will never be used.
        /// </summary>
        /// <exception cref="NotImplementedException">Not implemented ConvertBack</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
