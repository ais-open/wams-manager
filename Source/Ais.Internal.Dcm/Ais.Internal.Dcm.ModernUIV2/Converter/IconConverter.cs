using Ais.Internal.Dcm.ModernUIV2.Common;
using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Ais.Internal.Dcm.ModernUIV2.Converter
{
    /// <summary>
    /// Given a file name returns the default icon associated with file type
    /// </summary>
    public class FileIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BitmapImage bImage = null;
            string explorerObjectName = value as string;
            bImage = IconHelper.GetIconImageFromFilename(explorerObjectName);
            return bImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
