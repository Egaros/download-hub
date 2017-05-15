using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;

namespace ViewModel
{
    public class ActiveDownloadSelectionViewModel : INotifyPropertyChanged
    {
        private bool _resumeEnabled = false;
        private bool _pauseEnabled = true;
        private bool _cancelEnabled = true;
        //private double _progressValue = 0.0;
        private string _status = "";
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
        public bool ResumeEnabled
        {
            get
            {
                return _resumeEnabled;
            }
            set
            {
                _resumeEnabled = value;
                NotifyPropertyChanged();
            }
        }

        //public double ProgressValue
        //{
        //    get
        //    {
        //        return _progressValue;
        //    }
        //    set
        //    {
        //        _progressValue = value;
        //        NotifyPropertyChanged();
        //    }
        //}

        public bool CancelEnabled
        {
            get
            {
                return _cancelEnabled;
            }
            set
            {
                _cancelEnabled = value;
                NotifyPropertyChanged();
            }
        }
        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }
        public bool PauseEnabled
        {
            get
            {
                return _pauseEnabled;
            }
            set
            {
                _pauseEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public void pause(DownloadOperation download)
        {
            try
            {
                download.Pause();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                PauseEnabled = false;
                ResumeEnabled = true;
                Status = download.Progress.Status.ToString();
            }
            
            
        }

        public void resume(DownloadOperation download)
        {
            try
            {
                download.Resume();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                PauseEnabled = true;
                ResumeEnabled = false;
                Status = download.Progress.Status.ToString();
            }           
            
        }

        public void cancel(DownloadOperation download)
        {
            download.AttachAsync().Cancel();
        }

        public bool checkstatus(DownloadOperation download)
        {
            bool flag = true;
            if (download.Progress.Status == BackgroundTransferStatus.Completed)
            {
                Status = "Completed";
                PauseEnabled = false;
                ResumeEnabled = false;
                CancelEnabled = false;
                flag = false;
            }
            else if (download.Progress.Status == BackgroundTransferStatus.PausedByApplication)
            {
                Status = "Paused By Application";
                PauseEnabled = false;
                ResumeEnabled = true;
            }
            else if (download.Progress.Status == BackgroundTransferStatus.Running)
            {
                Status = "Running";
                PauseEnabled = true;
                ResumeEnabled = false;
            }
            else if (download.Progress.Status == BackgroundTransferStatus.PausedCostedNetwork || download.Progress.Status == BackgroundTransferStatus.PausedNoNetwork || download.Progress.Status == BackgroundTransferStatus.PausedSystemPolicy)
            {
                Status = "Paused by System";
                PauseEnabled = true;
                ResumeEnabled = false;
            }
            return flag;
        }
    }
}
