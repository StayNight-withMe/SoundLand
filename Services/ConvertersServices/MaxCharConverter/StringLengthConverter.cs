using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace test
{
    public class StringLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            int parametr = System.Convert.ToInt32(parameter);

            if (value == null) return string.Empty;
   
            else if(text.Length > parametr) 
            {
                return text.Substring(0, parametr) + "...";
            }
            else return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
