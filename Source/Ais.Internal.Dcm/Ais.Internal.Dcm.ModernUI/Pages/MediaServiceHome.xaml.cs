using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.ModernUI.Common;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ais.Internal.Dcm.ModernUI.Pages
{
    /// <summary>
    /// Interaction logic for MediaServiceHome.xaml
    /// </summary>
    public partial class MediaServiceHome : Page, IDisposable
    {
        public ServiceHelper serviceHelper;
        private BackgroundWorker reloadWorker = new BackgroundWorker();

        public MediaServiceHome()
        {
            InitializeComponent();
            serviceHelper = new ServiceHelper();
            reloadWorker.DoWork += reloadWorker_DoWork;
            reloadWorker.RunWorkerCompleted += reloadWorker_RunWorkerCompleted;
            reloadWorker.RunWorkerAsync();
        }

        void reloadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DataContext = serviceHelper;
            LoadingMediaGrid.Visibility = Visibility.Collapsed;
            MediaDataGrid.Visibility = Visibility.Visible;
        }

        void reloadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReloadServices();
        }

        private void Reload_Services(object sender, RoutedEventArgs e)
        {
            LoadingMediaGrid.Visibility = Visibility.Visible;
            MediaDataGrid.Visibility = Visibility.Collapsed;
            reloadWorker.RunWorkerAsync();
        }

        private void ReloadServices()
        {
            serviceHelper.ListMediaServices();
        }

        private void GoToMediaPage(MediaServiceInfo service)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, this);
                App.SelectedFrame = frame;
                if (frame != null && service != null)
                {
                    var param = string.Format("{0};{1}", service.AccountName, service.PrimaryAccountKey);
                    frame.Source = new Uri(string.Format("../Pages/Home.xaml#{0}", param),
                                           UriKind.Relative);
                    frame.KeepContentAlive = false;
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void ListBoxItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var media = serviceHelper.SelectedMediaService;
            GoToMediaPage(media);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (reloadWorker != null) reloadWorker.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
