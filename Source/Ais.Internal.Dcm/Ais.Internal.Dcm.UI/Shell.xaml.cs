using Ais.Internal.Dcm.Business;
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
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        private AssetWindow assetWindow;

        public Shell()
        {
            InitializeComponent();
            this.Loaded += (sender, args) =>
                {
                    this.WindowState = WindowState.Maximized;
                    //mainFrame.NavigationService.Navigate(new ManageAssetPage(new Asset(MediaContextHelper.Instance,"randomstring"){Name="New Asset"}));
                    mainFrame.NavigationService.Navigate(new AssetHomePage());
                };
        }

        private void NewAsset_OnClick(object sender, RoutedEventArgs e)
        {
            assetWindow = new AssetWindow(MediaContextHelper.Instance) { WindowStartupLocation = WindowStartupLocation.CenterScreen };            
            assetWindow.AssetCreated += (o, args) =>
            {
                var assetEventArgs = args as AssetEventArgs;
                if (assetEventArgs != null)
                {
                    var asset = assetEventArgs.Asset;
                    //show user dialog for managing asset
                    //var result = MessageBox.Show(string.Format("Do you want to manage this Asset - {0}?", asset.Name), "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    //if (result == MessageBoxResult.Yes)
                    //{
                        //Navigate to Manage Asset screen.
                        mainFrame.NavigationService.Navigate(new ManageAssetPage(asset));
                    //}
                    //this.AssetList.Add(asset);
                    //this.AssetList = new ObservableCollection<Asset>(AssetList.OrderBy(ast => ast.Name));
                }
            };

            var dialog = assetWindow.ShowDialog();

        }

        private void ExitMenu_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
