using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Ais.Internal.Dcm.ModernUIV2
{
    /// <summary>
    /// Interaction logic for AssetWindow.xaml
    /// </summary>
    public partial class AssetWindow : Window, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
       
        private string _waitMessage;
        public string WaitMessage
        {
            get { return _waitMessage; }
            set { _waitMessage = value; OnPropertyChanged("WaitMessage"); }
        }

        public event EventHandler AssetCreated;
        protected void OnAssetCreated()
        {
            if (AssetCreated != null)
            {
                AssetCreated(this, new AssetEventArgs(Asset));
            }
        }

        HttpClient Client = new HttpClient();

        private string _mediaServiceName;
        private AssetInfo Asset { get; set; }
        private bool okEnabled;
        public bool OkEnabled { get { return okEnabled; } set { okEnabled = value; OnPropertyChanged("OkEnabled"); } }

        public AssetWindow(HttpClient client, string mediaServiceName)
        {
            try
            {
                InitializeComponent();
                this.Client = client;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                _mediaServiceName = mediaServiceName; OkEnabled = true;
                this.Loaded += AssetWindow_Loaded;
                //this.CloseButton.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void AssetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                txtAssetName.Focus();
                DataContext = this;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private async void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtAssetName.Text) && IsValidAssetName(txtAssetName.Text))
                {
                    OkEnabled = false;
                    LoadingAssetsGrid.Visibility = Visibility.Visible;
                    WaitMessage = Literals.MESSAGE_INFO_CREATING_ASSET;// 
                    //SaveGrid.Visibility = Visibility.Collapsed;
                    this.LoadingAssetsGrid.Visibility = Visibility.Visible;
                    string assetName = txtAssetName.Text.Trim();

                    Asset = await CreateAsset(assetName);

                    if (Asset != null)
                    {
                        OnAssetCreated();
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                    new Action(
                                                                        () =>
                                                                        {
                                                                            this.LoadingAssetsGrid.Visibility =
                                                                                Visibility.Collapsed;
                                                                            //SaveGrid.Visibility = Visibility.Visible;
                                                                            OkEnabled = true;
                                                                            this.Close();
                                                                        }));



                    }
                }
                else
                {
                    ModernDialog.ShowMessage("Invalid Asset Name. Only Alphanumeric names are accepted.", "Error",
                                                 MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private bool IsValidAssetName(string p)
        {
           return  Regex.IsMatch(p, "^[a-zA-Z0-9_]+$");
        }

        private async Task<AssetInfo> CreateAsset(string assetName)
        {

            AssetInfo asset;
            string uri = string.Format(Literals.URL_CREATE_ASSET,
                                       _mediaServiceName, assetName);
            var response = await this.Client.PostAsync(uri, new StringContent(assetName));
            response.EnsureSuccessStatusCode(); // Throw on error code.

            var responseAsset = response.Content.ReadAsAsync<AssetInfo>().Result;
            asset = responseAsset;
            return asset;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Client != null) Client.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                UIHelper.HandlerException(exception);
            }
        }
    }

    public class AssetEventArgs : EventArgs
    {
        public AssetInfo Asset { get; private set; }

        public AssetEventArgs(AssetInfo asset)
        {
            Asset = asset;
        }
    }
}
