using System;
using System.Globalization;
using System.Windows.Data;

namespace RemarkableSleepScreenManager
{
    /// <summary>
    /// Convertit une largeur en hauteur selon un ratio (par défaut 4:3 -> 1.3333).
    /// </summary>
    public sealed class WidthToHeightConverter : IValueConverter
    {
        /// <summary>Hauteur = Largeur * Factor. Pour un portrait 3:4, Factor = 4/3 ≈ 1.3333.</summary>
        public double Factor { get; set; } = 4.0 / 3.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double w && !double.IsNaN(w))
                return w * Factor;
            return 0d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
