using System;
using System.Collections.Generic;
//using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Controls = System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace ToDo.Client.ViewModels
{
    //TODO: Move to Converters folder
    public class DateStyleConverter : IValueConverter
    {
        public static List<DateTime> Dates = new List<DateTime>();

        private static Style Bolded;

        static DateStyleConverter()
        {
            Bolded = new Style(typeof(CalendarDayButton));
            Bolded.Setters.Add(new Setter(Controls.Calendar.FontWeightProperty, FontWeights.Bold));
            Bolded.Setters.Add(new Setter(Controls.Calendar.BackgroundProperty, new SolidColorBrush(Colors.LightGreen)));
        }

        private static FontWeight HighlightWeight = FontWeights.Bold;
        private static SolidColorBrush HighlightColor = new SolidColorBrush(Colors.LightGreen);

        private static FontWeight NormalWeight = FontWeights.Normal;
        private static SolidColorBrush NormalColor = new SolidColorBrush(Colors.Transparent);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime && parameter is string)
            {
                DateTime dt = (DateTime)value;
                bool contains = Dates.Contains(dt);

                switch ((string)parameter)
                {
                    case "Color":
                        return contains ? HighlightColor : NormalColor;
                    case "FontWeight":
                        return contains ? HighlightWeight : NormalWeight;
                }
            }

            return null;

            /*
            if (value is DateTime)
            {
                DateTime dt = (DateTime)value;

                if (Dates.Contains(dt))
                    return FontWeights.ExtraBlack;
            }

            return FontWeights.Normal;
            */
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
