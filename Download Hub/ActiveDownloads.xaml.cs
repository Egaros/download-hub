using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModel;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Download_Hub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ActiveDownloads : Page
    {
        public static List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        IReadOnlyList<DownloadOperation> downloads;
        //ActiveDownloadsViewModel viewModel;

        public ActiveDownloads()
        {
            this.InitializeComponent();
            //viewModel = (ActiveDownloadsViewModel)this.Resources["viewModel"];
        }

  
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
       

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            base.OnNavigatedTo(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                await DiscoverActiveDownloadsAsync();
                //await DiscoverActiveDownloadsAsync();
            }
            catch (Exception ex)
            {
                Helper.HelperMethods.MessageUser("Something went wrong");
                //Frame.Navigate(typeof(MainPage));
            }

            //viewModel.GetReferences(view,downloads);

        }

        private void downloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CurrentSourcePageType != typeof(ActiveDownloads))
                Frame.Navigate(typeof(ActiveDownloads));
        }

        private void helpAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(help));
        }

        private void allDownloadsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(allDownloads));
        }

        private void homeAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CurrentSourcePageType != typeof(MainPage))
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void aboutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(aboutPage));
        }

        private async Task DiscoverActiveDownloadsAsync()
        {

            activeDownloads = new List<DownloadOperation>();
            
            downloads = null;
            downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            //downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(music.musicGroup);

            //Log("Loading background downloads: " + downloads.Count);
            DataContext = downloads;


            //DataContext = ActiveDownloadInitialiser.dl;
            if (downloads.Count > 0)
            {
                
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {   
                    // Attach progress and completion handlers.
                    tasks.Add(HandleDownloadAsync(download, false));
                }

                
                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
                // download only when the first one completed; attach to the third download when the second one
                // completes etc. We want to attach to all downloads immediately.
                // If there are actions that need to be taken once downloads complete, await tasks here, outside
                // the loop.

                await Task.WhenAll(tasks);
            }
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                //LogStatus("Running: " + download.Guid, NotifyType.StatusMessage);

                // Store the download so we can pause/resume.
                activeDownloads.Add(download);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                //CancellationTokenSource cts = new CancellationTokenSource();
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.AttachAsync().AsTask(progressCallback);

                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    //await download.AttachAsync().AsTask(App.cts.Token, progressCallback);
                    try
                    {
                        await download.AttachAsync().AsTask(progressCallback);
                    }
                    catch (Exception ex)
                    {
                        //Helper.HelperMethods.MessageUser("Something went wrong. Please try again");
                    }
                }


                //ResponseInformation response = download.GetResponseInformation();

                //LogStatus(String.Format(CultureInfo.CurrentCulture, "Completed: {0}, Status Code: {1}",
                //    download.Guid, response.StatusCode), NotifyType.StatusMessage);


            }
            catch (TaskCanceledException)
            {
                Helper.HelperMethods.MessageUser("Something went wrong. Please try again");
            }
            finally
            {
                activeDownloads.Remove(download);
            }


        }

        private void DownloadProgress(DownloadOperation download)
        {
            
            double percent = 0;
            if (download.Progress.TotalBytesToReceive > 0)
            {
                percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
                try
                {
                    //if (percent == 100 && !App.allDownloads.Containers.ContainsKey(download.ResultFile.Name))                    
                    //    App.allDownloads.Values.Add(new KeyValuePair<string, object>(download.ResultFile.Name, download.ResultFile.Path));
                    if (percent == 100 && !Helper.HelperMethods.checkEntry(App.conn, download.ResultFile.Name, download.ResultFile.Path))
                    {
                        Helper.HelperMethods.addData(App.conn, download.ResultFile.Name, download.ResultFile.Path,download.ResultFile.FileType.ToString());
                    }
                }
                catch
                { }
                var listView = view as ListView;
                foreach (var listViewItem in listView.Items)
                {
                    if (listViewItem.Equals(download as object))
                    {
                        var container = listView.ContainerFromItem(listViewItem);
                        var children = AllChildren(container);
                        var progressName = "progressBar";
                        var bytesReceivedName = "bytesReceived";
                        var totalBytesName = "totalBytes";
                        var statusName = "statusText";
                        var _progressControl = (ProgressBar)children.First(c => c.Name == progressName);
                        _progressControl.Value = percent;

                        var _bytesReceivedControl = (TextBox)children.First(d => d.Name == bytesReceivedName);
                        _bytesReceivedControl.Text = ((download.Progress.BytesReceived / 1024) + " KB").ToString();
                        var _totalBytesControl = (TextBox)children.First(d => d.Name == totalBytesName);
                        _totalBytesControl.Text = ((download.Progress.TotalBytesToReceive / 1024) + " KB").ToString();
                        var _statusControl = (TextBox)children.First(d => d.Name == statusName);
                        _statusControl.Text = download.Progress.Status.ToString();

                    }
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ActiveDownloads));
        }

        private void view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as ListView;
            DownloadOperation operation = view.SelectedItem as DownloadOperation;
            Frame.Navigate(typeof(activeDownloadSelection), operation);
        }

        public List<Control> AllChildren(DependencyObject parent)
        {
            var _list = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _child = VisualTreeHelper.GetChild(parent, i);
                if (_child is Control)
                {
                    _list.Add(_child as Control);
                }
                _list.AddRange(AllChildren(_child));
            }
            return _list;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var t in view.Items)
            {
                var download = t as DownloadOperation;
                download.AttachAsync().Cancel();
            }
            Frame.Navigate(typeof(ActiveDownloads));
        }        
    }
}
