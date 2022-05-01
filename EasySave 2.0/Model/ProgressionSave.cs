using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EasySave_2._0
{
     
    /// Class used to create object that store information during a save protocol (complete or differencial)
     
    class ProgressionSave : INotifyPropertyChanged
    {
         
        /// Contructor, create the object from simple parameters
         
        /// <param name="_totalFilesNumber">Total file number to save</param>
        /// <param name="_totalSize">Total size to save</param>
        /// <param name="_filesRemaining">Total file remaining</param>
        /// <param name="_progressState">Percentage of progress of the save</param>
        /// <param name="_sizeRemaining">Total size remaining</param>
        public ProgressionSave(int _totalFilesNumber, long _totalSize, int _filesRemaining, long _progressState, long _sizeRemaining)
        {
            //Enter the current time at the creation of the object
            LaunchTime = DateTime.Now.ToString();
            TotalFilesNumber = _totalFilesNumber;
            TotalSize = _totalSize;
            FilesRemaining = _filesRemaining;
            ProgressState = _progressState;
            SizeRemaining = _sizeRemaining;
            CurrentDestinationFilePath = null;
            CurrentSourceFilePath = null;
            IsPaused = false;
            Cancelled = false;
            IsEncrypting = false;
        }

         
        /// Thread friendly object for oversized files
         
        public static object taken = new object();

        private bool isPaused;

        //Time at the launch of the save protocol
        private string launchTime;

        /// Save launch time

        public string LaunchTime
        {
            get { return launchTime; }
            set
            {
                launchTime = value;
                OnPropertyChanged("LaunchTime");
            }
        }
        /// Pause parameter

        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        private bool cancelled;
         
        /// Cancel parameter (if true : exit the save asap)
         
        public bool Cancelled
        {
            get { return cancelled; }
            set { cancelled = value; }
        }

        //Total file to save
        private int totalFilesNumber;
         
        /// Total file number to save
         
        public int TotalFilesNumber
        {
            get { return totalFilesNumber; }
            set 
            { 
                totalFilesNumber = value;
                OnPropertyChanged("TotalFilesNumber");
            }
        }

        //Total size to copy
        private long totalSize;
         
        /// Total file size to save
         
        public long TotalSize
        {
            get { return totalSize; }
            set 
            {
                totalSize = value;
                OnPropertyChanged("TotalSize");
            }
        }

        //Percent of progress in the save protocol
        private double progressState;

        /// Percentage of progress

        public double ProgressState
        {
            get { return progressState; }
            set
            {
                progressState = value;
                OnPropertyChanged("ProgressState");
            }
        }

        //Total file remaining to save
        private int filesRemaining;
         
        /// Files remaining to save
         
        public int FilesRemaining
        {
            get { return filesRemaining; }
            set
            { 
                filesRemaining = value;
                OnPropertyChanged("FilesRemaining");
            }
        }


        //Size remaining to save
        private long sizeRemaining;
         
        /// Size remaining to save
         
        public long SizeRemaining
        {
            get { return sizeRemaining; }
            set 
            { 
                sizeRemaining = value;
                OnPropertyChanged("SizeRemaining");
            }
        }

        //Source path of the current file we need to save
        private string currentSourceFilePath;
         
        /// Current file source path to saving
         
        public string CurrentSourceFilePath
        {
            get { return currentSourceFilePath; }
            set 
            { 
                currentSourceFilePath = value;
                OnPropertyChanged("CurrentSourceFilePath");
            }
        }

        //Target path of the current file we need to save
        private string currentDestinationFilePath;
         
        /// Current file destination path to saving
         
        public string CurrentDestinationFilePath
        {
            get { return currentDestinationFilePath; }
            set
            { 
                currentDestinationFilePath = value;
                OnPropertyChanged("CurrentDestinationFilePath");
            }
        }

        private bool isEncrypting;
         
        /// Encryption process parameter
         
        public bool IsEncrypting
        {
            get { return isEncrypting; }
            set 
            { 
                isEncrypting = value;
                OnPropertyChanged("IsEncrypting");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        /// Update the progress state

        public void UpdateProgressState()
        {
            double sizeDifference = TotalSize - SizeRemaining;

            //Check if the difference in size is equal to 0, to avoid division by 0
            if (sizeDifference != 0)
            {
                ProgressState = sizeDifference / TotalSize * 100;
            }
            Model.OnProgressUpdate();
        }

    }
}
