using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.ModernUI.Common;
using System.Configuration;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Ais.Internal.Dcm.ModernUI
{
    public class ServiceHelper: INotifyPropertyChanged
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

        private MobileDataService service;
        private string mobileTableName = string.Empty;
        private string mobileApplicationKey = string.Empty;
        private string mobileUrl = string.Empty;

        public ServiceHelper()
        {
            //MediaServiceList = new ObservableCollection<MediaService>
            //    {
            //        new MediaService { AccountName = "aismediaservice3", AccountKey = "LrOXhQRf/0R8X1TRHMMkLVtEMJsJtXV7MbRNi90ZiFY=" },
            //        new MediaService { AccountName = "aisdemomodernui", AccountKey = "A72Gb+BUnGKQDlO2ptqaZpHMyi2oOEKrbVKgLOBFmc0="},
            //        new MediaService { AccountName = "aisdemomediaservice", AccountKey = "byfwSL991NeVHc5De1gs7HLbXiTZk+caY49dgPPGXSA="},
            //        new MediaService { AccountName = "uxmediaservice", AccountKey = "eTQLzz9n4vSDkGKDplMMH4pHjfSj/R+oI6bDEyWSH1I="}
            //    };
        }

        public void ListMediaServices()
        {
            try
            {
                if (GetMobileSettings())
                {
                    if (!string.IsNullOrWhiteSpace(mobileApplicationKey) && !string.IsNullOrWhiteSpace(mobileUrl))
                    {
                        service = new MobileDataService(mobileApplicationKey, mobileUrl);
                        if (!string.IsNullOrWhiteSpace(mobileTableName))
                        {
                            var v = service.ListMediaServices(mobileTableName);
                            MediaServiceList = new ObservableCollection<MediaServiceInfo>(v);
                        }
                    }
                    else
                    {
                        MediaServiceList = new ObservableCollection<MediaServiceInfo>();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                UIHelper.ShowMessage(ex.Message, "Error", MessageBoxButton.OK);
                //UIHelper.HandlerException(ex);
            }
        }

        private bool GetMobileSettings()
        {
            try
            {
                AppSettingsReader rdr = new AppSettingsReader();
                mobileApplicationKey = (string) rdr.GetValue("MobileApplicationKey", typeof (string));
                mobileUrl = (string) rdr.GetValue("MobileServiceUrl", typeof (string));
                mobileTableName = (string) rdr.GetValue("MobileServiceTableName", typeof (string));
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                UIHelper.HandlerException(ex);
            }
            return false;
        }
    }

    public class MediaContextHelper
    {
        public static string accountName = string.Empty;
        public static string accountKey = string.Empty;

        private static string _accountName = string.Empty;
        private static string _accountKey = string.Empty;

        private static MediaServiceContext instance;
        public static MediaServiceContext Instance
        {
            get
            {
                if (instance == null)
                {
                    //if (CheckForSettings())
                    if(!string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountKey))
                    {
                        if (String.CompareOrdinal(_accountName, accountName) != 0)
                        {
                            _accountName = accountName;
                            _accountKey = accountKey;
                            instance = new MediaServiceContext(accountName, accountKey);
                        }

                    }
                }
                return instance;
            }
        }

        public static void UpdateInstance()
        {
            if (!string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountKey))
            {
                if (String.CompareOrdinal(_accountName, accountName) != 0)
                {
                    _accountName = accountName;
                    _accountKey = accountKey;
                    instance = new MediaServiceContext(accountName, accountKey);
                    InitializeValues();
                }
            }
        }

        private static bool CheckForSettings()
        {
            try
            {
                AppSettingsReader rdr = new AppSettingsReader();
                accountName = (string) rdr.GetValue("AccountName", typeof(string));
                accountKey = (string)rdr.GetValue("AccountKey", typeof(string));
                return true;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
            return false;
        }

        private static void InitializeValues()
        {
            SelectedAsset = null;
            AssetList = null;
            AssetFileList = null;
            ThumbnailUrls = null;
            OutputUrls = null;
        }

        public static Asset SelectedAsset { get; set; }

        /// <summary>
        /// Check for files exists for an asset for job assignment
        /// </summary>
        public static bool IsJobEnabled
        {
            get;
            set;
        }

        public static string SelectedAssetType { get; set; }

        public static List<Asset> AssetList { get; set; } 
        public static List<AssetFile> AssetFileList { get; set; }
        public static List<string> ThumbnailUrls { get; set; }
        public static List<AssetWithFiles> OutputUrls { get; set; }
    }
}