using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using NLog;

namespace Ais.Internal.Dcm.ModernUIV2.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page, IContent, INotifyPropertyChanged, IDisposable
    {
        #region Private Members
        /// <summary>
        /// http client to make
        /// </summary>
        private HttpClient client = null;
        /// <summary>
        /// reference of logger
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Home()
        {
            InitializeComponent();
            client = App.GetHttpClient();
            Loaded += Home_Loaded;
        }

        #region Binding Properties
        private ObservableCollection<AssetInfo> assetList;
        public ObservableCollection<AssetInfo> AssetList
        {
            get { return assetList; }
            set
            {
                assetList = value; OnPropertyChanged("AssetList");
            }
        }


        private string mediaServiceFriendlyName;
        public string MediaServiceFriendlyName
        {
            get { return mediaServiceFriendlyName; }
            set
            {
                mediaServiceFriendlyName = value;
                OnPropertyChanged("MediaServiceFriendlyName");
            }
        }

        private string mediaServiceName;
        public string MediaServiceName
        {
            get { return mediaServiceName; }
            set { mediaServiceName = value; OnPropertyChanged("MediaServiceName"); }
        }
        #endregion

        #region Async Operations
        private async void Home_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                var assets = await FetchAssets();
                AssetList = new ObservableCollection<AssetInfo>((assets).OrderBy(ast => ast.Name));

                await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(
                    () =>
                    {
                        DataContext = this;
                        LoadingAssetsGrid.Visibility = Visibility.Collapsed;
                        AssetsGrid.Visibility = Visibility.Visible;
                    }));
                this.Loaded -= Home_Loaded;
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS;//
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
        }
      
        private async Task<List<AssetInfo>> FetchAssets()
        {
            var assets = new List<AssetInfo>();
            try
            {
                string uri = string.Format(Literals.URL_GET_ASSETS, MediaContextHelper.accountName);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<IEnumerable<AssetInfo>>().Result;
                assets = responseAsset as List<AssetInfo>;
                return assets;
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS; //"Error while retrieving assets.";
                logger.LogException(LogLevel.Error, message, exception);
            }
            return assets;
        }
        #endregion

        #region Event Handler and Navigations
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ButtonDeleteAsset.IsEnabled = true;
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var frame = NavigationHelper.FindFrame(null, this);
                    if (frame != null)
                    {
                       // UploadHelper.RemoveAllFromQueue();
                        GoToAssetPage(e.AddedItems[0] as AssetInfo);
                        frame.KeepContentAlive = false;
                    }
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES;// "Error while working with an asset.";
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void ButtonAddAsset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AssetWindow newAssetWindow = new AssetWindow(client, MediaContextHelper.accountName);
                newAssetWindow.AssetCreated += newAssetWindow_AssetCreated;
                newAssetWindow.ShowDialog();
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_CREATING_ASSET; //"Error while creating an asset.";
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void newAssetWindow_AssetCreated(object sender, EventArgs e)
        {
            try
            {
                var asset = (e as AssetEventArgs).Asset;
                GoToAssetPage(asset);
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void GoToAssetPage(AssetInfo asset)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null && asset != null)
                {
                    if ((MediaContextHelper.SelectedAsset != null) && (asset.Id != MediaContextHelper.SelectedAsset.Id))
                    {                        
                        MediaContextHelper.AssetFileList = null;
                        MediaContextHelper.ThumbnailUrls = null;
                        MediaContextHelper.OutputUrls = null;
                        MediaContextHelper.GroupedOutputs = null;
                    }
                    MediaContextHelper.SelectedAsset = asset;
                    frame.Source = new Uri(string.Format("../Pages/AssetLandingPage.xaml#{0}", asset.Id),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES; //"Error while navigating to asset details.";
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void RefreshAsset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingAssetsGrid.Visibility = Visibility.Visible;
                AssetsGrid.Visibility = Visibility.Collapsed;
                MediaContextHelper.AssetList = null;
                AssetList = null;
                Home_Loaded(sender, e);
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES; //"Error while refreshing to asset details.";
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
          // empty method for the modern UI framework
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
               // Loaded -= Home_Loaded; // requirement of the moder UI framework to avoid multi reload
            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }
        
        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null,  this);
                frame = App.SelectedFrame;
                if (frame != null)
                {
                    var source = frame.Source;                              // reading media service name and friendly name 
                    string serviceParam = source.ToString().Split('#')[1];  // which are passed as paramter
                    string[] serviceValues = serviceParam.Split(';');       // separated by ';'
                    MediaServiceName = serviceValues[0];                    // first param is mediaservice
                    MediaServiceFriendlyName = serviceValues[1];            // second param is media service friendlyname

                    if (!string.IsNullOrWhiteSpace(mediaServiceName) // if not empty and same update the instance
                        &&
                        string.CompareOrdinal(mediaServiceName, MediaContextHelper.accountName) != 0)
                    {
                        MediaContextHelper.accountName = mediaServiceName;
                        MediaContextHelper.accountKey = mediaServiceFriendlyName;
                        MediaContextHelper.UpdateInstance();
                        AssetList = null;
                    }
                    
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                //Loaded -= Home_Loaded;
            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        private void Display_About(object sender, RoutedEventArgs e)
        {
            Home parentwindow;
            try
            {
                AboutWindow window = new AboutWindow();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_ABOUT; //"Error while loading About  Window.";
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void NavigateToHome(object sender, RoutedEventArgs e)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                App.SelectedFrame = frame;
                if (frame != null)
                {
                    frame.Source = new Uri(string.Format("../Pages/MediaServiceHome.xaml"),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = "Error while navigating to MediaService page.";
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Clean up
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null) client.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
