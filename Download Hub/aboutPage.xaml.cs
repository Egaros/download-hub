using Microsoft.AdMediator.Core.Models;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Download_Hub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class aboutPage : Page
    {
        public aboutPage()
        {
            this.InitializeComponent();
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
            if(Frame.CurrentSourcePageType!=typeof(aboutPage))
            Frame.Navigate(typeof(aboutPage));
        }
    }
}
