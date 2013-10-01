using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;

namespace Ais.Internal.Dcm.ModernUIV2.Converter
{
    public class TagConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var tagString = string.Empty;
            if (value != null)
            {
                var tagList = value as List<Tag>;
                foreach (var tag in tagList)
                {
                    if (!string.IsNullOrWhiteSpace(tag.Name))
                        tagString += tag.Name + ", ";
                }
                tagString = !(string.IsNullOrWhiteSpace(tagString)) ? tagString.Remove(tagString.LastIndexOf(',')) : string.Empty;
            }
            return tagString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
