using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
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
using System.Windows.Threading;
using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows.Navigation;
using System.ComponentModel;
using NLog;

namespace Ais.Internal.Dcm.ModernUIV2.Pages
{
    /// <summary>
    /// Interaction logic for MediaServiceHome.xaml
    /// </summary>
    public partial class MediaServiceHome : Page, INotifyPropertyChanged, IDisposable
    {
        #region Private members
        /// <summary>
        /// HttpClient used to make API request
        /// </summary>
        private HttpClient client = null;
        /// <summary>
        /// logger
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Change Settings and URL
        /// </summary>
        private URLWindow window = null;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaServiceHome()
        {
            InitializeComponent();
            client = App.GetHttpClient();
            window = new URLWindow();
            window.URLChanged += MediaServiceHome_Loaded;
            Loaded += MediaServiceHome_Loaded;
        }

        #region Binding Properties
        private ObservableCollection<MediaServiceInfo> mediaServiceList;
        public ObservableCollection<MediaServiceInfo> MediaServiceList
        {
            get { return mediaServiceList; }
            set
            {
                mediaServiceList = value;
                OnPropertyChanged("MediaServiceList");
            }
        }

        private MediaServiceInfo selectedMediaService;
        public MediaServiceInfo SelectedMediaService
        {
            get { return selectedMediaService; }
            set
            {
                selectedMediaService = value;
                OnPropertyChanged("SelectedMediaService");
            }
        }
        #endregion

        #region Event Handlers and Navigation
        private async void MediaServiceHome_Loaded(object sender, EventArgs e)
        {
            try
            {
                LoadingMediaGrid.Visibility = Visibility.Visible;
                MediaDataGrid.Visibility = Visibility.Collapsed;
                client = App.GetHttpClient();
                await ReloadServices(); // reload collections
                this.Loaded -= MediaServiceHome_Loaded;
            }
            catch (Exception exception)
            {
                LoadingMediaGrid.Visibility = Visibility.Collapsed;
                MediaDataGrid.Visibility = Visibility.Visible;
                string message = Literals.MESSAGE_ERROR_NAVIGATION_MEDIASERVICE_PAGE; ;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                LaunchSettingsWindow();
                logger.LogException(LogLevel.Error, message + exception.ToString(), exception);
            }
        }


        private async Task ReloadServices()
        {
            try
            {
                var mediaList = await ListMediaServices("client-key");
                LoadingMediaGrid.Visibility = Visibility.Collapsed;
                MediaDataGrid.Visibility = Visibility.Visible;
                MediaServiceList = new ObservableCollection<MediaServiceInfo>(mediaList);

                DataContext = this;
            }
            catch (Exception)
            {
                throw; // throw so that handler catches it
            }
        }
        private async Task<List<MediaServiceInfo>> ListMediaServices(string clientKey)
        {
            try
            {
                var response = await App.GetHttpClient().GetAsync(string.Format("api/media/MediaServices?clientKey={0}", clientKey));
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var mediaList = await response.Content.ReadAsAsync<IEnumerable<MediaServiceInfo>>();
                return mediaList as List<MediaServiceInfo>;
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_MEDIASERVICE_PAGE;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message + exception.ToString(), exception);
                throw;
            }
        }

        private async void Reload_Services(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadingMediaGrid.Visibility = Visibility.Visible;
                MediaDataGrid.Visibility = Visibility.Collapsed;
                await ReloadServices();
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_MEDIASERVICE_PAGE;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void Display_Settings(object sender, RoutedEventArgs e)
        {
            try
            {
                LaunchSettingsWindow();
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_MEDIASERVICE_PAGE;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void LaunchSettingsWindow()
        {
            try
            {
                window = new URLWindow();
                window.URLChanged += MediaServiceHome_Loaded;
                window.ShowDialog();
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private  void Display_About(object sender, RoutedEventArgs e)
        {
            try
            {
                AboutWindow window = new AboutWindow();
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_ABOUT;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }
       

        private void GoToMediaPage(MediaServiceInfo service)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                App.SelectedFrame = frame;
                if (frame != null && service != null)
                {
                    var param = string.Format("{0};{1}", service.AccountName, service.MediaServiceFriendlyName);
                    frame.Source = new Uri(string.Format("../Pages/Home.xaml#{0}", param),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = "Error while navigating to asset page.";
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void ListBoxItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var media = this.SelectedMediaService;
                GoToMediaPage(media);
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_ASSET_PAGE; ;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    var param = txtSearchBox.Text;
                    RedirectToSearch(param);
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_ASSET_PAGE;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }

        }

        private void RedirectToSearch(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
            {
                var frame = NavigationHelper.FindFrame(null, this);
                App.SelectedFrame = frame;
                if (frame != null)
                {
                    frame.Source = new Uri(string.Format("../Pages/SearchResults.xaml#{0}", searchString),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var text = txtSearchBox.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    RedirectToSearch(text);
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void MediaServiceHome_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var media = this.SelectedMediaService;
                GoToMediaPage(media);
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_ASSET_PAGE; ;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
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
    }
}
