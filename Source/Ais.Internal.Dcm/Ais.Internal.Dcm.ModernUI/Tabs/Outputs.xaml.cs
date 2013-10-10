using Ais.Internal.Dcm.Business;
using Ais.Internal.Dcm.ModernUI.Common;
using Ais.Internal.Dcm.ModernUI.Pages;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Ais.Internal.Dcm.ModernUI.Tabs
{
    /// <summary>
    /// Interaction logic for Outputs.xaml
    /// </summary>
    public partial class Outputs : UserControl, IContent, INotifyPropertyChanged, IDisposable
    {
        public Outputs()
        {
            InitializeComponent();
           
            jobStatus = new ObservableCollection<JobEntity>();
            jobStatus.CollectionChanged += JobStatusOnCollectionChanged;

            // this.Loaded -= Thumbnails_Loaded;
            outputWorker.DoWork += OutputWorker_DoWork;
            outputWorker.RunWorkerCompleted += OutputWorker_RunWorkerCompleted;
        }

        private BackgroundWorker outputWorker = new BackgroundWorker();
        private ObservableCollection<AssetWithFiles> _urls = null;

        private Asset selectedAsset;
        public Asset SelectedAsset { get { return selectedAsset; } set { selectedAsset = value; OnPropertyChanged("SelectedAsset"); } }

        /// <summary>
        /// Represents the Asset Under Management
        /// </summary>
        public string SelectedAssetId { get; set; }

        /// <summary>
        /// Bindable thumbnail urls of the asset
        /// </summary>
        public ObservableCollection<AssetWithFiles> Urls
        {
            get { return _urls; }
            set
            {
                _urls = value; OnPropertyChanged("Urls");
            }
        }
        /// <summary>
        /// Wrapper around PropertyChanged Event
        /// </summary>
        /// <param name="propertyName">name of the property</param>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            try
            {
                Loaded -= Output_Loaded;
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
                Loaded -= Output_Loaded;
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
                var frame = NavigationHelper.FindFrame(null, (FrameworkElement) this.Parent);
                if (frame != null)
                {
                    var source = frame.Source;
                    string assetId = source.ToString().Split('#')[1];
                    SelectedAssetId = assetId;
                    SelectedAsset = MediaContextHelper.SelectedAsset;
                    if (MediaContextHelper.OutputUrls != null && MediaContextHelper.OutputUrls.Count > 0)
                    {
                        if (Urls == null)
                        {
                            loading.Visibility = Visibility.Collapsed;
                            Urls = new ObservableCollection<AssetWithFiles>(MediaContextHelper.OutputUrls);
                            this.DataContext = this;
                        }
                        Loaded -= Output_Loaded;
                    }
                    else
                    {
                        Loaded += Output_Loaded;
                    }
                  
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Output_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (outputWorker.IsBusy != true)
                {
                    // Start the asynchronous operation.
                    outputWorker.RunWorkerAsync();
                }
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
                this.Loaded -= Output_Loaded;
            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        private void OutputWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var assetFiles = MediaContextHelper.Instance.GetOutputAssetFiles(SelectedAssetId);
                e.Result = assetFiles;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void OutputWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                MediaContextHelper.OutputUrls = e.Result as List<AssetWithFiles>;
                Urls = new ObservableCollection<AssetWithFiles>(e.Result as List<AssetWithFiles>);
                this.DataContext = this;
                loading.Visibility = Visibility.Collapsed;
                outputGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Reload_Outputs(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                Urls = null;
                Output_Loaded(sender, e);
            }
            catch (Exception exp)
            {
                UIHelper.HandlerException(exp);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MediaContextHelper.IsJobEnabled)
                {
                    CreateEncodingJobDialog dialog = new CreateEncodingJobDialog(SelectedAssetId);
                    dialog.Width = 500;
                    dialog.Height = 500;
                    dialog.JobCreated += (o, args) =>
                        {
                            var jobEvengArgs = args as JobEventArgs;
                            if (jobEvengArgs != null)
                            {
                                var job = jobEvengArgs.Job;
                                JobStatus.Add(new JobEntity
                                    {
                                        Name = job.Name,
                                        State = job.State.ToString(),
                                        WamJob = job
                                    });
                            }
                        };
                    dialog.JobCreated -= (o, args) => { };
                    dialog.ShowDialog();
                }
                else
                {
                    ModernDialog msgBox = new ModernDialog();
                    msgBox.Title = "There are no files in this Asset.";
                    msgBox.ShowDialog();
                }
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

        private void CopyToClipBoard_Click(object sender, RoutedEventArgs e)
        {
            AssetWithFiles file = GetStreamingUrl(sender);
            if (file != null)
            {
                Clipboard.SetDataObject(file.URL, true);
                ModernDialog dialog = new ModernDialog();
                dialog.Title = "URL copied to clipboard";
                dialog.Show();
            } 
        }

        private static AssetWithFiles GetStreamingUrl(object sender)
        {
            try
            {
                Button btnCopy = (Button) sender;
                if (btnCopy != null)
                {
                    AssetWithFiles file = btnCopy.DataContext as AssetWithFiles;
                    return file;
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
            return null;
        }

        private static void GenerateSmoothStreamingHtml(string url)
        {
            try
            {
                string curDir = Directory.GetCurrentDirectory();
                string content = string.Empty;
                StringBuilder output = new StringBuilder();
                string path = curDir + "\\preview-template-ss.html";
                string fileContents = File.ReadAllText(path);
                fileContents = fileContents.Replace("[mediafileurl]", url);
                string newPath = curDir + "\\preview.html";
                File.WriteAllText(newPath, fileContents);
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private static void GenerateMp4Html(string url)
        {
            try
            {
                string curDir = Directory.GetCurrentDirectory();
                string content = string.Empty;
                StringBuilder output = new StringBuilder();
                string path = curDir + "\\preview-template-mp4.html";
                string fileContents = File.ReadAllText(path);
                fileContents = fileContents.Replace("[mediafileurl]", url);
                string newPath = curDir + "\\preview.html";
                File.WriteAllText(newPath, fileContents);
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AssetWithFiles file = GetStreamingUrl(sender);
                string url = string.Format("../Tabs/Viewer.xaml");
                if (file != null)
                {
                    if (file.URL.Contains(".ism"))
                    {
                        GenerateSmoothStreamingHtml(file.URL);
                        MediaContextHelper.SelectedAssetType = "SMOOTHSTREAMING";
                    }
                    else
                    {
                        GenerateMp4Html(file.URL);
                        MediaContextHelper.SelectedAssetType = "MP4";
                    }
                }
                PreviewWindow window = new PreviewWindow();
                window.CloseButton.Content = "Close";
                window.ShowDialog();
                //var frame = NavigationHelper.FindFrame(null, this);
                //if (frame != null)
                //{
                //    frame.Source = new Uri(url, UriKind.Relative);
                //    frame.KeepContentAlive = false;
                //}
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
                if (outputWorker != null) outputWorker.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

