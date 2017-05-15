using System;
using System.Threading.Tasks;
using ViewModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Download_Hub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class videos : Page
    {
        VideosViewModel viewModel;
        string youtubeUrl = "";
        public videos()
        {
            this.InitializeComponent();
            //metacafeWebElement.NavigationCompleted += metacafeWebElement_NavigationCompleted;

            var api = "Windows.Phone.UI.Input.HardwareButtons";
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(api))
            {

                youtubeWebElement.FrameContentLoading += YoutubeWebElement_FrameContentLoading;
        }
            else
            {
                youtubeWebElement.FrameDOMContentLoaded += youtubeWebElement_FrameDOMContentLoaded;
            }
    viewModel = (VideosViewModel)this.Resources["viewModel"];
           
        }

 

        

        private void YoutubeWebElement_FrameContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            string temp = args.Uri.ToString();
            if (temp.Contains("/watch?v="))
            {
                youtubeUrl = temp;
            }
            Task.Delay(3000);
            if (youtubeUrl.Contains("/watch?v="))
                downloadAppBarButton.IsEnabled = true;
            else
                downloadAppBarButton.IsEnabled = false;
        }

        
        //void metacafeWebElement_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        //{

        //    //metacafeUrl = metacafeWebElement.Source.ToString();
        //    //if (metacafeUrl.Contains("www.metacafe.com/watch/"))
        //    //    downloadAppBarButton.IsEnabled = true;
        //    //else
        //    //    downloadAppBarButton.IsEnabled = false;
        //}

        void youtubeWebElement_FrameDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            //viewModel.checkForUrl(args);
            string temp = args.Uri.ToString();
            if (temp.Contains("/watch?v="))
            {
                youtubeUrl = temp;
            }
            Task.Delay(3000);
            if (youtubeUrl.Contains("/watch?v="))
                downloadAppBarButton.IsEnabled = true;
            else
                downloadAppBarButton.IsEnabled = false;


            //Task.Delay(3000);
            //youtubeUrl = sender.Source.ToString();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            //metacafeWebElement.Navigate(new Uri("http://www.metacafe.com/"));

            youtubeWebElement.Navigate(new Uri("http://www.youtube.com/"));
            

            //e.Uri = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            base.OnNavigatedFrom(e);

            var api = "Windows.Phone.UI.Input.HardwareButtons";
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(api))
            {

                youtubeWebElement.FrameContentLoading -= YoutubeWebElement_FrameContentLoading;
            }
            else
            {
                youtubeWebElement.FrameDOMContentLoaded -= youtubeWebElement_FrameDOMContentLoaded;
            }
            viewModel = null;
            youtubeWebElement.NavigationCompleted -= youtubeWebElement_NavigationCompleted;
            youtubeWebElement.NavigationStarting -= youtubeWebElement_NavigationStarting;
            youtubeWebElement.NavigationFailed -= youtubeWebElement_NavigationFailed;
            viewModel = null;
            webElementGrid = null;
            youtubeWebElement = null;

            //e.Uri = null;
            youtubeWebElement = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void homeAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            youtubeWebElement.Stop();
            if(youtubeWebElement.CanGoBack)
            youtubeWebElement.GoBack();
            Frame.Navigate(typeof(MainPage));
        }

        private void downloadAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.downloadClick(youtubeWebElement, youtubeUrl);
            youtubeUrl = "";
            downloadAppBarButton.IsEnabled = false;
        }

        private void backAppBarButton_Click(object sender, RoutedEventArgs e)
        {
                if (youtubeWebElement.CanGoBack)
                {
                    youtubeWebElement.GoBack();
                }
        }

        private void view_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.selectionChanged(sender);
        }

        private void defaultFolder_Click(object sender, RoutedEventArgs e)
        {
            viewModel.defaultPath();
        }

        private void selectFolder_Click(object sender, RoutedEventArgs e)
        {
            viewModel.selectPath();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.cancelSelection();
        }

        private async void youtubeWebElement_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            await Helper.HelperMethods.MessageUser("Please connect to the internet and try again");
            Frame.Navigate(typeof(MainPage));
        }

        private void youtubeWebElement_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            pageLoadingRing.IsActive = true;
        }

        private void youtubeWebElement_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            pageLoadingRing.IsActive = false;
            pageLoadingStack.Visibility = Visibility.Collapsed;
        }
    }
}
