using System.ComponentModel;
using System.Windows.Threading;
using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.UI.Common;
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
using System.Windows.Shapes;

namespace Ais.Internal.Dcm.UI
{
    /// <summary>
    /// Interaction logic for Asset.xaml
    /// </summary>
    public partial class AssetWindow : Window
    {
        public event EventHandler AssetCreated;
        protected void OnAssetCreated()
        {
            if(AssetCreated != null)
            {
                AssetCreated(this, new AssetEventArgs(Asset));
            }
        }

        private MediaServiceContext _context;
        private Asset Asset { get; set; }

        public AssetWindow(MediaServiceContext context)
        {
            try
            {
                InitializeComponent();
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                _context = context;
                this.Loaded += AssetWindow_Loaded;
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
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtAssetName.Text))
                {
                    var assetWorker = new BackgroundWorker();
                    loading.Visibility = Visibility.Visible;
                    string assetName = txtAssetName.Text.Trim();
                    assetWorker.DoWork += (o, args) => { args.Result = _context.CreateNewAsset(assetName); };
                    assetWorker.RunWorkerCompleted += (o, args) =>
                        {
                            Asset = args.Result as Asset;
                            if (Asset != null)
                            {
                                OnAssetCreated();
                                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                           new Func<Visibility>(
                                               () => loading.Visibility = Visibility.Collapsed));


                                this.Close();
                            }
                        };
                    assetWorker.RunWorkerAsync();
                    //Asset = _context.CreateNewAsset(txtAssetName.Text.Trim());
                    //if (Asset != null)
                    //{
                    //    OnAssetCreated();
                    //    loading.Visibility = Visibility.Collapsed;

                    //    this.Close();
                    //}
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }
    }

    public class AssetEventArgs : EventArgs
    {
        public Asset Asset { get; private set; }

        public AssetEventArgs(Asset asset)
        {
            Asset = asset;
        }
    }
}
