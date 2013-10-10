using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using NLog;

namespace Ais.Internal.Dcm.ModernUIV2.Tabs
{
    /// <summary>
    /// Interaction logic for UploadDialog.xaml
    /// </summary>
    public partial class CloudUploadDialog : Window, INotifyPropertyChanged, IDisposable
    {
        #region Constructor
        public CloudUploadDialog(string assetId)
        {
            InitializeComponent();
            client = App.GetHttpClient();
            selectedAssetId = assetId;
            this.Loaded += CloudUploadDialog_Loaded;
        }
        #endregion
        
        #region Properties
        HttpClient client = new HttpClient();
        public string selectedAssetId { get; set; }

        //private ObservableCollection<AssetFile> assetFileList;
        public List<AssetFileInfo> AssetFileList { get; set; }
        //{
        //    get { return assetFileList; }
        //    set
        //    {
        //        assetFileList = value; OnPropertyChanged("AssetFileList");
        //    }
        //}

        private AssetFileInfo selectedAssetFile;
        public AssetFileInfo SelectedAssetFile
        {
            get { return selectedAssetFile; }
            set
            {
                selectedAssetFile = value;
                OnPropertyChanged("SelectedAssetFile");
            }
        }

        private bool hasFiles;
        public bool HasFiles
        {
            get { return hasFiles; }
            set
            {
                hasFiles = value;
                OnPropertyChanged("HasFiles");
            }
        }

        private Logger logger = LogManager.GetCurrentClassLogger(); 
        #endregion

        #region Event Handlers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event EventHandler AssetFileSelected;
        protected void OnAssetFileSelected()
        {
            if (AssetFileSelected != null)
            {
                AssetFileSelected(this, new AssetFileEventArgs(SelectedAssetFile));
            }
        }
        #endregion       

        #region Private Methods
        void CloudUploadDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Task<Task<List<AssetFileInfo>>>.Factory.StartNew(FetchAssetFiles)
                                                .ContinueWith((t) =>
                                                {
                                                    MediaContextHelper.AssetFileList = t.Result.Result;
                                                    AssetFileList =
                                                        new List<AssetFileInfo>(
                                                            MediaContextHelper.AssetFileList);

                                                    Application.Current.Dispatcher.BeginInvoke(
                                                        DispatcherPriority.Normal,
                                                        new Action(
                                                            () =>
                                                            {
                                                                this.HasFiles =
                                                                    !(MediaContextHelper.AssetFileList.Count > 0);
                                                                this.DataContext = this;
                                                                LoadingMediaGrid.Visibility = Visibility.Collapsed;
                                                                dataGrid.Visibility = Visibility.Visible;
                                                            }));

                                                });
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private async Task<List<AssetFileInfo>> FetchAssetFiles()
        {
            var assetFiles = new List<AssetFileInfo>();
            try
            {
                string uri = string.Format(Literals.URL_GET_ASSETFILES,
                                           MediaContextHelper.accountName, selectedAssetId);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<IEnumerable<AssetFileInfo>>().Result;
                assetFiles = responseAsset as List<AssetFileInfo>;
                return assetFiles;
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                //UIHelper.HandlerException(ex);
            }
            return assetFiles;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedAssetFile != null)
                OnAssetFileSelected();
            this.Close();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedAssetFile != null)
                OnAssetFileSelected();
            this.Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed) // if it's not right button
                {
                    this.DragMove();
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        } 
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null) client.Dispose();
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } 
        #endregion
    }

    public class AssetFileEventArgs : EventArgs
    {
        public AssetFileInfo AssetFile { get; private set; }

        public AssetFileEventArgs(AssetFileInfo assetFile)
        {
            AssetFile = assetFile;
        }
    }
}
