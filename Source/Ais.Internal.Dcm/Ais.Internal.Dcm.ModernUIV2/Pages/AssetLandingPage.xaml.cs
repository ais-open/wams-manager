using System;
using System.Windows;
using System.Windows.Controls;
using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using System.ComponentModel;
using FirstFloor.ModernUI.Windows.Navigation;
using NLog;

namespace Ais.Internal.Dcm.ModernUIV2.Pages
{
    /// <summary>
    /// Interaction logic for AssetLandingPage.xaml
    /// </summary>
    public partial class AssetLandingPage : Page, INotifyPropertyChanged
    {
        #region Private member
        private Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public AssetLandingPage()
        {
            InitializeComponent();
            this.Loaded += AssetLandingPage_Loaded;
        }

        #region Bindable Properties
        private AssetInfo selectedAsset;
        /// <summary>
        /// Represents the selected Album/Asset
        /// </summary>
        public AssetInfo SelectedAsset
        {
            get
            {
                return selectedAsset;
            }
            set
            {
                selectedAsset = value;
                OnPropertyChanged("SelectedAsset");
            }
        }
        #endregion

        #region Event Handlers
        void AssetLandingPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedAsset = MediaContextHelper.SelectedAsset;
                this.DataContext = this;
                this.Loaded -= AssetLandingPage_Loaded;
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_MEDIA;//;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
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
                string message = Literals.MESSAGE_ERROR_ABOUT; //"Error while loading About  Window.";
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                logger.LogException(LogLevel.Error, message, exception);
            }
        }

        private void NavigateToAsset(object sender, RoutedEventArgs e)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                App.SelectedFrame = frame;
                if (frame != null && MediaContextHelper.accountName != null)
                {
                    // Media Service Name and Friendly Name is passed as 'param' to Home page.
                    var param = string.Format("{0};{1}", MediaContextHelper.accountName, MediaContextHelper.accountKey);
                    frame.Source = new Uri(string.Format("../Pages/Home.xaml#{0}", param),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION_ASSET_PAGE; //"Error while navigating to asset page.";
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
                    // Navigate to starting page where all collections are displayed
                    frame.Source = new Uri(string.Format("../Pages/MediaServiceHome.xaml"),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_MEDIA; //;
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
    }
}
