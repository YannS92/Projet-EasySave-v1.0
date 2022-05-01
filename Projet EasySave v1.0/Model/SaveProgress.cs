using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_EasySave_v1._0
{

    //Class used to create object that store information during a save protocol (complete or differencial)
    class ProgressionSave
    {
        //Contructor, create the object from simple parameters
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
        }

        //Time at the launch of the save protocol
        private string launchTime;

        public string LaunchTime
        {
            get { return launchTime; }
            set { launchTime = value; }
        }

        //Total file to save
        private int totalFilesNumber;

        public int TotalFilesNumber
        {
            get { return totalFilesNumber; }
            set { totalFilesNumber = value; }
        }

        //Total size to copy
        private long totalSize;

        public long TotalSize
        {
            get { return totalSize; }
            set { totalSize = value; }
        }

        //Total file remaining to save
        private int filesRemaining;

        public int FilesRemaining
        {
            get { return filesRemaining; }
            set { filesRemaining = value; }
        }

        //Percent of progress in the save protocol
        private long progressState;

        public long ProgressState
        {
            get { return progressState; }
            set { progressState = value; }
        }

        //Size remaining to save
        private long sizeRemaining;

        public long SizeRemaining
        {
            get { return sizeRemaining; }
            set { sizeRemaining = value; }
        }

        //Source path of the current file we need to save
        private string currentSourceFilePath;

        public string CurrentSourceFilePath
        {
            get { return currentSourceFilePath; }
            set { currentSourceFilePath = value; }
        }

        //Target path of the current file we need to save
        private string currentDestinationFilePath;

        public string CurrentDestinationFilePath
        {
            get { return currentDestinationFilePath; }
            set { currentDestinationFilePath = value; }
        }
    }
}
