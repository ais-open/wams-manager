using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ais.Internal.Dcm.ModernUIV2.Common
{
    public class ServiceHelper : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<MediaServiceInfo> mediaServiceList;
        public ObservableCollection<MediaServiceInfo> MediaServiceList
        {
            get { return mediaServiceList; }
            set
            {
                mediaServiceList = value;
                OnPropertyChanged("MediaServiceList");
            }
        }

        private MediaServiceInfo selectedMediaService;
        public MediaServiceInfo SelectedMediaService
        {
            get { return selectedMediaService; }
            set
            {
                selectedMediaService = value;
                OnPropertyChanged("SelectedMediaService");
            }
        }

        HttpClient client = new HttpClient();
        public ServiceHelper()
        {
            client = App.GetHttpClient();
        }

        public async void ListMediaServices(string clientKey)
        {
            try
            {
                var response = await client.GetAsync(string.Format(Literals.URL_LIST_MEDIASERVICE, clientKey));
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var medialist = await response.Content.ReadAsAsync<IEnumerable<MediaServiceInfo>>();
                MediaServiceList = new ObservableCollection<MediaServiceInfo>(medialist);
            }
            catch (Exception ex)
            {
                //Logger.WriteLog(ex);
                UIHelper.ShowMessage(ex.Message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null) client.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
