using System.Windows.Input;
using Ais.Internal.Dcm.ModernUIV2.Common;
using Ais.Internal.Dcm.ModernUIV2.ViewModels;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using NLog;
using FirstFloor.ModernUI.Windows.Controls;
using Ais.Internal.Dcm.ModernUIV2.Controls;

namespace Ais.Internal.Dcm.ModernUIV2.Tabs
{
    /// <summary>
    /// Interaction logic for AssetFiles.xaml
    /// </summary>
    public partial class AssetFiles : UserControl, IContent, INotifyPropertyChanged, IDisposable
    {
        #region Constructor

        public AssetFiles()
        {
            InitializeComponent();
            client = App.GetHttpClient();
            MediaContextHelper.FileUploader.OnUploadStatusChange += uploadHelper_OnUploadStatusChange;

        }

        #endregion

        #region Properties

       // private UploadHelper uploadHelper = new UploadHelper();
        private string selectedAssetId { get; set; }
        private string filePath = string.Empty;
        private HttpClient client = new HttpClient();
        private bool CanNavigate { get; set; }
        private Logger logger = LogManager.GetCurrentClassLogger();
        private bool IsFileLocal { get; set; }
        public string AssetName { get; set; }

        private AssetInfo selectedAsset;

        public AssetInfo SelectedAsset
        {
            get { return selectedAsset; }
            set
            {
                selectedAsset = value;
                OnPropertyChanged("SelectedAsset");
            }
        }

        private AssetFileInfo selectedAssetFile;

        public AssetFileInfo SelectedAssetFile
        {
            get { return selectedAssetFile; }
            set
            {
                selectedAssetFile = value;
                OnPropertyChanged("SelectedAssetFile");
            }
        }

        private string localFilePath;

        public string LocalFilePath
        {
            get { return localFilePath; }
            set
            {
                localFilePath = value;
                OnPropertyChanged("LocalFilePath");
                CheckForEncodingOrUpload(localFilePath);
            }
        }

        private string uploadStatus;

        public string UploadStatus
        {
            get { return uploadStatus; }
            set
            {
                uploadStatus = value;
                OnPropertyChanged("UploadStatus");
            }
        }

        private ObservableCollection<AssetFileInfo> assetFileList;

        public ObservableCollection<AssetFileInfo> AssetFileList
        {
            get { return assetFileList; }
            set
            {
                assetFileList = value;
                OnPropertyChanged("AssetFileList");
            }
        }

        private ObservableCollection<EncodingTypeModel> encodingTypes;

        public ObservableCollection<EncodingTypeModel> EncodingTypes
        {
            get { return encodingTypes; }
            set
            {
                encodingTypes = value;
                OnPropertyChanged("EncodingTypes");
            }
        }

        private EncodingTypeModel selectedEncodingType;

        public EncodingTypeModel SelectedEncodingType
        {
            get { return selectedEncodingType; }
            set
            {
                selectedEncodingType = value;
                OnPropertyChanged("SelectedEncodingType");
            }
        }

        private List<string> imageQualityList;

        public List<string> ImageQualityList
        {
            get { return imageQualityList; }
            set
            {
                imageQualityList = value;
                OnPropertyChanged("ImageQualityList");
            }
        }

        private string selectedImageQuality;

        public string SelectedImageQuality
        {
            get { return selectedImageQuality; }
            set
            {
                selectedImageQuality = value;
                OnPropertyChanged("SelectedImageQuality");
            }
        }

        private List<string> imageSizeList;

        public List<string> ImageSizeList
        {
            get { return imageSizeList; }
            set
            {
                imageSizeList = value;
                OnPropertyChanged("ImageSizeList");
            }
        }

        private string selectedImageSize;

        public string SelectedImageSize
        {
            get { return selectedImageSize; }
            set
            {
                selectedImageSize = value;
                OnPropertyChanged("SelectedImageSize");
            }
        }

        private int imageDurationInSeconds;

        public int ImageDurationInSeconds
        {
            get { return imageDurationInSeconds; }
            set
            {
                imageDurationInSeconds = value;
                OnPropertyChanged("ImageDurationInSeconds");
            }
        }

        private int maxNoOfThumbnails;

        public int MaxNoOfThumbnails
        {
            get { return maxNoOfThumbnails; }
            set
            {
                maxNoOfThumbnails = value;
                OnPropertyChanged("MaxNoOfThumbnails");
            }
        }

        private bool thumbnailJobSelected;

        public bool ThumbnailJobSelected
        {
            get { return thumbnailJobSelected; }
            set
            {
                thumbnailJobSelected = value;
                OnPropertyChanged("ThumbnailJobSelected");
            }
        }

        private bool thumbnailJobNotSelected;

        public bool ThumbnailJobNotSelected
        {
            get { return thumbnailJobNotSelected; }
            set
            {
                thumbnailJobNotSelected = value;
                OnPropertyChanged("ThumbnailJobNotSelected");
            }
        }

        private string _waitMessage;

        public string WaitMessage
        {
            get { return _waitMessage; }

            set
            {
                _waitMessage = value;
                OnPropertyChanged("WaitMessage");
            }
        }

        private bool processEnabled;

        public bool ProcessEnabled
        {
            get { return processEnabled; }
            set
            {
                processEnabled = value;
                OnPropertyChanged("ProcessEnabled");
            }
        }

        private ObservableCollection<UploadStatusViewModel> uploadList;

        public ObservableCollection<UploadStatusViewModel> UploadList
        {
            get { return uploadList; }
            set
            {
                uploadList = value;
                OnPropertyChanged("UploadList");
            }
        }

        private ObservableCollection<Tag> tagList;

        public ObservableCollection<Tag> TagList
        {
            get { return tagList; }
            set
            {
                tagList = value;
                OnPropertyChanged("TagList");
            }
        }

        private ObservableCollection<string> tagSource;

        public ObservableCollection<string> TagSource
        {
            get { return tagSource; }
            set
            {
                tagSource = value;
                OnPropertyChanged("TagSource");
            }
        }

        #endregion

        #region Event Handlers

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Navigation Methods
        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                Loaded -= AssetFiles_Loaded;
            }
            catch (Exception exp)
            {
                logger.LogException(LogLevel.Error, exp.Message, exp);
                UIHelper.HandlerException(exp);
            }
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                Loaded -= AssetFiles_Loaded;
            }
            catch (Exception exp)
            {
                logger.LogException(LogLevel.Error, exp.Message, exp);
                UIHelper.HandlerException(exp);
            }
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, (FrameworkElement) this.Parent);
                if (frame != null)
                {
                    var source = frame.Source;
                    selectedAssetId = source.ToString().Split('#')[1];
                    SelectedAsset = MediaContextHelper.SelectedAsset;
                    this.TagList = new ObservableCollection<Tag>();
                    this.LocalFilePath = string.Empty;
                    if (MediaContextHelper.AssetFileList != null && MediaContextHelper.AssetFileList.Count > 0)
                    {
                        if (AssetFileList == null)
                        {
                            AssetFileList = new ObservableCollection<AssetFileInfo>(MediaContextHelper.AssetFileList);
                            this.DataContext = this;
                        }
                        Loaded -= AssetFiles_Loaded; // no load required
                    }
                    else
                    {
                        Loaded += AssetFiles_Loaded;
                    }
                }
            }
            catch (Exception exp)
            {
                logger.LogException(LogLevel.Error, exp.Message, exp);
                UIHelper.HandlerException(exp);
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                //if (CanNavigate == false)
                //{
                //    MessageBoxResult result =
                //        ModernDialog.ShowMessage("Uploading and Encoding of a file is in process? ", "Warning",
                //                                 MessageBoxButton.YesNo);
                //    if (result == MessageBoxResult.Yes)
                //    {
                //        Loaded -= AssetFiles_Loaded;
                //    }
                //    if (result != MessageBoxResult.Yes)
                //    {
                //        e.Cancel = true;
                //    }
                //}
                //else
                //{
                    Loaded -= AssetFiles_Loaded;
                //}
                UploadList = null;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }
        #endregion

        #region Private Methods
        private void uploadHelper_OnUploadStatusChange(object sender, UploadFileEventArgs e)
        {
            try
            {
                List<UploadStatusViewModel> list = new List<UploadStatusViewModel>();
                foreach (var item in e.FileStatus)
                {
                    if (item.AssetId == MediaContextHelper.SelectedAsset.Id)
                    {
                        list.Add(new UploadStatusViewModel { FileName = item.FileName, Percentage = item.UploadPercentage });
                    }
                    //update the percentage
                }
                if (!list.Exists(p => p.Percentage != 100))
                {
                    ToggleEnableVisibility(true);
                }
                UploadList = new ObservableCollection<UploadStatusViewModel>(list);
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_LOADING_ASSETS_PROPERTIES;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private async void AssetFiles_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessEnabled = false;
                CanNavigate = true;
                LoadImageCustomizations();
                await LoadTags();
                this.TagList = new ObservableCollection<Tag>();
                //TagBox.EntryChanged += TagBox_EntryChanged;
                var encodingList = await FetchEncodingTypes();
                EncodingTypes = new ObservableCollection<EncodingTypeModel>(encodingList);
                //uploadHelper.OnUploadStatusChange += uploadHelper_OnUploadStatusChange;
                UploadList = new ObservableCollection<UploadStatusViewModel>();
               await Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new Action(
                        () =>
                        {
                            this.DataContext = this;
                            //ProcessEnabled = true;
                            loading.Visibility = Visibility.Collapsed;
                        }));
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private void CheckForEncodingOrUpload(string localFilePath)
        {
            if (!string.IsNullOrWhiteSpace(localFilePath))
            {
                this.ProcessEnabled = true;
            }
        }

        private async Task LoadTags()
        {
            var tags = new List<Tag>();
            try
            {
                string uri = string.Format(Literals.URL_GET_TAGS,
                                           MediaContextHelper.accountName);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<IEnumerable<Tag>>().Result;
                tags = responseAsset as List<Tag>;
                TagSource = new ObservableCollection<string>(tags.Select(t => t.Name));
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
        }

        private void LoadImageCustomizations()
        {
            try
            {
                ThumbnailJobSelected = false;
                this.MaxNoOfThumbnails = 5;
                var imgQualityList = new List<string> { "High", "Medium", "Low" };
                this.ImageQualityList = new List<string>(imgQualityList);
                this.selectedImageQuality = "High";

                var imgSizeList = new List<string> { "320x240", "480x320", "640x480", "800x600" };
                this.ImageSizeList = new List<string>(imgSizeList);
                this.SelectedImageSize = "640x480";
                this.ImageDurationInSeconds = 20;
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_THUMBNAIL_SETTINGS;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
        }

        private async Task<List<EncodingTypeModel>> FetchEncodingTypes()
        {
            var encodingTypes = new List<EncodingTypeModel>();
            try
            {
                string uri = string.Format(Literals.URL_GET_ENCODING_TYPE,
                                           MediaContextHelper.accountName);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<IEnumerable<EncodingTypeModel>>().Result;
                encodingTypes = responseAsset as List<EncodingTypeModel>;
                return encodingTypes;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
                //UIHelper.HandlerException(ex);
            }
            return encodingTypes;
        }

        private void LocalUpload_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog
                    {
                        FileName = "",
                        DefaultExt = ".mp4",
                        Filter =
                            Literals.SETTINGS_MEDIA_FILE_TYPES,
                        Multiselect = true
                    };

                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    // Open document 
                    filePath = dlg.FileName;
                    LocalFilePath = filePath;
                    IsFileLocal = true;
                    
                    //UploadFileToAsset(filePath);
                }
            }
            catch (Exception exception)
            {
                var message = Literals.MESSAGE_ERROR_THUMBNAIL_SETTINGS;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                //UIHelper.HandlerException(ex);
            }
        }

        private void CloudUpload_Click(object sender, RoutedEventArgs e)
        {
            var cloudWindow = new CloudUploadDialog(selectedAssetId);
            cloudWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            cloudWindow.AssetFileSelected += cloudWindow_AssetFileSelected;
            cloudWindow.ShowDialog();

            
        }

        private void cloudWindow_AssetFileSelected(object sender, EventArgs e)
        {
            try
            {
                IsFileLocal = false;
                SelectedAssetFile = (e as AssetFileEventArgs).AssetFile;
                LocalFilePath = SelectedAssetFile.Name;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, Literals.MESSAGE_ERROR_LOAD_CLOUD_FILE, exception);
                UIHelper.HandlerException(exception);
            }
        }

        private async void ProcessJob_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedEnFriendlyNames = string.Empty;
                var selectedEncodings = SelectedEncodings(ref selectedEnFriendlyNames);
                bool isPrimary = false;
                string uploadSasUrl = string.Empty;
                string fileName = string.Empty;
                bool jobAlreadyExists = false;

                if (IsFileLocal)
                {
                    CanNavigate = false;
                    //selected encoding is empty or no thumbnail job is created
                    //if ((string.IsNullOrWhiteSpace(selectedEncodings) && !ThumbnailJobSelected))
                    //{
                    //    MessageBoxResult upload =
                    //        ModernDialog.ShowMessage(
                    //            Literals.MESSAGE_WARNING_ONLY_UPLOAD,
                    //            Literals.MESSAGE_HEADER_UPLOAD_CONFIRMATION, MessageBoxButton.YesNo);
                    //    if (upload == MessageBoxResult.No)
                    //    {
                    //        CanNavigate = true;
                    //        return;
                    //    }
                    //}
                    jobAlreadyExists = await IsAJobPending(selectedAssetId);
                    if (jobAlreadyExists)
                    {
                        MessageBoxResult upload =
                            ModernDialog.ShowMessage(
                                Literals.MESSAGE_WARNING_ONLY_UPLOAD,
                                Literals.MESSAGE_HEADER_UPLOAD_CONFIRMATION, MessageBoxButton.YesNo);
                        if (upload == MessageBoxResult.No)
                        {
                            CanNavigate = true;
                            return;
                        }
                    }

                    await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                               new Action(
                                                                   () =>
                                                                   {
                                                                       loading.Visibility = Visibility.Visible;
                                                                       SelectFileGrid.Visibility =
                                                                           Visibility.Collapsed;
                                                                       ProcessEnabled = false;
                                                                   }));


                    WaitMessage = Literals.MESSAGE_STATUS_UPLOAD_CHECKING_FILE;// "Checking file";

                    uploadSasUrl = await GetUploadUrl();

                    if (!string.IsNullOrWhiteSpace(uploadSasUrl))
                    {
                        fileName = System.IO.Path.GetFileName(filePath);
                        WaitMessage = Literals.MESSAGE_STATUS_UPLOAD_CHECKING_FILE; 
                        string sasUrl = BuildSasUrl(uploadSasUrl, fileName);
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                                                                   new Action(
                                                                       () =>
                                                                       {
                                                                           WaitMessage = Literals.MESSAGE_STATUS_UPLOAD_UPLOADING_FILE; // Uploading file....
                                                                       }));

                        await Task.Run(() => MediaContextHelper.FileUploader.UploadFile(MediaContextHelper.SelectedAsset.Id, filePath, uploadSasUrl)).ContinueWith(
                            delegate { CanNavigate = true; });

                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send,
                                                                   new Action(
                                                                       () =>
                                                                       {
                                                                           WaitMessage = Literals.MESSAGE_STATUS_UPLOAD_UPLOADED_FILE; // 
                                                                       }));
                        bool linkSuccess = await LinkFileToAsset();
                        if (linkSuccess)
                        {
                            await CreateUpdateTagsForAsset();
                            if (!string.IsNullOrWhiteSpace(selectedEncodings) || ThumbnailJobSelected)
                            {
                                var assetFiles = await GetFilesInAsset();
                                if (assetFiles != null)
                                {
                                    var files = assetFiles.Where(file => file.Name == Path.GetFileName(fileName));
                                    var maxDate = files.Max(f => f.Created);
                                    var assetFile = assetFiles.FirstOrDefault(f => f.Created == maxDate);
                                    if (assetFile != null)
                                    {
                                        isPrimary = await MakeFileAsPrimary(assetFile.Id, ThumbnailJobSelected);
                                        await CreateUpdateTagsForAssetFile(assetFile.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                               new Action(
                                                                   () =>
                                                                   {
                                                                       loading.Visibility = Visibility.Visible;
                                                                       SelectFileGrid.Visibility =
                                                                           Visibility.Collapsed;
                                                                       ProcessEnabled = false;
                                                                   }));
                    WaitMessage = "Checking status";
                    jobAlreadyExists = await IsAJobPending(selectedAssetId);
                    if (jobAlreadyExists)
                    {
                        await Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                   new Action(
                                                                       () =>
                                                                       {
                                                                           loading.Visibility = Visibility.Collapsed;
                                                                           SelectFileGrid.Visibility =
                                                                               Visibility.Visible;
                                                                           ProcessEnabled = true;
                                                                       }));
                        ModernDialog.ShowMessage(Literals.MESSAGE_WARNING_ALREADY_JOB_RUNNING,
                                                 Literals.MESSAGE_HEADER_SERVICE_INFO,
                                                 MessageBoxButton.OK);

                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(selectedEncodings) || ThumbnailJobSelected)
                    {

                        isPrimary = await MakeFileAsPrimary(SelectedAssetFile.Id, ThumbnailJobSelected);
                    }
                    //TODO: Show message encoding should be selected
                    else
                    {
                        ModernDialog.ShowMessage(Literals.MESSAGE_WARNING_MUST_SELECT_THUMBNAIL_SETTING, 
                            Literals.MESSAGE_HEADER_UPLOAD_CONFIRMATION,
                                                 MessageBoxButton.OK);
                        ToggleEnableVisibility(true);
                        return;
                    }
                }

                if (jobAlreadyExists) return;
                //Initiate Job
                if (ThumbnailJobSelected)
                {
                    WaitMessage = Literals.MESSAGE_STATUS_UPLOAD_GENERATING_THUMBNAIL;;

                    await InitiateThumbnailJob();
                }

                if (!string.IsNullOrWhiteSpace(selectedEncodings))
                {
                    WaitMessage = Literals.MESSAGE_STATUS_UPLOAD_PROCESSING_ENCODING;

                    await InitiateJob(selectedEncodings, selectedEnFriendlyNames);
                    WaitMessage =  Literals.MESSAGE_STATUS_UPLOAD_COMPLETED_ENCODING;
                }
                await LoadTags();
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_ENCODING_ERROR;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                //UIHelper.HandlerException(ex);
            }
        }

        private async Task CreateUpdateTagsForAsset()
        {
            try
            {
                var tags = PrepareTagStringToUpload();
                string uri =
                    string.Format(Literals.URL_UPDATE_ASSET_TAG,
                                  MediaContextHelper.accountName, selectedAssetId, tags);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_UPDATING_TAG;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                throw;
            }
        }

        private string PrepareTagStringToUpload()
        {
            try
            {
                string tags = TagList.Aggregate(string.Empty, (current, tag) => current + (tag.Name + ","));
                //string tags = TextTags.Text.Trim();
                if (!string.IsNullOrWhiteSpace(tags))
                {
                    var lastIndex = tags.LastIndexOf(",", System.StringComparison.Ordinal);
                    if (lastIndex > 0 && lastIndex == tags.Length - 1)
                        tags = tags.Remove(tags.LastIndexOf(",", System.StringComparison.Ordinal));
                }
                return tags;
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_UPDATING_TAG;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
                throw;
            }
        }

        private async Task CreateUpdateTagsForAssetFile(string assetFileId)
        {
            var tags = PrepareTagStringToUpload();
            string uri =
                string.Format(
                    Literals.URL_UPDATE_FILE_TAG,
                    MediaContextHelper.accountName, selectedAssetId, assetFileId, tags);
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode(); // Throw on error code.
        }

        private async Task<bool> IsAJobPending(string selectedAssetId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(selectedAssetId))
                {
                    string uri = string.Format(
                        Literals.URL_GET_JOB_STATUS,
                        MediaContextHelper.accountName, selectedAssetId);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode(); // Throw on error code.

                    var responseAsset = response.Content.ReadAsAsync<bool>().Result;

                    ToggleEnableVisibility(true);

                    return responseAsset;
                }
            }
            catch (Exception exception)
            {
                string message = Literals.MESSAGE_ERROR_JOB_STATUS;
                logger.LogException(LogLevel.Error, message, exception);
                UIHelper.ShowMessage(message, Literals.MESSAGE_HEADER_ERROR, MessageBoxButton.OK);
            }
            return false;
        }

        private string SelectedEncodings(ref string selectedEnFriendlyNames)
        {
            string selectedEncodings = string.Empty;
            try
            {
                foreach (var encoding in EncodingTypes)
                {
                    if (encoding.IsSelected)
                    {
                        selectedEncodings = selectedEncodings + encoding.TechnicalName + ",";
                        selectedEnFriendlyNames = selectedEnFriendlyNames + encoding.FriendlyName + ",";
                    }
                }
                selectedEncodings = !string.IsNullOrWhiteSpace(selectedEncodings)
                                        ? selectedEncodings.Remove(selectedEncodings.LastIndexOf(","))
                                        : string.Empty;
                selectedEnFriendlyNames = !string.IsNullOrWhiteSpace(selectedEnFriendlyNames)
                                              ? selectedEnFriendlyNames.Remove(selectedEnFriendlyNames.LastIndexOf(","))
                                              : string.Empty;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return selectedEncodings;
        }

        private async Task<string> InitiateThumbnailJob()
        {
            try
            {
                var maxNoOfThumnails = MaxNoOfThumbnails;
                var imgQuality = SelectedImageQuality;
                var imageSize = SelectedImageSize.Split('x');
                var height = imageSize[0];
                var width = imageSize[1];
                var imgDurInSec = ImageDurationInSeconds.ToString();
                var enStrings = imgQuality + "," + height + "," + width + ","
                                + maxNoOfThumnails + "," + imgDurInSec;
                string uri = string.Format(
                   Literals.URL_INIT_THUMB_JOB,
                    MediaContextHelper.accountName, selectedAssetId, enStrings);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<string>().Result;

                ToggleEnableVisibility(true);

                return responseAsset;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return string.Empty;
        }

        private async Task<string> InitiateJob(string selEncodings, string selEnFriendlyNames)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(selEncodings))
                {
                    string uri = string.Format(
                       Literals.URl_INIT_JOB,
                        MediaContextHelper.accountName, selectedAssetId, selEncodings, selEnFriendlyNames);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode(); // Throw on error code.

                    var responseAsset = response.Content.ReadAsAsync<string>().Result;

                    ToggleEnableVisibility(true);

                    return responseAsset;
                }
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return string.Empty;
        }

        private void ToggleEnableVisibility(bool visible)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                       new Action(
                                                           () =>
                                                           {
                                                               loading.Visibility = (visible
                                                                                         ? Visibility.Collapsed
                                                                                         : Visibility.Visible);
                                                               SelectFileGrid.Visibility = visible
                                                                                               ? Visibility.Visible
                                                                                               : Visibility
                                                                                                     .Collapsed;
                                                               ProcessEnabled = visible;
                                                           }));
        }

        private async Task<bool> LinkFileToAsset()
        {
            try
            {
                string uri = string.Format(Literals.URL_GENERATE_FILE_METADATA,
                                           MediaContextHelper.accountName, selectedAssetId);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<bool>().Result;

                return responseAsset;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return false;
        }

        private async Task<List<AssetFileInfo>> GetFilesInAsset()
        {
            try
            {
                string uri = string.Format(Literals.URL_ASSET_FILE_CONTEXT,
                                           MediaContextHelper.accountName, selectedAssetId);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<List<AssetFileInfo>>().Result;

                return responseAsset;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return null;
        }

        private async Task<bool> MakeFileAsPrimary(string assetFileId, bool isThumbnailJobSelected)
        {
            try
            {
                string uri = string.Format(
                   Literals.URL_MAKE_FILE_PRIMARY,
                    MediaContextHelper.accountName, selectedAssetId, assetFileId, isThumbnailJobSelected);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<bool>().Result;

                return responseAsset;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return false;
        }

        internal string BuildSasUrl(string locatorPath, string fileName)
        {
            try
            {
                string queryPart = new Uri(locatorPath).Query;
                string blobContainerUri = locatorPath.Substring(0, locatorPath.Length - queryPart.Length);
                string fileUrl = string.Format("{0}/{1}{2}", blobContainerUri, fileName, queryPart);
                return fileUrl;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return string.Empty;
        }

        private async Task<string> GetUploadUrl()
        {
            try
            {
                string uri = string.Format(Literals.URL_GET_SAS_URL_UPLOAD,
                                           MediaContextHelper.accountName, selectedAssetId);
                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode(); // Throw on error code.

                var responseAsset = response.Content.ReadAsAsync<string>().Result;

                return responseAsset;
            }
            catch (Exception exception)
            {
                logger.LogException(LogLevel.Error, exception.Message, exception);
            }
            return string.Empty;
        }

        private void AddTags_Click(object sender, RoutedEventArgs e)
        {
            var tags = TextTags.Text.Trim();
            if (!String.IsNullOrWhiteSpace(tags))
            {
                var tagStrings = tags.Split(',').ToList();
                foreach (var tag in tagStrings)
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        var tagItem = new Tag { Id = Guid.Empty.ToString(), Name = tag };
                        if (TagList.Count == 0 || TagList.Count(t => t.Name == tag) == 0)
                        {
                            this.TagList.Add(new Tag() { Id = Guid.Empty.ToString(), Name = tag });
                        }
                        else
                        {
                            if (TagList.Any(t => string.CompareOrdinal(t.Name, tag) == 0))
                                UIHelper.ShowMessage("Tag with the same name exists", "Information", MessageBoxButton.OK);
                        }
                    }
                }
                TextTags.Text = string.Empty;
            }
        }

        private void RemoveTag_Click(object sender, RoutedEventArgs e)
        {
            var tagId = (sender as Button).Tag.ToString();
            var tag = this.TagList.FirstOrDefault(t => t.Id == tagId);
            this.TagList.Remove(tag);
        } 
        #endregion

        #region Protected Methods
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (client != null) client.Dispose();
            }
        }
        #endregion

        #region Public Methods
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
