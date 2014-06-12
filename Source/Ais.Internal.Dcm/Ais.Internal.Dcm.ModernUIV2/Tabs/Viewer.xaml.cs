using FirstFloor.ModernUI.Windows;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Ais.Internal.Dcm.ModernUIV2.Tabs
{
    /// <summary>
    /// Interaction logic for Viewer.xaml
    /// </summary>
    public partial class Viewer : UserControl, IContent
    {
        #region Constructor
        public Viewer()
        {
            InitializeComponent();
        } 
        #endregion

        #region Navigation Methods
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {

        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MediaContextHelper.SelectedAssetType))
            {
                string curDir = Directory.GetCurrentDirectory();
                Uri uri = new Uri(string.Format("file:///{0}/smf.html", curDir));
                if (MediaContextHelper.SelectedAssetType == "SMOOTHSTREAMING")
                    uri = new Uri(string.Format("file:///{0}/smf.html", curDir));
                if (MediaContextHelper.SelectedAssetType == "MP4")
                    uri = new Uri(string.Format("file:///{0}/mp4.html", curDir));
                webBrowser.Navigate(uri);

                noPreviewTextBlock.Visibility = Visibility.Collapsed;
                webBrowser.Visibility = Visibility.Visible;
            }
            else
            {
                noPreviewTextBlock.Visibility = Visibility.Visible;
                webBrowser.Visibility = Visibility.Collapsed;
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            MediaContextHelper.SelectedAssetType = string.Empty;
            webBrowser.Navigate("about:blank");
        } 
        #endregion
    }
}
