using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.ModernUI.Common;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Ais.Internal.Dcm.ModernUI.Tabs
{
    /// <summary>
    /// Interaction logic for AssetFiles.xaml
    /// </summary>
    public partial class AssetFiles : UserControl, IContent, INotifyPropertyChanged, IDisposable
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

        private Asset selectedAsset;
        public Asset SelectedAsset { get { return selectedAsset; } set { selectedAsset = value; OnPropertyChanged("SelectedAsset"); } }
        private string filePath = string.Empty;

        public string AssetName { get; set; }
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

        private BackgroundWorker filesWorker = new BackgroundWorker();
        BackgroundWorker uploadWorker = new BackgroundWorker();
        #endregion

        public AssetFiles()
        {
            InitializeComponent();
            filesWorker.DoWork += filesWorker_DoWork;
            filesWorker.RunWorkerCompleted += filesWorker_RunWorkerCompleted;
        }

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                Loaded -= AssetFiles_Loaded;
            }
            catch (Exception exp)
            {
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
                UIHelper.HandlerException(exp);
            }
        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                var frame = NavigationHelper.FindFrame(null, (FrameworkElement)this.Parent);
                if (frame != null)
                {
                    var source = frame.Source;
                    string assetId = source.ToString().Split('#')[1];
                    SelectedAsset = MediaContextHelper.SelectedAsset;
                    if (MediaContextHelper.AssetFileList != null && MediaContextHelper.AssetFileList.Count > 0)
                    {
                        if (AssetFileList == null)
                        {
                            loading.Visibility = Visibility.Collapsed;
                            AssetFileList = new ObservableCollection<AssetFile>(MediaContextHelper.AssetFileList);
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
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        void AssetFiles_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                if (filesWorker.IsBusy != true)
                {
                    // Start the asynchronous operation.
                    filesWorker.RunWorkerAsync();
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
                MediaContextHelper.AssetFileList = e.Result as List<AssetFile>;
                AssetFileList = new ObservableCollection<AssetFile>(e.Result as List<AssetFile>);
                this.DataContext = this;
                loading.Visibility = Visibility.Collapsed;
                AssetFilesGrid.Visibility = Visibility.Visible;
                if (AssetFileList.Count > 0)
                    MediaContextHelper.IsJobEnabled = true;
                else
                    MediaContextHelper.IsJobEnabled = false;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                Loaded -= AssetFiles_Loaded;
            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
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
                // Process open file dialog box results 
                if (result == true)
                {
                    // Open document 
                    filePath = dlg.FileName;
                    UploadFileToAsset(filePath);
                }
                //UploadStatus = filename;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        void UploadFileToAsset(string path)
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

                    bool uploadSuccess = args.Result is bool && (bool)args.Result;
                    if (uploadSuccess)
                    {
                        UploadStatus = "Done";
                        if (filesWorker.IsBusy != true)
                        {
                            loading.Visibility = Visibility.Visible;
                            // Start the asynchronous operation.
                            filesWorker.RunWorkerAsync();
                        }
                        ModernDialog dlg = new ModernDialog();
                        dlg.Title = "Upload Successful";
                        dlg.ShowDialog();
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

        private void Reload_Files(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                this.AssetFileList = null;
                AssetFiles_Loaded(sender, e);
            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        private void MakePrimary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var assetFile = AssetFileList.FirstOrDefault(file => file.IsPrimary);
                if (SelectedAssetFile == null)
                {
                    
                    ModernDialog dlg = new ModernDialog();
                    dlg.Title = "Please select a file to make it primary";
                    dlg.ShowDialog();
                    return;
                }
                if (SelectedAssetFile.IsPrimary)
                {
                    string str = "Selected file is already a primary.";
                    ModernDialog dlg = new ModernDialog();
                    dlg.Title = str;
                    dlg.ShowDialog();
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

                    var success = args.Result is bool && (bool)args.Result;
                    if (success)
                    {
                        string msg = "Selected file is made as primary.";
                        ModernDialog dlg = new ModernDialog();
                        dlg.Title = msg;
                        dlg.ShowDialog();
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (filesWorker != null) filesWorker.Dispose();
                if (uploadWorker != null) uploadWorker.Dispose();
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
            if (PropertyChanged != null)
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
