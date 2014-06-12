using System;
using System.Collections.Generic;
using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;

namespace Ais.Internal.Dcm.ModernUIV2
{
   
    public class MediaContextHelper
    {
        private static string _accountName = string.Empty;
        private static string _accountKey = string.Empty;
        
        
        static MediaContextHelper()
        {
            FileUploader = new UploadHelper();
        }
        public static string accountName = string.Empty;
        public static string accountKey = string.Empty;
        public static UploadHelper FileUploader;
        /// <summary>
        /// Check for files exists for an asset for job assignment
        /// </summary>
        public static bool IsJobEnabled
        {
            get;
            set;
        }

        public static string SelectedAssetType { get; set; }

        public static List<AssetInfo> AssetList { get; set; }
        public static List<AssetFileInfo> AssetFileList { get; set; }
        public static List<ThumbnailModel> ThumbnailUrls { get; set; }
        public static List<AssetWithFiles> OutputUrls { get; set; }
        public static List<GroupedOutputViewModel> GroupedOutputs { get; set; }
        

        public static void UpdateInstance()
        {
            if (!string.IsNullOrWhiteSpace(accountName) && !string.IsNullOrWhiteSpace(accountKey))
            {
                if (String.CompareOrdinal(_accountName, accountName) != 0)
                {
                    _accountName = accountName;
                    _accountKey = accountKey;
                    InitializeValues();
                }
            }
        }
        public static AssetInfo SelectedAsset { get; set; }

        private static void InitializeValues()
        {
            SelectedAsset = null;
            AssetList = null;
            AssetFileList = null;
            ThumbnailUrls = null;
            GroupedOutputs = null;
        }
        
    }
}