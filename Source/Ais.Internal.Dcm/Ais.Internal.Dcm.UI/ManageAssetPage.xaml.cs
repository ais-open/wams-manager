using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Configuration;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.UI.Common;
using Ais.Internal.Dcm.UI.ViewModels;
using Microsoft.Win32;
using System.ComponentModel;
using System.Timers;

namespace Ais.Internal.Dcm.UI
{
    /// <summary>
    /// Interaction logic for ManageAssetPage.xaml
    /// </summary>
    public partial class ManageAssetPage : Page, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Properties
        public Asset SelectedAsset { get; set; }
        public string AssetName { get; set; }
        private string filePath = string.Empty;
        private Job selectedJob = null;
        private AssetFile selectedAssetFile;

        public AssetFile SelectedAssetFile
        {
            get { return selectedAssetFile; }
            set
            {
                selectedAssetFile = value;
                OnPropertyChanged("SelectedAssetFile");
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

        private ObservableCollection<AssetFile> assetFileList;
        public ObservableCollection<AssetFile> AssetFileList
        {
            get { return assetFileList; }
            set
            {
                assetFileList = value; OnPropertyChanged("AssetFileList");
            }
        }

        public enum RadioButtons { ThumbnailRbtn, EncodeRbtn, None }
        public RadioButtons SelectedRadioButton { get; set; }

        private ObservableCollection<EncodingType> encodingTypes; 
        public ObservableCollection<EncodingType> EncodingTypes
        {
            get { return encodingTypes; }
            set { encodingTypes = value; OnPropertyChanged("EncodingTypes"); }
        }

        private EncodingType selectedType;
        public EncodingType SelectedType
        {
            get { return selectedType; }
            set
            {
                selectedType = value;
                OnPropertyChanged("SelectedType");
            }
        }

        private ObservableCollection<Thumnbnails> thumbnails;
        public ObservableCollection<Thumnbnails> Thumbnails
        {
            get { return thumbnails; }
            set { thumbnails = value;
                OnPropertyChanged("Thumbnails");
            }
        }

        private ObservableCollection<AssetWithFiles> outputSasUrls;
        public ObservableCollection<AssetWithFiles> OutputSasUrls
        {
            get { return outputSasUrls; }
            set
            {
                outputSasUrls = value;
                OnPropertyChanged("OutputSasUrls");
            }
        }

        private ObservableCollection<JobEntity> jobStatus;
        public ObservableCollection<JobEntity> JobStatus 
        {
            get { return jobStatus; }
            set
            {
                jobStatus = value;

                OnPropertyChanged("JobStatus");
            }
        } 

        private BackgroundWorker filesWorker = new BackgroundWorker();
        private BackgroundWorker thumbnailWorker = new BackgroundWorker();
        private BackgroundWorker outputWorker = new BackgroundWorker();
        BackgroundWorker uploadWorker = new BackgroundWorker();

        #endregion

        public ManageAssetPage(Asset asset)
        {
            try
            {
                InitializeComponent();
                SelectedAsset = asset;
                //UploadStatus = "Selected File";
                AssetName = SelectedAsset.Name;
                jobStatus = new ObservableCollection<JobEntity>();
                Loaded += OnLoaded;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            try
            {
                loading.Visibility = Visibility.Visible;

                filesWorker.DoWork += filesWorker_DoWork;
                filesWorker.RunWorkerCompleted += filesWorker_RunWorkerCompleted;

                if (filesWorker.IsBusy != true)
                {
                    // Start the asynchronous operation.
                    filesWorker.RunWorkerAsync();
                }
                SelectedRadioButton = RadioButtons.ThumbnailRbtn;
                GetEncodingTypes();

                jobStatus.CollectionChanged += JobStatusOnCollectionChanged;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog
                    {
                        FileName = "",
                        DefaultExt = ".mp4",
                        Filter =
                            "MediaFiles (*.asf)|*.asf|(*.avi)|*.avi|(*.m2ts)|*.m2ts|(*.m2v)|*.m2v|(*.mp4)|*.mp4|(*.mpeg)|*.mpeg|(*.mpg)|*.mpg|(*.mts)|*.mts|(*.ts)|*.ts|(*.wmv)|*.wmv",
                        Multiselect = true
                    };

                // Show open file dialog box
                bool? result = dlg.ShowDialog();
                //string filename = UploadStatus;
                // Process open file dialog box results 
                if (result == true)
                {
                    // Open document 
                    filePath = dlg.FileName;
                    //filename = dlg.SafeFileName;
                }
                //UploadStatus = filename;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath)) return;
                UploadStatus = "Uploading...";
                uploadWorker.DoWork += (o, args) => args.Result = SelectedAsset.CreateFileForAsset(filePath);
                uploadWorker.RunWorkerCompleted += (o, args) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                   new Func<Visibility>(
                                                                       () => loading.Visibility = Visibility.Collapsed));

                        bool uploadSuccess = args.Result is bool && (bool) args.Result;
                        if (uploadSuccess)
                        {
                            UploadStatus = "Done";
                            if (filesWorker.IsBusy != true)
                            {
                                loading.Visibility = Visibility.Visible;
                                // Start the asynchronous operation.
                                filesWorker.RunWorkerAsync();
                            }
                            MessageBox.Show("Upload Successful");
                        }
                    };
                uploadWorker.RunWorkerAsync();
                loading.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void GetEncodingTypes()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ConfigurationSettings.AppSettings["EncodeTypes"]))
                {
                    var types = ConfigurationSettings.AppSettings["EncodeTypes"];
                    int i = 0;
                    var typeList =
                        types.Split(Convert.ToChar(";"))
                             .Select(s => new EncodingType {TypeId = i++, EncodingFormat = s})
                             .ToList();
                    EncodingTypes = new ObservableCollection<EncodingType>(typeList);
                }
                SelectedType = EncodingTypes[0];
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void AssignJob_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                var jobWorker = new BackgroundWorker();
                jobWorker.DoWork += (o, args) =>
                    {
                        args.Result = SelectedRadioButton == RadioButtons.EncodeRbtn
                                ? (String.CompareOrdinal(SelectedType.EncodingFormat, "Apple HLS Format") == 0
                                ? SelectedAsset.CreateEncodingJob1(SelectedType.EncodingFormat.Trim()) : SelectedAsset.CreateEncodingJob(SelectedType.EncodingFormat.Trim()))
                                : SelectedAsset.CreateThumbnailJob();
                    };
                jobWorker.RunWorkerCompleted += (o, args) =>
                    {
                        var job = args.Result as Job;
                        JobStatus.Add(new JobEntity {Name = job.Name, State = job.State.ToString(), WamJob = job});
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                   new Func<Visibility>(
                                                                       () => loading.Visibility = Visibility.Collapsed));
                    };

                if (jobWorker.IsBusy != true) jobWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void JobStatusOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var job = e.NewItems[0] as JobEntity;
                    var worker = new BackgroundWorker();
                    worker.DoWork += (o, args) => { args.Result = job.WamJob.GetJobStatus(job.WamJob.Id).ToString(); };
                    worker.RunWorkerCompleted += (o, args) =>
                        {
                            job.State = args.Result as string;
                            if (job.State != JobState.Finished.ToString() || job.State != JobState.Error.ToString())
                            {
                                worker.RunWorkerAsync();
                            }
                        };
                    worker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void CreateThumbnailUrl(DoWorkEventArgs e)
        {
            try
            {
                var thmbnails = new List<Thumnbnails>();

                List<string> thumnailUrls = MediaContextHelper.Instance.GetThumbnailUrls(SelectedAsset.Id);
                thmbnails =
                    thumnailUrls.Select(thumb => new Thumnbnails {URL = thumb, Caption = SelectedAsset.Name}).ToList();
                e.Result = thmbnails;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void MakePrimary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var assetFile = AssetFileList.FirstOrDefault(file => file.IsPrimary);
                if (SelectedAssetFile == null)
                {
                    MessageBox.Show("Please select a file to make it primary",
                                    "Information");
                    return;
                }
                if (SelectedAssetFile.IsPrimary)
                {
                    MessageBox.Show(string.Format("File - {0} is already a primary.", selectedAssetFile.Name),
                                    "Information");
                    return;
                }
                loading.Visibility = Visibility.Visible;
                var primaryWorker = new BackgroundWorker();
                primaryWorker.DoWork += (o, args) =>
                    {
                        if (assetFile != null)
                            SelectedAsset.MakePrimaryFile(assetFile.Id, false);
                        bool primaryFileMade = SelectedAsset.MakePrimaryFile(SelectedAssetFile.Id, true);
                        args.Result = primaryFileMade;
                    };
                primaryWorker.RunWorkerCompleted += (o, args) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                                   new Func<Visibility>(
                                                                       () => loading.Visibility = Visibility.Collapsed));

                        var success = args.Result is bool && (bool) args.Result;
                        if (success)
                        {
                            MessageBox.Show(string.Format("File - {0} is made as primary.", SelectedAssetFile.Name),
                                            "Information");
                            if (filesWorker.IsBusy != true)
                            {
                                loading.Visibility = Visibility.Visible;
                                filesWorker.RunWorkerAsync();
                            }
                        }
                    };
                if (primaryWorker.IsBusy != true)
                    primaryWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void MakeNonPrimary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool primaryFileMade = SelectedAsset.MakePrimaryFile(SelectedAssetFile.Id, false);
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void CreateSASURL_Click(object sender, RoutedEventArgs e)
        {
            //var encodeAccessPolicy = MediaContextHelper.Instance.CreateAccessPolicy("EncodeAccess", 15,
            //                                                                        AccessPolicyPermission.Read);
            //var primaryFile = AssetFileList.FirstOrDefault(file => file.IsPrimary == true);
            //string locator = SelectedAsset.CreateLocator(encodeAccessPolicy.Id, SelectedAsset.Id);
            //var sasUrl = SelectedAsset.BuildSasUrl(locator, primaryFile.Name);
        }

        private void CreateOutputUrl(DoWorkEventArgs e)
        {
            try
            {
                List<AssetWithFiles> outputUrls = MediaContextHelper.Instance.GetOutputAssetFiles(SelectedAsset.Id);
                e.Result = outputUrls;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var selectedTab = e.AddedItems[0] as TabItem; // Gets selected tab
                    if (selectedTab != null)
                    {
                        if (selectedTab.Name == "tabThumbnail" && Thumbnails == null ||
                            (Thumbnails != null && Thumbnails.Count == 0))
                        {
                            loading.Visibility = Visibility.Visible;

                            if (thumbnailWorker.IsBusy != true)
                            {
                                thumbnailWorker.DoWork += thumbnailWorker_DoWork;
                                thumbnailWorker.RunWorkerCompleted += thumbnailWorker_RunWorkerCompleted;

                                // Start the asynchronous operation.
                                thumbnailWorker.RunWorkerAsync();
                            }
                        }
                        else if (selectedTab.Name == "tabOutput" && OutputSasUrls == null ||
                                 (OutputSasUrls != null && OutputSasUrls.Count == 0))
                        {
                            loading.Visibility = Visibility.Visible;

                            if (outputWorker.IsBusy != true)
                            {
                                outputWorker.DoWork += outputWorker_DoWork;
                                outputWorker.RunWorkerCompleted += outputWorker_RunWorkerCompleted;

                                // Start the asynchronous operation.
                                outputWorker.RunWorkerAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void filesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var assetFiles = SelectedAsset.ListAssetFiles();
                e.Result = assetFiles;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void filesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                AssetFileList = new ObservableCollection<AssetFile>(e.Result as List<AssetFile>);
                this.DataContext = this;
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                           new Func<Visibility>(
                                                               () => loading.Visibility = Visibility.Collapsed));
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void thumbnailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                CreateThumbnailUrl(e);
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void thumbnailWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                Thumbnails = new ObservableCollection<Thumnbnails>(e.Result as List<Thumnbnails>);
                thumbnailWorker.DoWork -= thumbnailWorker_DoWork;
                thumbnailWorker.RunWorkerCompleted -= thumbnailWorker_RunWorkerCompleted;

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                           new Func<Visibility>(
                                                               () => loading.Visibility = Visibility.Collapsed));
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void outputWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                OutputSasUrls = new ObservableCollection<AssetWithFiles>(e.Result as List<AssetWithFiles>);

                outputWorker.DoWork -= outputWorker_DoWork;
                outputWorker.RunWorkerCompleted -= outputWorker_RunWorkerCompleted;

                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                                           new Func<Visibility>(
                                                               () => loading.Visibility = Visibility.Collapsed));
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void outputWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                CreateOutputUrl(e);
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tabFiles.IsSelected && filesWorker.IsBusy != true)
                {
                    loading.Visibility = Visibility.Visible;
                    // Start the asynchronous operation.
                    filesWorker.RunWorkerAsync();
                }
                if (tabThumbnail.IsSelected && thumbnailWorker.IsBusy != true)
                {
                    loading.Visibility = Visibility.Visible;
                    thumbnailWorker.DoWork += thumbnailWorker_DoWork;
                    thumbnailWorker.RunWorkerCompleted += thumbnailWorker_RunWorkerCompleted;
                    // Start the asynchronous operation.
                    thumbnailWorker.RunWorkerAsync();
                }
                if (tabOutput.IsSelected && outputWorker.IsBusy != true)
                {
                    loading.Visibility = Visibility.Visible;
                    outputWorker.DoWork += outputWorker_DoWork;
                    outputWorker.RunWorkerCompleted += outputWorker_RunWorkerCompleted;
                    // Start the asynchronous operation.
                    outputWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                filesWorker.Dispose();
                outputWorker.Dispose();
                thumbnailWorker.Dispose();
                uploadWorker.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    public class EncodingType
    {
        public int TypeId { get; set; }
        public string EncodingFormat { get; set; }
    }

    public class JobEntity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Job wamJob;
        public Job WamJob
        {
            get { return wamJob; }
            set
            {
                wamJob = value;
                OnPropertyChanged("WamJob");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string state;
        public string State
        {
            get { return state; }
            set
            {
                state = value;
                OnPropertyChanged("State");
            }
        }
    }
}
