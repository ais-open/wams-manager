using Ais.Internal.Dcm.ModernUIV2.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using System.Windows.Navigation;
using System.Diagnostics;
using NLog;
using BitlyDotNET.Interfaces;
using BitlyDotNET.Implementations;

namespace Ais.Internal.Dcm.ModernUIV2
{
    /// <summary>
    /// Interaction logic for AssetWindow.xaml
    /// </summary>
    public partial class ShareViewWindow : Window,INotifyPropertyChanged
    {
        private const string FACEBOOK_SHARE_URL_FORMAT = "https://www.facebook.com/sharer/sharer.php?u={0}";
        private const string TWITTER_SHARE_URL_FORMAT = "https://www.twitter.com/share?url={0}";

        public event PropertyChangedEventHandler PropertyChanged;
        private Logger logger = LogManager.GetCurrentClassLogger();

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _selectedUrl;
        public string SelectedUrl
        {
            get
            {
                return _selectedUrl;
            }
            set
            {
                _selectedUrl = value;
                OnPropertyChanged("SelectedUrl");
            }
        }

        private bool _validUrl;
        public bool ValidUrl
        {
            get
            {
                return _validUrl;
            }
            set
            {
                _validUrl = value;
                OnPropertyChanged("ValidUrl");
            }
        }


        private List<VideoOutput> _ouputs;
        public List<VideoOutput> Outputs
        {
            get
            {
                return _ouputs;
            }
            set
            {
                _ouputs = value;
                OnPropertyChanged("Outputs");
            }
        }


        private string _thumbnailUrl;
        public string ThumbnailUrl
        {
            get
            {
                return _thumbnailUrl;
            }
            set
            {
                _thumbnailUrl = value;
                OnPropertyChanged("ThumbnailUrl");
            }
        }

        public ShareViewWindow()
        {
            
            InitializeComponent();
            this.DataContext = this;
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
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
           
        }
            
        private void Window_Deactivated(object sender, EventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comboControl = (ComboBox)sender;
                var item = (VideoOutput)comboControl.SelectedItem;
                SelectedUrl = item.Url;
                if (!string.IsNullOrEmpty(SelectedUrl))
                {
                    ValidUrl = true;
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
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
                logger.LogException(LogLevel.Error, exception.Message, exception);
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
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void Facebook_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                IBitlyService s = new BitlyService(Config.BitlyUsername, Config.BitlyKey);

                string shortened = s.Shorten(e.Uri.AbsoluteUri.ToString());
                if (shortened != null)
                {
                    Process.Start(new ProcessStartInfo(string.Format(FACEBOOK_SHARE_URL_FORMAT, shortened)));
                    e.Handled = true;
                }
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
                IBitlyService s = new BitlyService(Config.BitlyUsername, Config.BitlyKey);

                string shortened = s.Shorten(e.Uri.AbsoluteUri.ToString());
                if (shortened != null)
                {
                    // do something with "shortened"
                    Process.Start(new ProcessStartInfo(string.Format(TWITTER_SHARE_URL_FORMAT,shortened)));
                    e.Handled = true;
                }
            }
            catch (Exception exception)
            {
                UIHelper.HandlerException(exception);
            }

        }
    }
}
