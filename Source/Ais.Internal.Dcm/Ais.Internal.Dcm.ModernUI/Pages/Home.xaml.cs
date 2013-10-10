using System;
using System.Collections.Generic;
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
using System.Windows.Threading;
using Ais.Internal.Dcm.Business;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Ais.Internal.Dcm.ModernUI.Common;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Windows;

namespace Ais.Internal.Dcm.ModernUI.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page, IContent, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<Asset> assetList;
        public ObservableCollection<Asset> AssetList
        {
            get { return assetList; }
            set
            {
                assetList = value; OnPropertyChanged("AssetList");
            }
        }

        private BackgroundWorker listWorker = new BackgroundWorker();

        private string accName;
        private string accKey { get; set; }

        public string MediaServiceName
        {
            get { return accName; }
            set { accName = value; OnPropertyChanged("MediaServiceName"); }
        }

        public Home()
        {
            InitializeComponent();
            //this.Loaded += Home_Loaded;
        }

        private void Home_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (string.CompareOrdinal(accName, MediaContextHelper.accountName) != 0)
                //{
                //    MediaContextHelper.AssetList = null;
                //    AssetList = null;
                //}
                if (MediaContextHelper.AssetList != null && MediaContextHelper.AssetList.Count > 0)
                {
                    if (AssetList == null)
                    {
                        AssetList = new ObservableCollection<Asset>(MediaContextHelper.AssetList);
                        DataContext = this;
                        LoadingAssetsGrid.Visibility = Visibility.Collapsed;
                        AssetsGrid.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    listWorker.WorkerReportsProgress = true;
                    listWorker.WorkerSupportsCancellation = true;

                    if (listWorker.IsBusy != true)
                    {
                        listWorker.DoWork += listWorker_DoWork;
                        listWorker.RunWorkerCompleted += listWorker_RunWorkerCompleted;
                        listWorker.RunWorkerAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void FetchAssets(DoWorkEventArgs e)
        {
            try
            {
                //var assets = MediaContextHelper.Instance.GetAllAssets();
                var assets = MediaContextHelper.Instance.GetAllParentAssets();
                e.Result = assets;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void listWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var assets = e.Result as List<Asset>;
                if (assets != null)
                {
                    //DataContext = new ObservableCollection<Asset>(assets.OrderBy(ast => ast.Name));
                    MediaContextHelper.AssetList = new List<Asset>(assets.OrderBy(ast => ast.Name));
                    AssetList = new ObservableCollection<Asset>(assets.OrderBy(ast => ast.Name));
                    DataContext = this;
                    listWorker.DoWork -= listWorker_DoWork;
                    listWorker.RunWorkerCompleted -= listWorker_RunWorkerCompleted;
                    LoadingAssetsGrid.Visibility = Visibility.Collapsed;
                    AssetsGrid.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void listWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            FetchAssets(e);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //MessageBox.Show(e.AddedItems[0].ToString());
                ButtonDeleteAsset.IsEnabled = true;
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var frame = NavigationHelper.FindFrame(null, this);
                    if (frame != null)
                    {
                        GoToAssetPage(e.AddedItems[0] as Asset);
                        frame.KeepContentAlive = false;
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void ButtonAddAsset_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                AssetWindow newAssetWindow = new AssetWindow(MediaContextHelper.Instance);
                newAssetWindow.AssetCreated += newAssetWindow_AssetCreated;
                newAssetWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
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

        private void GoToAssetPage(Asset asset)
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
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
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
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                //Loaded -= Home_Loaded;
                //MediaContextHelper.AssetList = null;
                //AssetList = null;

            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                Loaded -= Home_Loaded;
                //MediaContextHelper.AssetList = null;
                //AssetList = null;

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
                    var source = frame.Source;
                    string serviceParam = source.ToString().Split('#')[1];
                    string[] serviceValues = serviceParam.Split(';');
                    accName = serviceValues[0];
                    accKey = serviceValues[1];
                    if (!string.IsNullOrWhiteSpace(accName) &&
                        string.CompareOrdinal(accName, MediaContextHelper.accountName) != 0)
                    {
                        MediaContextHelper.accountName = accName;
                        MediaContextHelper.accountKey = accKey;
                        MediaContextHelper.UpdateInstance();
                        AssetList = null;
                    }
                    Loaded += Home_Loaded;
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                Loaded -= Home_Loaded;
                //MediaContextHelper.AssetList = null;
                //AssetList = null;

            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listWorker != null) listWorker.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
