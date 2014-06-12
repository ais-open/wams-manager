using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Ais.Internal.Dcm.ModernUIV2.Converter
{
    /// <summary>
    /// This class converts input size value and presents it in KB/MB/GB.
    /// For example if the value is passed as 1024, it converts and returns 1 KB.
    /// </summary>
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || value == System.DBNull.Value)
                return string.Empty;

            double size = System.Convert.ToDouble(value);
            long bytesSize = System.Convert.ToInt64(value);
            StringBuilder stringBuilder = new StringBuilder();
            string str;
            string sizeAsString;
            if (size / 1024 < 1)
            {
                sizeAsString = size.ToString(CultureInfo.CurrentCulture);
                stringBuilder.AppendFormat("{0} {1}", sizeAsString, "Bytes");
                str = stringBuilder.ToString();
            }
            else
            {
                size /= 1024;
                if (size / 1024 < 1)
                {
                    sizeAsString = Math.Round(size, 2).ToString(CultureInfo.CurrentCulture);
                    stringBuilder.AppendFormat("{0} {1}", sizeAsString, "KB");
                    str = stringBuilder.ToString();
                }
                else
                {
                    size /= 1024;
                    if (size / 1024 < 1)
                    {
                        sizeAsString = Math.Round(size, 2).ToString(CultureInfo.CurrentCulture);
                        stringBuilder.AppendFormat("{0} {1}", sizeAsString, "MB");
                        str = stringBuilder.ToString();
                    }
                    else
                    {
                        size /= 1024;
                        sizeAsString = Math.Round(size, 2).ToString(CultureInfo.CurrentCulture);
                        stringBuilder.AppendFormat("{0} {1}", sizeAsString, "GB");
                        str = stringBuilder.ToString();
                    }
                }
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

}
