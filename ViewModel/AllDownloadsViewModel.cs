using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ViewModel
{
    public class AllDownloadsViewModel : INotifyPropertyChanged
    {
        private List<Helper.AllDownloads.CompletedDownload> _list = new List<Helper.AllDownloads.CompletedDownload>();
        private Visibility _listVisible = Visibility.Collapsed;
        private Visibility _nothingToShowVisible = Visibility.Collapsed;
        //private ApplicationDataContainer _allDownloads;
        public event PropertyChangedEventHandler PropertyChanged;

        public AllDownloadsViewModel()
        {
            //if (Windows.Storage.ApplicationData.Current.LocalSettings.Containers.ContainsKey("allDownloads"))
            //    _allDownloads = Windows.Storage.ApplicationData.Current.LocalSettings.Containers["allDownloads"];
            //else
            //    _allDownloads = Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer("allDownloads", ApplicationDataCreateDisposition.Always);
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public ApplicationDataContainer AllDownloads
        //{
        //    get
        //    {
        //        return _allDownloads;
        //    }
        //    set
        //    {
        //        _allDownloads = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        //private Dictionary<string, string> _downloadHistory=new Dictionary<string, string>();

        public Visibility ListVisible
        {
            get
            {
                if (_list.Count == 0)
                    _listVisible = Visibility.Collapsed;
                else
                    _listVisible = Visibility.Visible;
                return _listVisible;
            }
            set
            {
                _listVisible = value;
                NotifyPropertyChanged();
            }

        }

        public Visibility NothingToShowVisible
        {
            get
            {
                if (_list.Count == 0)
                    _nothingToShowVisible = Visibility.Visible;
                else
                    _nothingToShowVisible = Visibility.Collapsed;
                return _nothingToShowVisible;
            }
            set
            {
                _nothingToShowVisible = value;
                NotifyPropertyChanged();
            }

        }

        public List<Helper.AllDownloads.CompletedDownload> DownloadList
        {
            get
            {
                return _list;
            }
            set
            {
                _list = value;
                NotifyPropertyChanged();
            }

        }
        //public Dictionary<string, string> downloadHistory
        //{
        //    get
        //    {
        //        return _downloadHistory;
        //    }
        //    set
        //    {
        //        _downloadHistory = value;
        //        NotifyPropertyChanged();
        //    }
        //}


        //public void FillAllDownloadsList()
        //{
        //    foreach (var value in AllDownloads.Values)
        //    {
        //        CreateCollection(value.Key, value.Value.ToString());
        //    }
        //}

        public void LoadData(SQLitePCL.SQLiteConnection conn)
        {
            DownloadList = Helper.HelperMethods.getData(conn);
        }

        public void deleteEntry(SQLitePCL.SQLiteConnection conn, string name)
        {
            Helper.HelperMethods.removeData(conn, name);
        }
        //private void CreateCollection(string a, string b)
        //{

        //    downloadHistory.Add(a, b);


        //    //StackPanel panel = new StackPanel();
        //    //panel.Margin = new Thickness(0, 5, 0, 5);
        //    //TextBlock block1 = new TextBlock();
        //    //block1.Name = "first";
        //    //block1.TextWrapping = TextWrapping.Wrap;
        //    //block1.FontSize = 20;
        //    //block1.Text = a;
        //    //TextBlock block2 = new TextBlock();
        //    //block2.TextWrapping = TextWrapping.Wrap;
        //    //block2.Text = b;
        //    //panel.Children.Add(block1);
        //    //panel.Children.Add(block2);

        //    //list.Items.Add(panel);

        //}
    }
}
