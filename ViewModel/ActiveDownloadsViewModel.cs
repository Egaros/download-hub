using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ViewModel
{
    public class ActiveDownloadsViewModel : INotifyPropertyChanged
    {
        //properties
        public event PropertyChangedEventHandler PropertyChanged;
        //private double _progress = 0.0;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public string ActiveDownloadCount
        //{
        //    get
        //    {
        //        if (Downloads == null)
        //            return 0.ToString();
        //        else
        //        return Downloads.Count.ToString();
        //    }
        //    set
        //    {

        //    }
        //}

        //public IReadOnlyList<DownloadOperation> Downloads
        //{
        //    get
        //    {
        //        return _downloads;
        //    }
        //    set
        //    {
        //        _downloads = value;
        //        NotifyPropertyChanged();
        //    }
        //}
        //public double Progress
        //{
        //    get
        //    {
        //        return _progress;
        //    }
        //    set
        //    {
        //        _progress = value;
        //        NotifyPropertyChanged();

        //    }
        //}





        //public async Task DiscoverActiveDownloadsAsync()
        //{

        //    activeDownloads = new List<DownloadOperation>();

        //    Downloads = null;
        //    Downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
        //    //downloads = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(music.musicGroup);

        //    //Log("Loading background downloads: " + downloads.Count);
        //    //DataContext = downloads;


        //    //DataContext = ActiveDownloadInitialiser.dl;
        //    if (_downloads.Count > 0)
        //    {

        //        List<Task> tasks = new List<Task>();
        //        foreach (DownloadOperation download in _downloads)
        //        {
        //            // Attach progress and completion handlers.
        //            tasks.Add(HandleDownloadAsync(download, false));
        //        }


        //        // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second
        //        // download only when the first one completed; attach to the third download when the second one
        //        // completes etc. We want to attach to all downloads immediately.
        //        // If there are actions that need to be taken once downloads complete, await tasks here, outside
        //        // the loop.

        //        await Task.WhenAll(tasks);
        //    }
        //}

        //private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        //{
        //    try
        //    {
        //        //LogStatus("Running: " + download.Guid, NotifyType.StatusMessage);

        //        // Store the download so we can pause/resume.
        //        activeDownloads.Add(download);
        //        Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
        //        //CancellationTokenSource cts = new CancellationTokenSource();
        //        if (start)
        //        {
        //            // Start the download and attach a progress handler.
        //            await download.AttachAsync().AsTask(progressCallback);

        //        }
        //        else
        //        {
        //            // The download was already running when the application started, re-attach the progress handler.
        //            //await download.AttachAsync().AsTask(App.cts.Token, progressCallback);
        //            try
        //            {
        //                await download.AttachAsync().AsTask(progressCallback);
        //            }
        //            catch (Exception ex)
        //            {
        //                //Helper.HelperMethods.MessageUser("Something went wrong. Please try again");
        //            }
        //        }


        //        //ResponseInformation response = download.GetResponseInformation();

        //        //LogStatus(String.Format(CultureInfo.CurrentCulture, "Completed: {0}, Status Code: {1}",
        //        //    download.Guid, response.StatusCode), NotifyType.StatusMessage);


        //    }
        //    catch (TaskCanceledException)
        //    {
        //        Helper.HelperMethods.MessageUser("Something went wrong. Please try again");
        //    }
        //    finally
        //    {
        //        activeDownloads.Remove(download);
        //    }


        //}

        //private void DownloadProgress(DownloadOperation download)
        //{

        //    double percent = 0;
        //    if (download.Progress.TotalBytesToReceive > 0)
        //    {
        //        percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
        //        try
        //        {
        //            //if (percent == 100 && !App.allDownloads.Containers.ContainsKey(download.ResultFile.Name))                    
        //            //    App.allDownloads.Values.Add(new KeyValuePair<string, object>(download.ResultFile.Name, download.ResultFile.Path));
        //            if (percent == 100 && !Helper.HelperMethods.checkEntry(App.conn, download.ResultFile.Name, download.ResultFile.Path))
        //            {
        //                Helper.HelperMethods.addData(App.conn, download.ResultFile.Name, download.ResultFile.Path, download.ResultFile.FileType.ToString());
        //            }
        //        }
        //        catch
        //        { }
        //        var listView = view as ListView;
        //        foreach (var listViewItem in listView.Items)
        //        {
        //            if (listViewItem.Equals(download as object))
        //            {
        //                var container = listView.ContainerFromItem(listViewItem);
        //                var children = AllChildren(container);
        //                var progressName = "progressBar";
        //                var bytesReceivedName = "bytesReceived";
        //                var totalBytesName = "totalBytes";
        //                var statusName = "statusText";
        //                var _progressControl = (ProgressBar)children.First(c => c.Name == progressName);
        //                _progressControl.Value = percent;

        //                var _bytesReceivedControl = (TextBox)children.First(d => d.Name == bytesReceivedName);
        //                _bytesReceivedControl.Text = ((download.Progress.BytesReceived / 1024) + " KB").ToString();
        //                var _totalBytesControl = (TextBox)children.First(d => d.Name == totalBytesName);
        //                _totalBytesControl.Text = ((download.Progress.TotalBytesToReceive / 1024) + " KB").ToString();
        //                var _statusControl = (TextBox)children.First(d => d.Name == statusName);
        //                _statusControl.Text = download.Progress.Status.ToString();

        //            }
        //        }
        //    }

        //}

        //public List<Control> AllChildren(DependencyObject parent)
        //{
        //    var _list = new List<Control>();
        //    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        //    {
        //        var _child = VisualTreeHelper.GetChild(parent, i);
        //        if (_child is Control)
        //        {
        //            _list.Add(_child as Control);
        //        }
        //        _list.AddRange(AllChildren(_child));
        //    }
        //    return _list;
        //}

        //public void GetReferences(ListView temp, IReadOnlyList<DownloadOperation> downloads)
        //{
        //    view = temp;
        //    Downloads = downloads;
        //}

    }
}
