using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class EncodingTypeModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string mediaServiceName;
        public string MediaServiceName
        {
            get { return mediaServiceName; }
            set { mediaServiceName = value; OnPropertyChanged("MediaServiceName"); }
        }

        private string technicalName;
        public string TechnicalName { get { return technicalName; } set { technicalName = value; OnPropertyChanged("TechnicalName"); } }

        private string friendlyName;
        public string FriendlyName { get { return friendlyName; } set { friendlyName = value; OnPropertyChanged("FriendlyName"); } }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; OnPropertyChanged("IsSelected"); }
        }
    }
}
