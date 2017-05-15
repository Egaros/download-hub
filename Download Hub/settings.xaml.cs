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
    public sealed partial class settings : Page
    {
        SettingsViewModel viewModel;
        public settings()
        {
            this.InitializeComponent();
            viewModel = (SettingsViewModel)this.Resources["viewModel"];
        }

  

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

        private void settingsAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CurrentSourcePageType != typeof(settings))
            {
                Frame.Navigate(typeof(settings));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.PickFolder();            
        }
    }
}
