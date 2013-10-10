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
    public partial class CreateEncodingJobDialog : ModernDialog,INotifyPropertyChanged
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
        private string _selectedEncodingType = string.Empty;
        public bool IsButtonEnabled { get { return !string.IsNullOrWhiteSpace(_selectedEncodingType); } }
        private Job Job { get; set; }

        public CreateEncodingJobDialog(string assetID)
            : base()
        {
            try
            {
                this._selectedAssetID = assetID;
                InitializeComponent();
                ResizeMode = System.Windows.ResizeMode.CanResizeWithGrip;
                CloseButton.Visibility = System.Windows.Visibility.Hidden;
                //
                _encodingTypes = new ObservableCollection<string>(GetEncodingTypes());
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        public ObservableCollection<string> EncodingTypes
        {
            get { return _encodingTypes; }
            set { _encodingTypes = value; NotifyPropertyChanged("EncodingTypes"); }
        }

        private ObservableCollection<string> _encodingTypes;
        private CreateEncodingJobDialog()
        {
           
        }

        private List<string> GetEncodingTypes()
        {
            try
            {
                AppSettingsReader reader = new AppSettingsReader();
                var v = reader.GetValue("EncodeTypes", typeof (string)).ToString().Split(';').ToList();
                return v;
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
                    _selectedEncodingType = (string) e.AddedItems[0];
                    NotifyPropertyChanged("IsButtonEnabled");
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
                        args.Result = String.CompareOrdinal(_selectedEncodingType, "Apple HLS Format") == 0
                                          ? asset.CreateEncodingJob1(_selectedEncodingType.Trim())
                                          : asset.CreateEncodingJob(_selectedEncodingType.Trim());
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
    }

    public class JobEventArgs : EventArgs
    {
        public Job Job { get; private set; }

        public JobEventArgs(Job job)
        {
            Job = job;
        }
    }
}
