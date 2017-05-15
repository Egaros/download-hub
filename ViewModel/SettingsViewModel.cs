using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace ViewModel
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        public static ApplicationDataContainer appSettings;
        StorageFolder folder;
        FolderPicker picker;
        private string _defaultFolder = "";

        public SettingsViewModel()
        {
            appSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string DefaultFolder
        {
            get
            {
                if (appSettings.Values.ContainsKey("defaultPath"))
                {
                    return _defaultFolder = (string)appSettings.Values["defaultPath"];
                }
                else
                {
                    return _defaultFolder = "No Path Selected";
                }

            }
            set
            {
                _defaultFolder = value;
                NotifyPropertyChanged();
            }

        }

        public async void PickFolder()
        {
            picker = new FolderPicker();
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wma");
            folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                appSettings.Values["defaultPath"] = folder.Path.ToString();
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                DefaultFolder = folder.Path;
            }
        }
    }
}
