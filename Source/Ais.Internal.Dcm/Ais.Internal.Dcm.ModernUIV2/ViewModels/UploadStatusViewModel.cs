using System.ComponentModel;

namespace Ais.Internal.Dcm.ModernUIV2.ViewModels
{
    public class UploadStatusViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string fileName;
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChanged("FileName"); }
        }

        private int percentage;
        public int Percentage { get { return percentage; } set { percentage = value; OnPropertyChanged("Percentage"); } }

    }
}
