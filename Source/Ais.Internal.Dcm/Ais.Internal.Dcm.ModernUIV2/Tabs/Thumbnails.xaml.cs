using System.Net.Http;
using System.Threading.Tasks;
using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NLog;

namespace Ais.Internal.Dcm.ModernUIV2.Tabs
{
    /// <summary>
    /// Interaction logic for Thumbnails.xaml
    /// </summary>
    public partial class Thumbnails : UserControl,IContent,INotifyPropertyChanged, IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private ObservableCollection<ThumbnailModel> _urls = null;
        private AssetInfo selectedAsset;
        public AssetInfo SelectedAsset { get { return selectedAsset; } set { selectedAsset = value; OnPropertyChanged("SelectedAsset"); } }

        /// <summary>
        /// Represents the Asset Under Management
        /// </summary>
        public string SelectedAssetId { get; set; }

        /// <summary>
        /// Bindable thumbnail urls of the asset
        /// </summary>
        public ObservableCollection<ThumbnailModel> Urls
        {
            get { return _urls; }
            set
            {
                _urls = value; OnPropertyChanged("Urls");
            }
        }

        HttpClient client = new HttpClient();
 
        public Thumbnails()
        {
            InitializeComponent();
            client = App.GetHttpClient();
        }

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

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            Loaded -= Thumbnails_Loaded;
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            Loaded -= Thumbnails_Loaded;
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
                    if (MediaContextHelper.ThumbnailUrls != null && MediaContextHelper.ThumbnailUrls.Count > 0)
                    {
                        if (Urls == null)
                        {
                            loading.Visibility = Visibility.Collapsed;
                            Urls = new ObservableCollection<ThumbnailModel>(MediaContextHelper.ThumbnailUrls);
                            this.DataContext = this;
                        }
                        Loaded -= Thumbnails_Loaded;
                    }
                    else
                    {
                        Loaded += Thumbnails_Loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private async void Thumbnails_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var urls = await FetchThumbnails();
                Urls = new ObservableCollection<ThumbnailModel>(urls);
                this.DataContext = this;
                loading.Visibility = Visibility.Collapsed;
                ThumbnailGrid.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }

        private async Task<List<ThumbnailModel>> FetchThumbnails()
        {
            var thumbnails = new List<ThumbnailModel>();
            try
            {
                string uri = string.Format(Literals.URL_GET_THUMBNAILS,
                    SelectedAssetId);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<IEnumerable<ThumbnailModel>>().Result;
                thumbnails = responseAsset as List<ThumbnailModel>;
                return thumbnails;
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
            return thumbnails;
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
           
        }

        private void Reload_Thumbnails(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                Urls = null;
                Thumbnails_Loaded(sender, e);
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message, ex);
                UIHelper.HandlerException(ex);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        
        private void Image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    var thumbnailModel = btn.DataContext as ThumbnailModel;
                    if (thumbnailModel != null)
                    {
                        string url = thumbnailModel.URL;
                        Clipboard.SetDataObject(url, true);
                    }
                    //ModernDialog dialog = new ModernDialog();
                    //dialog.Title = "URL copied to clipboard";
                    //dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                logger.LogException(LogLevel.Error, ex.Message,ex);
                UIHelper.HandlerException(ex);
            }
        }

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

        private void Display_About(object sender, RoutedEventArgs e)
        {

        }
    }
}
