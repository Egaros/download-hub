using System;
using System.Threading.Tasks;
using ViewModel;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Download_Hub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class activeDownloadSelection : Page
    {
        DownloadOperation download;
        ActiveDownloadSelectionViewModel viewModel;
        bool flag;
        public activeDownloadSelection()
        {
            this.InitializeComponent();
            viewModel = (ActiveDownloadSelectionViewModel)this.Resources["viewModel"];
            Loaded += ActiveDownloadSelection_Loaded;         
        }

        private async void ActiveDownloadSelection_Loaded(object sender, RoutedEventArgs e)
        {
            do
            {
                flag = viewModel.checkstatus(download);
                await Task.Delay(3000);
            } while (flag == true);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            base.OnNavigatedTo(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            download = (DownloadOperation)e.Parameter;
            nameTextBlock.Text = download.ResultFile.Name;          
            
        }

        private void downloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
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

            Frame.Navigate(typeof(MainPage));

        }

        private void aboutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(aboutPage));
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.cancel(download);
            Frame.Navigate(typeof(ActiveDownloads));
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.pause(download);
        }

        private void resumeButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.resume(download);
        }

    }
}
