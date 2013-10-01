using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.ModernUI.Common;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ais.Internal.Dcm.ModernUI.Pages
{
    /// <summary>
    /// Interaction logic for CreateEncodingJobDialog.xaml
    /// </summary>
    public partial class CreateThumbnailJobDialog : ModernDialog,INotifyPropertyChanged
    {
        public event EventHandler JobCreated;
        protected void OnJobCreated()
        {
            if (JobCreated != null)
            {
                JobCreated(this, new JobEventArgs(Job));
            }
        }

        private string _selectedAssetID = string.Empty;
        private string _selectedThumbnailTypes = string.Empty;
        public bool IsButtonEnabled { get { return !string.IsNullOrWhiteSpace(_selectedThumbnailTypes); } }
        private Job Job { get; set; }

        public CreateThumbnailJobDialog(string assetID)
            : base()
        {
            try
            {
                this._selectedAssetID = assetID;
                InitializeComponent();
                ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
                CloseButton.Visibility = System.Windows.Visibility.Hidden;
                //
                ThumbnailTypes = new ObservableCollection<string>(GetThumbnailTypes());
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        public ObservableCollection<string> ThumbnailTypes
        {
            get { return _encodingTypes; }
            set { _encodingTypes = value; NotifyPropertyChanged("ThumbnailTypes"); }
        }

        private ObservableCollection<string> _encodingTypes;
        private CreateThumbnailJobDialog()
        {
           
        }

        private List<string> GetThumbnailTypes()
        {
            try
            {
                string[] str = new string[] {"Highest Resolution", "Medium Resolution", "Low Resolution"};
                return str.ToList();
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
            return new List<string>();
        }


        private void NotifyPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    _selectedThumbnailTypes = (string) e.AddedItems[0];
                    this.NotifyPropertyChanged("IsButtonEnabled");
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                var jobWorker = new BackgroundWorker();
                jobWorker.DoWork += (o, args) =>
                    {
                        var asset = MediaContextHelper.SelectedAsset;
                        int imageResolution = GetResolution(_selectedThumbnailTypes);
                        args.Result = asset.CreateThumbnailJob(imageResolution);

                    };
                jobWorker.RunWorkerCompleted += (o, args) =>
                    {
                        loading.Visibility = Visibility.Collapsed;
                        Job = args.Result as Job;
                        OnJobCreated();
                        this.Close();
                    };

                if (jobWorker.IsBusy != true)
                {
                    jobWorker.RunWorkerAsync();
                    loading.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private int GetResolution(string _selectedThumbnailTypes)
        {
            int imageQuality = 0;
            try
            {
                switch (_selectedThumbnailTypes)
                {
                    case "Highest Resolution":
                        imageQuality = 100;
                        break;
                    case "Medium Resolution":
                        imageQuality = 50;
                        break;
                    case "Low Resolution":
                        imageQuality = 0;
                        break;
                }
                return imageQuality;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
            return imageQuality;
        }

        public ObservableCollection<string> _thumbnailTypes { get; set; }
    }
}
