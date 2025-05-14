using System;
using System.Globalization;
using System.Windows.Data;

namespace TimeTrackerApp.Converters
{
    public class ProgressToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress * 360; // Convert progress (0-1) to degrees (0-360)
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}