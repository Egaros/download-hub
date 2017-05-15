using System;
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
    /// 

    public sealed partial class music : Page
    {
        MusicViewModel viewModel;

        public music()
        {
            this.InitializeComponent();
            viewModel = (MusicViewModel)this.Resources["viewModel"];
            //this.SizeChanged += music_SizeChanged;
        }

  

        //private void music_SizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    if (e.NewSize.Width < new Windows.Foundation.Size(800, e.NewSize.Height).Width
        //        && e.NewSize.Width > new Windows.Foundation.Size(600, e.NewSize.Height).Width)
        //    {
        //        AdMediator_75365E.Disable();
        //        AdMediator_8932E1.Resume();
        //    }
        //    else if (e.NewSize.Width < new Windows.Foundation.Size(600, e.NewSize.Height).Width)
        //    {
        //        AdMediator_8932E1.Disable();
        //        AdMediator_75365E.Disable();
        //    }
        //    else if (e.NewSize.Width > new Windows.Foundation.Size(800, e.NewSize.Height).Width)
        //    {
        //        AdMediator_75365E.Resume();
        //        AdMediator_8932E1.Resume();
        //    }
        //}

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Frame.BackStack.Clear();
            base.OnNavigatedTo(e);
            GC.Collect();
            GC.WaitForPendingFinalizers();


            viewModel.bindTopResults();
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
            if (Frame.CurrentSourcePageType != typeof(MainPage))
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void aboutAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(aboutPage));
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.search();
        }

        private void songsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.selectionChanged(sender,e);
        }

        void selectPathButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.selectPath();            
        }

        void defaultPathButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.defaultPath();
        }

        private void topSongsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewModel.searchTopSong(sender, e);
        }
    }
}
