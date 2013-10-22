using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Ais.Internal.Dcm.ModernUIV2.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class SearchResult : Page, IContent, INotifyPropertyChanged, IDisposable
    {
        #region Private members
        /// <summary>
        /// Client used to make API calls
        /// </summary>
        private HttpClient client = null;
        /// <summary>
        /// logger
        /// </summary>
        private Logger logger = LogManager.GetCurrentClassLogger();

        private const int PAGE_SIZE = 30;
        /// <summary>
        /// Current Page
        /// </summary>
        private int currentPage = 0;
        /// <summary>
        /// Total number of records
        /// </summary>
        private int totalRecordsCount = 0;
        /// <summary>
        /// Due to sql lite implementation the page size varies depending on the records searched.
        /// hence it cannot be constant and initialezed with a const value
        /// </summary>
        private int pageSize = PAGE_SIZE;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SearchResult()
        {
            InitializeComponent();
            client = App.GetHttpClient();
            Loaded += SearchResult_Loaded;
        }

        #region Bindable Properties
        private PagingCollectionViewModel assetList;
        public PagingCollectionViewModel AssetList
        {
            get { return assetList; }
            set
            {
                assetList = value; 
                OnPropertyChanged("AssetList");
                OnPropertyChanged("PagingAllowed");
                OnPropertyChanged("CurrentPage");
                OnPropertyChanged("PageCount");
                OnPropertyChanged("PreviousAllowed");
                OnPropertyChanged("NextAllowed");
            }
        }
        
        private string searchDescription;
        public string SearchDescription
        {
            get { return searchDescription; }
            set
            {
                searchDescription = value;
                OnPropertyChanged("SearchDescription");
            }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                OnPropertyChanged("SearchText");
            }
        }
       
        private string searchType;
        public string SearchType
        {
            get { return searchType; }
            set
            {
                searchType = value;
                OnPropertyChanged("SearchType");
            }
        }

        private string mediaServiceName;
        public string MediaServiceName
        {
            get { return mediaServiceName; }
            set { mediaServiceName = value; OnPropertyChanged("MediaServiceName"); }
        }

        public bool PagingAllowed
        {
            get { return (this.assetList != null && this.assetList.PagingAllowed); }
        }

        public int CurrentPage
        {
            get { return currentPage+1; }// 0 based index
        }


        public int PageCount
        {
            get { return GetPageCount(); }
        }


        private int GetPageCount()
        {
            int pageCount = (totalRecordsCount / pageSize);
            if (totalRecordsCount % pageSize != 0)
                pageCount++;
            return pageCount;

        }
        public bool PreviousAllowed
        {
            get { return currentPage > 0; }
        }
        public bool NextAllowed
        {
            get { return CurrentPage < PageCount; }
        }
        #endregion
        #region Search
        private async Task PerformSearch(string searchString, string searchType)
        {
            int searchTypeParam = 0;
            if (string.IsNullOrEmpty(searchString))
            {
                return;
            }
            if (searchType == "Tag")
            {
                searchTypeParam = 1;
            }

            int rowsToSkip = (currentPage) * pageSize;
            var list = await SearchMedia(searchString, rowsToSkip, pageSize, searchTypeParam);

            if (list != null && list.Data != null && list.Data.Count > 0)
            {
                totalRecordsCount = (int)list.TotalCount;
                pageSize = Math.Min(PAGE_SIZE, list.Data.Count);
                if (totalRecordsCount > 0)
                {
                    AssetList = new PagingCollectionViewModel(list.Data, Math.Min(pageSize, list.Data.Count), totalRecordsCount);
                    AssetList.MoveToNext = MoveToNext;
                    AssetList.MoveToPrev = MoveToPrev;
                    SearchDescription = "Search Results for ";
                }
            }
          else
            {
                AssetList = null;
                SearchDescription = "No results found for ";
            }
            this.SearchText = searchString;
        }

        private async void MoveToNext()
        {
            if (CurrentPage < PageCount)
            {
                ++currentPage;
                await PerformSearch(this.searchText, this.searchType);
            }
        }
        private async void MoveToPrev()
        {
            if (currentPage > 0)
            {
                --currentPage;
                await PerformSearch(this.searchText, this.searchType);
            }
        }
        private async Task<SearchData> SearchMedia(string searchString, int rowsToSkip, int rowsToRetrieve,int searchType)
        {
            var searchData = new SearchData();
            try
            {
                string uri = string.Format(
                    "api/media/SearchMedia?searchString={0}&rowsToSkip={1}&rowsToRetrieve={2}&searchType={3}", searchString, rowsToSkip,
                    rowsToRetrieve,searchType);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                searchData = response.Content.ReadAsAsync<SearchData>().Result;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, "search", exception);
            }
            return searchData;
        }
        #endregion

        #region Event Handler and Navigations
        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Return)
                {
                    var searchText = txtSearchBox.Text;
                    currentPage = 0;
                    pageSize = PAGE_SIZE;
                    PerformSearch(searchText, "");
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_SEARCH;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }

        }

        private void OnNextClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.assetList != null)
                {
                    this.assetList.MoveToNextPage();
                    //PerformSearch(this.SearchText, "");
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
        }

        private void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.assetList != null)
                {
                    this.assetList.MoveToPreviousPage();
                    //PerformSearch(this.SearchText, "");
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
        }

        private void SearchResult_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                //AssetList = ReturnSearchResult(SearchText);// ;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(
                    () =>
                    {
                        DataContext = this;
                        LoadingAssetsGrid.Visibility =
                            Visibility.Collapsed;
                        AssetsGrid.Visibility = Visibility.Visible;
                    }));
                Loaded -= SearchResult_Loaded;

            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
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
                    }
                    MediaContextHelper.SelectedAsset = asset;
                    frame.Source = new Uri(string.Format("../Pages/AssetLandingPage.xaml#{0}", asset.Id),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
           
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                //Loaded -= SearchResult_Loaded;
             
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
                var frame = NavigationHelper.FindFrame(null, this);
                frame = App.SelectedFrame;
                if (frame != null)
                {
                    var source = frame.Source;
                    string searchString = source.ToString().Split('#')[1]; // split ??
                    this.searchText = searchString;
                    currentPage = 0;
                    pageSize = PAGE_SIZE;
                    PerformSearch(searchString,"");
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
           // place holder method
        }


        private void Display_About(object sender, RoutedEventArgs e)
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


        private void TagSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                var tagSearch = (string)btn.Content;
                searchType = "Tag";
                currentPage = 0;
                pageSize = PAGE_SIZE;
                PerformSearch(tagSearch, searchType);
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void Navigate_To_Collection(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                SearchResultViewModel data = btn.DataContext as SearchResultViewModel;
                if (data != null)
                {
                    GoToMediaPage(data.MediaServiceName, data.CollectionName);
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }

        }
        private void Navigate_To_Album(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = (Button)sender;
                SearchResultViewModel data = btn.DataContext as SearchResultViewModel;
                if (data != null)
                {
                    GoToAlbum(data);
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void GoToAlbum(SearchResultViewModel model)
        {
           
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                if (frame != null && model != null)
                {
                    if ((MediaContextHelper.SelectedAsset != null) && (model.ParentAssetId != MediaContextHelper.SelectedAsset.Id))
                    {                        
                        MediaContextHelper.AssetFileList = null;
                        MediaContextHelper.ThumbnailUrls = null;
                        MediaContextHelper.OutputUrls = null;
                    }
                    MediaContextHelper.SelectedAsset = new AssetInfo { Id = model.ParentAssetId, MediaServiceName = model.MediaServiceName, Name = model.AlbumName };
                    
                    frame.Source = new Uri(string.Format("../Pages/AssetLandingPage.xaml#{0}", model.ParentAssetId),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        
        }

        private void GoToMediaPage(string mediaServiceName, string friendlyName)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                App.SelectedFrame = frame;
                if (frame != null && !string.IsNullOrEmpty(mediaServiceName))
                {
                    var param = string.Format("{0};{1}", mediaServiceName, friendlyName);
                    frame.Source = new Uri(string.Format("../Pages/Home.xaml#{0}", param),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message =Literals.MESSAGE_ERROR_NAVIGATION_ASSET_PAGE;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var text = txtSearchBox.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    currentPage = 0;
                    pageSize = PAGE_SIZE;
                    PerformSearch(text, "");
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void Facebook_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception exception)
            {
                UIHelper.HandlerException(exception);
            }

        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var soureButton = (Button)sender;
                SearchResultViewModel model = (SearchResultViewModel)soureButton.DataContext;
                ShareViewWindow dialog = new ShareViewWindow();
                Point buttonPosition = soureButton.PointToScreen(new Point(0, 0));
                double adjustedX = Math.Min(buttonPosition.X + 100, this.ActualWidth);
                double adjustedY = buttonPosition.Y - 300;
                dialog.Top = adjustedY;
                dialog.Left = adjustedX;
                dialog.Outputs = model.Outputs;
                dialog.ThumbnailUrl = model.DefaultThumbnailUrl;
                dialog.Show();
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
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
                string message = Literals.URL_LIST_MEDIASERVICE;
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
