using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using YoutubeExtractor;

namespace ViewModel
{
    public class VideosViewModel : INotifyPropertyChanged
    {
        List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        public static CancellationTokenSource cts;
        Progress<DownloadOperation> progressCallback;
        Uri source;
        FolderPicker picker;
        StorageFolder folder;
        StorageFile file;
        //string youtubeUrl = "";
        //string metacafeUrl = "";
        //MetaCafeVideo metacafeVideo;
        VideoQuality youtubeVideo;
        GridView gridView;


        //properties
        
        private string _selectedVideoTitle = "";
        private bool _downloadButtonActive = false;
        private Visibility _selectionFlyout = Visibility.Collapsed;
        private List<VideoQuality> _listOfVideos;
        private Visibility _webViewVisible = Visibility.Visible;
        private Visibility _progressStackVisible = Visibility.Collapsed;
        private Visibility _selecttionGridViewVisible = Visibility.Collapsed;
        private bool _isProgressRingActive = false;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public VideosViewModel()
        {
            cts = new CancellationTokenSource();
        }

        public bool DownloadButtonActive
        {
            get
            {
                return _downloadButtonActive;
            }
            set
            {
                _downloadButtonActive = value;
                NotifyPropertyChanged();
            }

        }



        public Visibility SelectionFlyout
        {
            get
            {
                return _selectionFlyout;
            }
            set
            {
                _selectionFlyout = value;
                NotifyPropertyChanged();
            }

        }

        public string SelectedVideoTitle
        {
            get
            {
                return _selectedVideoTitle;
            }
            set
            {
                _selectedVideoTitle = value;
                NotifyPropertyChanged();
            }

        }

        public List<VideoQuality> ListOfVideos
        {
            get
            {
                return _listOfVideos;
            }
            set
            {
                _listOfVideos = value;
                NotifyPropertyChanged();

            }
        }

        public Visibility WebViewVisible
        {
            get
            {
                return _webViewVisible;
            }
            set
            {
                _webViewVisible = value;
                NotifyPropertyChanged();
            }

        }
        public Visibility ProgressStackVisible
        {
            get
            {
                return _progressStackVisible;
            }
            set
            {
                _progressStackVisible = value;
                NotifyPropertyChanged();
            }

        }

        public Visibility SelectionGridViewVisible
        {
            get
            {
                return _selecttionGridViewVisible;
            }
            set
            {
                _selecttionGridViewVisible = value;
                NotifyPropertyChanged();
            }

        }
        public bool ProgressRingActive
        {
            get
            {
                return _isProgressRingActive;
            }
            set
            {
                _isProgressRingActive = value;
                NotifyPropertyChanged();
            }

        }

        //public void checkForUrl(WebViewDOMContentLoadedEventArgs args)
        //{
        //    string temp = args.Uri.ToString();
        //    if (temp.Contains("/watch?v="))
        //    {
        //        youtubeUrl = temp;
        //    }
        //    Task.Delay(3000);
        //    if (youtubeUrl.Contains("/watch?v="))
        //        DownloadButtonActive = true;
        //    else
        //        DownloadButtonActive = false;
        //}

        public void downloadClick(WebView youtubeWebElement, string youtubeUrl)
        {
            if (youtubeUrl.Contains("/watch?v="))
            {
                if (youtubeWebElement.CanGoBack)
                    youtubeWebElement.GoBack();
                getYoutubeVideo(youtubeUrl);
                //youtubeUrl = "";
            }
        }

        public async void getYoutubeVideo(string youtubeUrl)
        {
            //typeOfVideo = "youtube";
            //youtubeWebElement.Stop();
            WebViewVisible = Visibility.Collapsed;
            ProgressRingActive = true;
            ProgressStackVisible = Visibility.Visible;

            ListOfVideos = await YoutubeDownloader.GetYouTubeVideoUrls(youtubeUrl);

            //youtube url to null
            youtubeUrl = "";
            int count = ListOfVideos.Count;
            for (int i = 0, j = 0; i < count; i++)
            {
                ListOfVideos.ElementAt(i).stringDimension = ListOfVideos.ElementAt(i).Dimension.ToString();
                //videos.ElementAt(j).stringDimension = videos.ElementAt(j).Dimension.ToString();
                //Uri uri = new Uri(videos.ElementAt(j).DownloadUrl);

                //WebRequest request = WebRequest.Create(uri);
                //try
                //{
                //    WebResponse response = await request.GetResponseAsync();
                //}
                //catch(WebException ex)
                //{
                //    //HelperMethods.HelperMethods.MessageUser(ex.ToString()+"\n"+ex.Message.ToString());
                //    videos.RemoveAt(j);
                //    continue;
                //}
                //j++;

            }
            if (_listOfVideos.Count == 0)
            {
                ProgressRingActive = false;
                ProgressStackVisible = Visibility.Collapsed;
                await Helper.HelperMethods.MessageUser("Something unexpected happenned or Youtube doesnt allow to download this. Try again to find out");
                WebViewVisible = Visibility.Visible;
                //Frame.Navigate(typeof(videos));
                //youtubeWebElement.Visibility = Visibility.Visible;
            }
            else
            {
                //DataContext = videos;
                ProgressRingActive = false;
                ProgressStackVisible = Visibility.Collapsed;
                SelectedVideoTitle = _listOfVideos.ElementAt(0).VideoTitle.ToString();
                SelectionGridViewVisible = Visibility.Visible;
                //await progIndicator.HideAsync();
            }

        }

        public void selectionChanged(object sender)
        {
            gridView = sender as GridView;
            if (gridView.SelectedIndex != -1)
            {
                youtubeVideo = gridView.SelectedItem as VideoQuality;
                youtubeVideo.VideoTitle = Helper.HelperMethods.normalizeName(youtubeVideo.VideoTitle);
                source = new Uri(youtubeVideo.DownloadUrl);
                SelectionGridViewVisible = Visibility.Collapsed;
                SelectionFlyout = Visibility.Visible;
            }

            //source = new Uri("http://r1---sn-qxa7en7s.c.youtube.com/videoplayback?sparams=id,initcwndbps,ip,ipbits,itag,lmt,mm,ms,mv,requiressl,source,upn,expire&expire=1410422182&key=yt5&ipbits=8&initcwndbps=242000&source=youtube&sver=3&ip=59.89.122.14&mm=30&itag=248&mv=u&upn=aFG9OizaVII&mt=1410400430&signature=5F67B124D8665D3514169EFDC4A429C6E5946834.C9DC4D869734C50942C607E4284FE753979047AB&ms=nxu&fexp=912108,914090,927622,931983,932404,932623,934024,934030,934922,946023,947209,952700,953724,953801,954806&id=o-AMpbNcMgbJPC552qYtvI0ZbaSqdqhP0VICLl2QeVKQPV");
            //generateSelectionFlyout();
        }

        public void cancelSelection()
        {
            SelectionGridViewVisible = Visibility.Collapsed;
            WebViewVisible = Visibility.Visible;
        }

        public async void selectPath()
        {
            picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".aac");
            picker.FileTypeFilter.Add(".wma");
            folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", folder);
                StartDownload();
                SelectionFlyout = Visibility.Collapsed;
                SelectionGridViewVisible = Visibility.Collapsed;
                WebViewVisible = Visibility.Visible;
            }
            else
            {
                Helper.HelperMethods.MessageUser("You did not select a folder. Select again");
                SelectionFlyout = Visibility.Collapsed;
                SelectionGridViewVisible = Visibility.Visible;
                gridView.SelectedIndex = -1;
            }
        }

        public async void defaultPath()
        {
            if (Helper.HelperMethods.defaultPathExists())
            {
                try
                {
                    folder = await StorageFolder.GetFolderFromPathAsync((string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["defaultPath"]);
                }
                catch
                {
                    Helper.HelperMethods.MessageUser("Something went wrong, Please try again");
                    gridView.SelectedIndex = -1;
                    SelectionFlyout = Visibility.Collapsed;
                    SelectionGridViewVisible = Visibility.Visible;
                    
                    //Frame.Navigate(typeof(MainPage));
                }
                StartDownload();
                SelectionFlyout = Visibility.Collapsed;
                SelectionGridViewVisible = Visibility.Collapsed;
                WebViewVisible = Visibility.Visible;
            }
            else
            {
                await Helper.HelperMethods.MessageUser("No default Folder selected...");
                SelectionFlyout = Visibility.Collapsed;
                gridView.SelectedIndex = -1;
                SelectionGridViewVisible = Visibility.Visible;
                //Frame.Navigate(typeof(MainPage));
            }
        }

        async void StartDownload()
        {
            try
            {
                //file = await folder.CreateFileAsync(video.Title+video.VideoExtension, CreationCollisionOption.GenerateUniqueName);
                file = await folder.CreateFileAsync(youtubeVideo.VideoTitle + "." + youtubeVideo.Extension, CreationCollisionOption.GenerateUniqueName);
            }
            catch (Exception ex)
            {
            }

            //BackgroundTransferGroup videoGroup = BackgroundTransferGroup.CreateGroup("video");

            BackgroundDownloader downloader = new BackgroundDownloader();
            //downloader.TransferGroup = videoGroup;

            

            string successToastText = "One of your VIDEO downloads has completed";
            string failureToastText = "One of your VIDEO downloads has failed";
            XmlDocument successToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            successToastXml.GetElementsByTagName("text").Item(0).InnerText = successToastText;
            ToastNotification successToast = new ToastNotification(successToastXml);
            downloader.SuccessToastNotification = successToast;


            XmlDocument failureToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            failureToastXml.GetElementsByTagName("text").Item(0).InnerText = failureToastText;
            ToastNotification failureToast = new ToastNotification(failureToastXml);
            downloader.FailureToastNotification = failureToast;

            DownloadOperation download = downloader.CreateDownload(source, file);
            download.CostPolicy = BackgroundTransferCostPolicy.Always;

            List<DownloadOperation> requestOperations = new List<DownloadOperation>();
            requestOperations.Add(download);
            UnconstrainedTransferRequestResult result;
            try
            {
                result = await BackgroundDownloader.RequestUnconstrainedDownloadsAsync(requestOperations);
            }
            catch (NotImplementedException)
            {
                return;
            }

            //await HandleDownloadAsync(download, true);

            await Helper.HelperMethods.MessageUser("Your download has been started...");
            //gridView = null;
            
            //Frame.Navigate((typeof(MainPage)));

            await HandleDownloadAsync(download, true);

            //await Task.WhenAll(t);
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                //Log("Running: " + download.Guid);

                // Store the download so we can pause/resume.
                activeDownloads.Add(download);

                progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (start)
                {
                    // Start the download and attach a progress handler.
                    await download.StartAsync().AsTask(cts.Token, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler.
                    await download.AttachAsync().AsTask(cts.Token, progressCallback);
                }
            }
            catch (TaskCanceledException)
            {
                //HelperMethods.HelperMethods.MessageUser("Downloads Cancelled", topStack);
                //Frame.Navigate(typeof(ActiveDownloads));
            }
            catch (Exception ex)
            {
                // Helper.HelperMethods.MessageUser("Something went wrong. Please try again");
            }

            finally
            {
                activeDownloads.Remove(download);
            }


        }

        private void DownloadProgress(DownloadOperation download)
        {
            //double percent = 0;
            //if (download.Progress.TotalBytesToReceive > 0)
            //{
            //    percent = download.Progress.BytesReceived * 100 / download.Progress.TotalBytesToReceive;
            //    try
            //    {
            //        if (percent == 100 && !App.allDownloads.Containers.ContainsKey(download.ResultFile.Name))
            //            App.allDownloads.Values.Add(new KeyValuePair<string, object>(download.ResultFile.Name, download.ResultFile.Path));
            //    }
            //    catch
            //    { }
            //}
        }
    }
}
