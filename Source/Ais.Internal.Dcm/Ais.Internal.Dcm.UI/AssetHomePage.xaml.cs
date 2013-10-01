using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.UI.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Ais.Internal.Dcm.UI
{
    /// <summary>
    /// Interaction logic for AssetHomePage.xaml
    /// </summary>
    public partial class AssetHomePage : Page,IDisposable
    {
        private BackgroundWorker listWorker= new BackgroundWorker();

        public string Status { get; set; }

        public AssetHome AssetHomeDataSource { get; set; }

        public AssetHomePage()
        {
            try
            {
                InitializeComponent();
                listWorker.WorkerReportsProgress = true;
                listWorker.WorkerSupportsCancellation = true;
                AssetHomeDataSource = new AssetHome();
                this.Loaded += AssetHomePage_Loaded;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void AssetHomePage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;

                listWorker.DoWork += listWorker_DoWork;
                listWorker.RunWorkerCompleted += listWorker_RunWorkerCompleted;
                if (listWorker.IsBusy != true)
                {
                    listWorker.RunWorkerAsync();
                }
            }
            catch (Exception wamsException)
            {
                UIHelper.HandlerException(wamsException);
            }
        }

        void listWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                var assets = e.Result as List<Asset>;
                if (assets != null)
                {
                    AssetHomeDataSource.AssetList = new ObservableCollection<Asset>(assets.OrderBy(ast => ast.Name));
                    AssetHomeDataSource.AssetSource = new CollectionView(AssetHomeDataSource.AssetList);
                    //AssetHomeDataSource.AssetSource =
                    //    (CollectionView)CollectionViewSource.GetDefaultView(AssetHomeDataSource.AssetList);
                    //AssetHomeDataSource.AssetSource.SortDescriptions.Add(new SortDescription("Name",
                    //                                                                         ListSortDirection.Ascending));
                    Status = string.Empty;

                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                               new Func<Visibility>(
                                                                   () => loading.Visibility = Visibility.Collapsed));

                    this.DataContext = AssetHomeDataSource;

                    if (AssetHomeDataSource.SelectedAsset == null && AssetHomeDataSource.AssetList != null && AssetHomeDataSource.AssetList.Count > 0)
                        AssetHomeDataSource.SelectedAsset = AssetHomeDataSource.AssetList[0];
                }
                listWorker.DoWork -= listWorker_DoWork;
                listWorker.RunWorkerCompleted -= listWorker_RunWorkerCompleted;
            }
            catch (Exception wamsException)
            {
                UIHelper.HandlerException(wamsException);
            }
        }

        void listWorker_DoWork(object sender, DoWorkEventArgs e)
        {
 	        FetchAssets(e);
        }

        private void FetchAssets(DoWorkEventArgs e)
        {
            try
            {
                Status = "Retrieving assets from media service.";
                //var assets = MediaContextHelper.Instance.GetAllAssets();
                var assets = MediaContextHelper.Instance.GetAllParentAssets();
                e.Result = assets;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (AssetHomeDataSource.SelectedAsset != null)
                {
                    NavigationService.Navigate(new ManageAssetPage(AssetHomeDataSource.SelectedAsset));
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (listWorker != null)
                {
                    listWorker.Dispose();
                }

            }
        }
    }

    public class AssetHome : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public CollectionView AssetSource;

        private ObservableCollection<Asset> assetList;
        public ObservableCollection<Asset> AssetList
        {
            get { return assetList; }
            set
            {
                assetList = value;
                OnPropertyChanged("AssetList");
            }
        }

        private Asset selectedAsset;
        public Asset SelectedAsset
        {
            get { return selectedAsset; }
            set
            {
                selectedAsset = value;
                OnPropertyChanged("SelectedAsset");
            }
        } 
    }

}
