using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.DesignData;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace Ais.Internal.Dcm.ModernUIV2
{
    /// <summary>
    /// Interaction logic for AssetWindow.xaml
    /// </summary>
    public partial class ChooseThumbnailPopup : Window, INotifyPropertyChanged, IDisposable
    {
        private const string FACEBOOK_SHARE_URL_FORMAT = "https://www.facebook.com/sharer/sharer.php?u={0}";
        private const string TWITTER_SHARE_URL_FORMAT = "https://www.twitter.com/share?url={0}";

        private ThumbnailRollViewModel thumbnailList;
        public ThumbnailRollViewModel ThumbnailList
        {
            get { return thumbnailList; }
            set
            {
                thumbnailList = value; OnPropertyChanged("ThumbnailList");
            }
        }

        private string selectedThumbnailUrl;
        public string SelectedThumbnailUrl
        {
            get
            {
                return selectedThumbnailUrl;
            }
            set
            {
                selectedThumbnailUrl = value; OnPropertyChanged("SelectedThumbnailUrl");
            }
        }


        private string downloadURL;
        public string DownloadURL
        {
            get
            {
                return downloadURL;
            }
            set
            {
                downloadURL = value; OnPropertyChanged("DownloadURL");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
       
      
        public ChooseThumbnailPopup(string mediaUrl, ThumbnailRollViewModel thumbnails)
        {
            try
            {
                InitializeComponent();
                DownloadURL = mediaUrl;
                this.ThumbnailList = thumbnails;
                this.DataContext = this;
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
                DataContext = this;
               // imageRoll.ItemsSource = ImageUrls.Urls;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void OnNextClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.thumbnailList != null)
                {
                    this.thumbnailList.MoveToNextPage();
                }
            }
            catch (Exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }

        }

        private void OnPreviousClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.thumbnailList != null)
                {
                    this.thumbnailList.MoveToPreviousPage();
                }
            }
            catch (Exception)
            {
                string message = Literals.MESSAGE_ERROR_NAVIGATION;
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }

        }
       
       
        protected virtual void Dispose(bool disposing)
        {
          
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
                //logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lstBox = (ListBox)sender;
            var url = (string)lstBox.SelectedItem;
            if (!string.IsNullOrEmpty(url))
            {
                SelectedThumbnailUrl = url;
            }
        }

        private void CopyEmbedTagToClipBoard(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = txtEmbed.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetDataObject(text, true);
                }
            }
            catch (Exception exception)
            {
                UIHelper.HandlerException(exception);
            }
        }

        private void CopyDownloadURLToClipBoard(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = txtDownload.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetDataObject(text, true);
                }
            }
            catch (Exception exception)
            {
                UIHelper.HandlerException(exception);
            }
        }
        private void Facebook_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(string.Format(FACEBOOK_SHARE_URL_FORMAT, e.Uri.AbsoluteUri)));
                e.Handled = true;
            }
            catch (Exception exception)
            {
                UIHelper.HandlerException(exception);
            }

        }


        private void Twitter_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(string.Format(TWITTER_SHARE_URL_FORMAT, e.Uri.AbsoluteUri)));
                e.Handled = true;
            }
            catch (Exception exception)
            {
                UIHelper.HandlerException(exception);
            }

        }
    }

   
}
