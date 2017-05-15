using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using ViewModel;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Download_Hub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class allDownloads : Page
    {
        AllDownloadsViewModel viewModel;
        public allDownloads()
        {
            this.InitializeComponent();
            viewModel = (AllDownloadsViewModel)this.Resources["viewModel"];
        }

        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            viewModel.LoadData(App.conn);
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
            if (Frame.CurrentSourcePageType != typeof(aboutPage))
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

        

        private async void list_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var _list = sender as ListView;
            var item = _list.SelectedItem as Helper.AllDownloads.CompletedDownload;
            //var textBlock = (TextBlock)panel.Children.ElementAt(0);
            viewModel.deleteEntry(App.conn,item.fileName);
            viewModel.LoadData(App.conn);
        }
    }
}
