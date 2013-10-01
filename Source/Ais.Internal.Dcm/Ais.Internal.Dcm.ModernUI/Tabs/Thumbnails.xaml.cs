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
using System.Windows;
using System.Windows.Controls;

namespace Ais.Internal.Dcm.ModernUI.Tabs
{
    /// <summary>
    /// Interaction logic for Thumbnails.xaml
    /// </summary>
    public partial class Thumbnails : UserControl,IContent,INotifyPropertyChanged, IDisposable
    {
        private BackgroundWorker filesWorker = new BackgroundWorker();
        private ObservableCollection<string> _urls = null;

        private Asset selectedAsset;
        public Asset SelectedAsset { get { return selectedAsset; } set { selectedAsset = value; OnPropertyChanged("SelectedAsset"); } }

        /// <summary>
        /// Represents the Asset Under Management
        /// </summary>
        public string SelectedAssetId { get; set; }

        /// <summary>
        /// Bindable thumbnail urls of the asset
        /// </summary>
        public ObservableCollection<string> Urls
        {
            get { return _urls; }
            set
            {
                _urls = value; OnPropertyChanged("Urls");
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
 
        public Thumbnails()
        {
            InitializeComponent();
            jobStatus = new ObservableCollection<JobEntity>();
            jobStatus.CollectionChanged += JobStatusOnCollectionChanged;

            // this.Loaded -= Thumbnails_Loaded;
            filesWorker.DoWork += ThumbnailWorker_DoWork;
            filesWorker.RunWorkerCompleted += ThumbnailWorker_RunWorkerCompleted;
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

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            Loaded -= Thumbnails_Loaded;
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {
            Loaded -= Thumbnails_Loaded;
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
                    SelectedAssetId = assetId;
                    SelectedAsset = MediaContextHelper.SelectedAsset;
                    if (MediaContextHelper.ThumbnailUrls != null && MediaContextHelper.ThumbnailUrls.Count > 0)
                    {
                        if (Urls == null)
                        {
                            loading.Visibility = Visibility.Collapsed;
                            Urls = new ObservableCollection<string>(MediaContextHelper.ThumbnailUrls);
                            this.DataContext = this;
                        }
                        Loaded -= Thumbnails_Loaded;
                    }
                    else
                    {
                        Loaded += Thumbnails_Loaded;
                    }
                }
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Thumbnails_Loaded(object sender, RoutedEventArgs e)
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

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
           
        }

        private void ThumbnailWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var assetFiles = MediaContextHelper.Instance.GetThumbnailUrls(SelectedAssetId);
                e.Result = assetFiles;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void ThumbnailWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                MediaContextHelper.ThumbnailUrls = e.Result as List<string>;
                Urls = new ObservableCollection<string>(e.Result as List<string>);
                this.DataContext = this;
                loading.Visibility = Visibility.Collapsed;
                ThumbnailGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                UIHelper.HandlerException(ex);
            }
        }

        private void Reload_Thumbnails(object sender, RoutedEventArgs e)
        {
            try
            {
                loading.Visibility = Visibility.Visible;
                Urls = null;
                Thumbnails_Loaded(sender, e);
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
                CreateThumbnailJobDialog dialog = new CreateThumbnailJobDialog(SelectedAssetId);
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

        private void Image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null)
                {
                    string url = btn.DataContext.ToString();
                    Clipboard.SetDataObject(url, true);
                    ModernDialog dialog = new ModernDialog();
                    dialog.Title = "URL copied to clipboard";
                    dialog.Show();
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
                if (filesWorker != null) filesWorker.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
