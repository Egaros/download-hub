using AppCoreMusic;
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

namespace ViewModel
{
    public class MusicViewModel : INotifyPropertyChanged
    {
        List<DownloadOperation> activeDownloads = new List<DownloadOperation>();
        ListView listview;
        public static CancellationTokenSource cts;
        private Mp3SkullMusic musics;
        private StorageFile file;
        private Uri source;
        private FolderPicker picker;
        private StorageFolder folder;
        Progress<DownloadOperation> progressCallback;
        private string baseUrl = "http://mp3skull.com/mp3/";
        private string tempHtml;


        //properties
        private List<Mp3SkullMusic> _listOfMusic;
        private List<Mp3SkullMusic> _listOfTopMusic;
        private Visibility _nothingFound = Visibility.Collapsed;
        private Visibility _progressStack = Visibility.Collapsed;
        private Visibility _songsList = Visibility.Collapsed;
        private Visibility _topSongsList = Visibility.Collapsed;
        private bool _progressRing = false;
        private string _searchText = "";
        private Visibility _selectionFlyout = Visibility.Collapsed;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MusicViewModel()
        {
            cts = new CancellationTokenSource();
        }
        public Visibility NothingFound
        {
            get
            {
                return _nothingFound;
            }
            set
            {
                _nothingFound = value;
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

        public Visibility ProgressStack
        {
            get
            {
                return _progressStack;
            }
            set
            {
                _progressStack = value;
                NotifyPropertyChanged();
            }

        }

        public Visibility SongsList
        {
            get
            {
                return _songsList;
            }
            set
            {
                _songsList = value;
                NotifyPropertyChanged();
            }
        }

        public Visibility TopSongsList
        {
            get
            {
                return _topSongsList;
            }
            set
            {
                _topSongsList = value;
                NotifyPropertyChanged();
            }
        }

        public bool ProgressRing
        {
            get
            {
                return _progressRing;
            }
            set
            {
                _progressRing = value;
                NotifyPropertyChanged();
            }
        }

        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                _searchText = value;
                NotifyPropertyChanged();
            }

        }

        public List<Mp3SkullMusic> ListOfMusic
        {
            get
            {
                return _listOfMusic;
            }
            set
            {
                _listOfMusic = value;
                NotifyPropertyChanged();
            }

        }

        public List<Mp3SkullMusic> ListOfTopMusic
        {
            get
            {
                return _listOfTopMusic;
            }
            set
            {
                _listOfTopMusic = value;
                NotifyPropertyChanged();
            }

        }


        public async void search()
        {
            TopSongsList = Visibility.Collapsed;
            SongsList = Visibility.Collapsed;
            NothingFound = Visibility.Collapsed;
            SelectionFlyout = Visibility.Collapsed;
            
            if (_searchText == "")
            {
                Helper.HelperMethods.MessageUser("Enter the search phrase");
                
            }
            else
            {
                ProgressRing = true;
                ProgressStack = Visibility.Visible;
                _searchText = _searchText.ToLower();
                _searchText = _searchText.Replace(" ", "_");
                string url = baseUrl + _searchText + ".html";
                try
                {
                    tempHtml = await Mp3SkullExtract.DownloadHtml(url);
                }
                catch (Exception ex)
                {
                    Helper.HelperMethods.MessageUser(ex.Message);
                    //Frame.Navigate(typeof(music));
                }
                ListOfMusic = Mp3SkullExtract.GetListOfDownloads(tempHtml);
                if (_listOfMusic.Count == 0)
                {
                    ProgressRing = false;
                    ProgressStack = Visibility.Collapsed;
                    SongsList = Visibility.Collapsed;
                    NothingFound = Visibility.Visible;
                }
                else
                {
                    ProgressRing = false;
                    ProgressStack = Visibility.Collapsed;
                    NothingFound = Visibility.Collapsed;
                    SongsList = Visibility.Visible;
                }
            }
        }

        

        public async void bindTopResults()
        {
            ProgressRing = true;
            ProgressStack = Visibility.Visible;
            try
            {
                tempHtml = await Mp3SkullExtract.DownloadHtml("http://mp3skull.com/");
            }
            catch (Exception ex)
            {
                Helper.HelperMethods.MessageUser(ex.Message);
                //Frame.Navigate(typeof(music));
            }
            ListOfTopMusic = Mp3SkullExtract.GetListOfTopMusic(tempHtml);
            ProgressRing = false;
            ProgressStack = Visibility.Collapsed;
            NothingFound = Visibility.Collapsed;
            TopSongsList = Visibility.Visible;
        }

        public void selectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listview = sender as ListView;
            if (listview.SelectedIndex != -1)
            {
                musics = listview.SelectedItem as Mp3SkullMusic;
                musics.name = Helper.HelperMethods.normalizeName(musics.name);
                source = new Uri(musics.downloadLink);
                SongsList = Visibility.Collapsed;
                SelectionFlyout = Visibility.Visible;
            }

        }

        public void searchTopSong(object sender, SelectionChangedEventArgs e)
        {
            listview = sender as ListView;
            if (listview.SelectedIndex != -1)
            {
                musics = listview.SelectedItem as Mp3SkullMusic;
                SearchText = musics.name;
                TopSongsList = Visibility.Collapsed;
                search();
            }

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
                SearchText = "";
                listview.SelectedIndex = -1;
                SongsList = Visibility.Visible;
            }
            else
            {
                Helper.HelperMethods.MessageUser("You did not select a folder. Select again");
                SelectionFlyout = Visibility.Collapsed;
                SongsList = Visibility.Visible;
                listview.SelectedIndex = -1;
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
                    //Frame.Navigate(typeof(MainPage));
                }
                StartDownload();
                SelectionFlyout = Visibility.Collapsed;
                SearchText = "";
                listview.SelectedIndex = -1;
                SongsList = Visibility.Visible;
            }
            else
            {
                await Helper.HelperMethods.MessageUser("No default Folder selected...");
                SelectionFlyout = Visibility.Collapsed;
                listview.SelectedIndex = -1;
                SongsList = Visibility.Visible;
                //Frame.Navigate(typeof(MainPage));
            }
        }

        async void StartDownload()
        {
            try
            {
                file = await folder.CreateFileAsync(musics.name + ".mp3", CreationCollisionOption.GenerateUniqueName);
            }
            catch (Exception ex)
            {
               
            }

            //BackgroundTransferGroup musicGroup = BackgroundTransferGroup.CreateGroup("Music");

            BackgroundDownloader downloader = new BackgroundDownloader();
            //downloader.TransferGroup = musicGroup;

            string successToastText = "One of your MUSIC downloads has completed";
            string failureToastText = "One of your MUSIC downloads has failed";

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
