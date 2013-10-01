using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NLog;

namespace Ais.Internal.Dcm.ModernUIV2.Tabs
{
    /// <summary>
    /// Interaction logic for Outputs.xaml
    /// </summary>
    public partial class Outputs : UserControl, IContent, INotifyPropertyChanged, IDisposable
    {
        #region Constructor
        public Outputs()
        {
            InitializeComponent();
            client = App.GetHttpClient();
        }
        #endregion

        #region Properties
        HttpClient client = new HttpClient();

        private Logger logger = LogManager.GetCurrentClassLogger();

        private const int THUMBNAIL_PAGING_SIZE = 4;

        private AssetInfo selectedAsset;

        public AssetInfo SelectedAsset { get { return selectedAsset; } set { selectedAsset = value; OnPropertyChanged("SelectedAsset"); } }

        /// <summary>
        /// Represents the Asset Under Management
        /// </summary>
        public string SelectedAssetId { get; set; }

        private ObservableCollection<GroupedOutputViewModel> _groupedOutputFiles = null;

        /// <summary>
        /// Bindable Grouped outputs of the asset
        /// </summary>
        public ObservableCollection<GroupedOutputViewModel> GroupedOutputFiles
        {
            get { return _groupedOutputFiles; }
            set
            {
                _groupedOutputFiles = value; OnPropertyChanged("GroupedOutputFiles");
            }
        }

        private ObservableCollection<JobInfo> jobStatus;
        public ObservableCollection<JobInfo> JobStatus
        {
            get { return jobStatus; }
            set
            {
                jobStatus = value;

                OnPropertyChanged("JobStatus");
            }
        } 
        #endregion

        #region Event Handlers
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wrapper around PropertyChanged Event
        /// </summary>
        /// <param name="propertyName">name of the property</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Navigation Methods
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                Loaded -= Output_Loaded;
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                Loaded -= Output_Loaded;
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, (FrameworkElement)this.Parent);
                if (frame != null)
                {
                    var source = frame.Source;
                    string assetId = source.ToString().Split('#')[1];
                    SelectedAssetId = assetId;
                    SelectedAsset = MediaContextHelper.SelectedAsset;
                    if (MediaContextHelper.GroupedOutputs != null && MediaContextHelper.GroupedOutputs.Count > 0)
                    {
                        if (GroupedOutputFiles == null)
                        {
                            loading.Visibility = Visibility.Collapsed;
                            VideoGrid.Visibility = Visibility.Visible;
                            GroupedOutputFiles = new ObservableCollection<GroupedOutputViewModel>(MediaContextHelper.GroupedOutputs);
                            this.DataContext = this;
                        }
                        Loaded -= Output_Loaded;
                    }
                    else
                    {
                        Loaded += Output_Loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                this.Loaded -= Output_Loaded;
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }
        #endregion

        #region Private Methods
        private async void Output_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await RefreshOutputsAndStatus();

                MediaContextHelper.GroupedOutputs = await FetchGroupedOutputFiles();
                GroupedOutputFiles =
                    new ObservableCollection<GroupedOutputViewModel>(
                        MediaContextHelper.GroupedOutputs);

               await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action(
                        () =>
                        {
                            this.DataContext = this;
                            loading.Visibility = Visibility.Collapsed;
                            VideoGrid.Visibility = Visibility.Visible;
                        }));
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        async Task RefreshOutputsAndStatus()
        {
            var jobsQueued = new List<JobInfo>();
            try
            {
                string uri = string.Format("api/media/Refresh?mediaServiceName={0}", MediaContextHelper.accountName);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private async Task<List<GroupedOutputViewModel>> FetchGroupedOutputFiles()
        {
            var outputEncodings = new List<GroupedOutputViewModel>();
            try
            {
                string uri = string.Format(Literals.URL_GET_GROUPED_OUTPUT,
                                           MediaContextHelper.accountName, SelectedAssetId);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<IEnumerable<GroupedOutputViewModel>>().Result;
                outputEncodings = responseAsset as List<GroupedOutputViewModel>;
                return outputEncodings;
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
            return outputEncodings;
        }

        private void Reload_Outputs(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                VideoGrid.Visibility = Visibility.Collapsed;
                GroupedOutputFiles = null;
                Output_Loaded(sender, e);
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private void CopyToClipBoard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GroupedOutputViewModel file = GetModel(sender);
                ThumbnailRollViewModel model = null;
                string url = GetMediaUrl(sender);
                if (file != null)
                {
                    if (file.Thumbnails != null)
                    {
                        var paging = Math.Min(file.Thumbnails.Count, THUMBNAIL_PAGING_SIZE);
                        model = new ThumbnailRollViewModel(file.Thumbnails, paging);
                    }
                    ChooseThumbnailPopup dialog = new ChooseThumbnailPopup(url, model);
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }

        }

        private GroupedOutputViewModel GetModel(object sender)
        {
            Button btnCopy = (Button)sender;
            if (btnCopy != null)
            {
                GroupedOutputViewModel file = btnCopy.CommandParameter as GroupedOutputViewModel;
                return file;
            }
            return null;
        }

        private string GetMediaUrl(object sender)
        {
            Button btnCopy = (Button)sender;
            if (btnCopy != null)
            {
                var url = btnCopy.DataContext as VideoOutput;
                return url.Url;
            }
            return string.Empty;
        }

        private void GenerateSmoothStreamingHtml(string url)
        {
            try
            {
                string curDir = Directory.GetCurrentDirectory();
                string content = string.Empty;
                StringBuilder output = new StringBuilder();
                string path = curDir + "\\preview-template-ss.html";
                string fileContents = File.ReadAllText(path);
                fileContents = fileContents.Replace("[mediafileurl]", url);
                string newPath = curDir + "\\preview.html";
                File.WriteAllText(newPath, fileContents);
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private void GenerateMp4Html(string url)
        {
            try
            {
                string curDir = Directory.GetCurrentDirectory();
                string content = string.Empty;
                StringBuilder output = new StringBuilder();
                string path = curDir + "\\preview-template-mp4.html";
                string fileContents = File.ReadAllText(path);
                fileContents = fileContents.Replace("[mediafileurl]", url);
                string newPath = curDir + "\\preview.html";
                File.WriteAllText(newPath, fileContents);
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string file = GetMediaUrl(sender);
                string url = string.Format("../Tabs/Viewer.xaml");
                if (file != null)
                {
                    if (file.Contains(".ism"))
                    {
                        GenerateSmoothStreamingHtml(file);
                        MediaContextHelper.SelectedAssetType = "SMOOTHSTREAMING";
                    }
                    else
                    {
                        GenerateMp4Html(file);
                        MediaContextHelper.SelectedAssetType = "MP4";
                    }
                }
                PreviewWindow window = new PreviewWindow();
                window.CloseButton.Content = "Close";
                window.Show();
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
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
}

