using Ais.Internal.Dcm.ModernUIV2.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using System.Net.Http;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;

namespace Ais.Internal.Dcm.ModernUIV2
{
    /// <summary>
    /// Interaction logic for AssetWindow.xaml
    /// </summary>
    public partial class URLWindow : Window, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public URLWindow()
        {
            try
            {
                InitializeComponent();
                this.Client = new HttpClient();
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                OkEnabled = true;
                this.Loaded += AssetWindow_Loaded;
                //this.CloseButton.Visibility = System.Windows.Visibility.Hidden;
                SettingHelper helper = new SettingHelper();
                this.txtAPIUrl.Text = helper.GetUrlFromSettingFile();
                string encodedCredential = helper.GetCredential();
                if (!string.IsNullOrWhiteSpace(encodedCredential))
                {
                    byte[] userNamePassword = Convert.FromBase64String(encodedCredential);
                    string utfString = UTF8Encoding.Default.GetString(userNamePassword);
                    string[] str = utfString.Split(new string[]{":"}, StringSplitOptions.None);
                    if (str != null && str.Length == 2)
                    {
                        this.txtUsername.Text = str[0];
                        this.txtPassword.Password = str[1];
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

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

        public event EventHandler URLChanged;
        protected void OnUrlChanged()
        {
            if (URLChanged != null)
            {
                URLChanged(this, new SettingsChangedEventArgs(this.txtAPIUrl.Text));
            }
        }

        HttpClient Client = new HttpClient();

        private AssetInfo Asset { get; set; }
        private bool okEnabled;
        public bool OkEnabled { get { return okEnabled; } set { okEnabled = value; OnPropertyChanged("OkEnabled"); } }

        private void AssetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.txtAPIUrl.Focus();
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
                if (!string.IsNullOrWhiteSpace(txtAPIUrl.Text))
                {
                    OkEnabled = false;
                    LoadingAssetsGrid.Visibility = Visibility.Visible;
                    WaitMessage = "Validating Url... please wait";
                    this.LoadingAssetsGrid.Visibility = Visibility.Visible;
                    string newUrl = txtAPIUrl.Text.Trim();
                    string userName = txtUsername.Text.Trim();
                    string password = txtPassword.Password.Trim();
                    SettingHelper helper = new SettingHelper();
                    bool isNewUrlValid = await helper.IsApiAccessible(newUrl,userName,password);
                    if (isNewUrlValid)
                    {
                        helper.SetNewUrlFromUser(newUrl);
                        helper.SetNewCredential(userName, password);
                        OnUrlChanged();
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                   new Action(
                                                                       () =>
                                                                       {
                                                                           this.LoadingAssetsGrid.Visibility =
                                                                               Visibility.Collapsed;
                                                                           OkEnabled = true;
                                                                           this.Close();
                                                                       }));
                    }
                    else
                    {
                        // invalid url show message or someting
                       await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                   new Action(
                                                                       () =>
                                                                           {
                                                                               this.LoadingAssetsGrid.Visibility =
                                                                                   Visibility.Collapsed;
                                                                               OkEnabled = true;
                                                                               WaitMessage = "Invalid Url";
                                                                           }));
                        ModernDialog.ShowMessage("Invalid Url, Username or password. Please contact admin for correct values.", "Error",
                                                 MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                           new Action(
                                                               () =>
                                                               {
                                                                   this.LoadingAssetsGrid.Visibility =
                                                                       Visibility.Collapsed;
                                                                   OkEnabled = true;
                                                                   WaitMessage = "Invalid Url";
                                                               }));
                ModernDialog.ShowMessage("Invalid Url, Username or password. Please contact admin for correct values.", "Error",
                                         MessageBoxButton.OK);
                UIHelper.HandlerException(ex);
            }
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

    public class SettingsChangedEventArgs : EventArgs
    {
        public string Url { get; private set; }

        public SettingsChangedEventArgs(string url)
        {
            this.Url = url;
        }
    }
}
