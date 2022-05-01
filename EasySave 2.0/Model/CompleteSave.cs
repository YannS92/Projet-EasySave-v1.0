using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace EasySave_2._0
{  
    /// Complete Save Class 
    class CompleteSave : InformationSaveWork, INotifyPropertyChanged
    {

        private string name;
        /// Name
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        private int index;
        /// Index
        public int Index
        {
            get { return index; }
            set 
            { 
                index = value;
                OnPropertyChanged("Index");
            }
        }

        private string sourcePath;
        ///Source Path
        public string SourcePath
        {
            get { return sourcePath; }
            set 
            { 
                sourcePath = value;
                OnPropertyChanged("SourcePath");
            }
        }

        private string destinationPath;
        ///target path
        public string DestinationPath
        {
            get { return destinationPath; }
            set 
            {
                destinationPath = value;
                OnPropertyChanged("DestinationPath");
            }
        }

        private List<Extension> extentionToEncryptList;
        ///Extension List
        public List<Extension> ExtentionToEncryptList
        {
            get { return extentionToEncryptList; }
            set 
            {
                extentionToEncryptList = value;
                OnPropertyChanged("ExtensionToEncryptList");
            }
        }

        private SaveWorkType type;
        /// Type of Save (complete or differential)
        public SaveWorkType Type
        {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        private string creationTime;
        /// Creation time of the save 
        public string CreationTime
        {
            get { return creationTime; }
            set
            { 
                creationTime = value;
                OnPropertyChanged("CreationTime");
            }
        }

        private ProgressionSave progress;
        /// Progression of the save
        public ProgressionSave Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private bool isActive;
        /// Boolean true if the save is active, false if not active
        public bool IsActive
        {
            get { return isActive; }
            set
            { 
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        
        /// Complete Save constructor

        /// <param name="NameProcedure">Save Work Name</param>
        /// <param name="_source">Directory Source Path</param>
        /// <param name="_target">Directory Target Destination Path</param>
        /// <param name="_extension">Extension List to Encrypt</param>
        /// <param name="_type">Save Work Type</param>
        public CompleteSave(string NameProcedure, string _source, string _target, List<Extension> _extension, SaveWorkType _type)
        {
            Name = NameProcedure;
            SourcePath = _source;
            DestinationPath = _target;
            type = _type;
            ExtentionToEncryptList = _extension;
            CreationTime = DateTime.Now.ToString();
            IsActive = false;
            ///Progress = null;
        }

        /// Launch the saving process
        /// <param name="obj">Thread friendly object</param>
        public void Save(object obj)
        {
            Log.StartSaveLogLine(this);
            Log.LaunchingSaveLogLine(Index);
            CompleteCopy();
        }

        /// Create a new save progress object, that store information of the running save

        /// <param name="_totalFilesNumber">Number of files in the directory to save (total number)</param>
        /// <param name="_totalSize">Total size (in Bytes) of the directory</param>
        /// <param name="_filesRemaining">Number of file(s) remaining to save</param>
        /// <param name="_progressState">The progress percentage of the save (0-100)</param>
        /// <param name="_sizeRemaining">Size remaining (in Bytes) to save</param>
        public void CreateProgress(int _totalFilesNumber, long _totalSize, int _filesRemaining, int _progressState, long _sizeRemaining)
        {
            Progress = new ProgressionSave(_totalFilesNumber, _totalSize, _filesRemaining, _progressState, _sizeRemaining);
        }

        // Delete ProgressionSave object when the saving protocol stops
        public void DeleteProgress()
        {
            Progress = null;
        }

         
        /// Do a complete copy from a folder to another
         
        private void CompleteCopy()
        {
            if (Directory.Exists(SourcePath))
            {
                //The Source directory has succesfully been found

                //Search directory info from source and target path
                var diSource = new DirectoryInfo(SourcePath);
                var diTarget = new DirectoryInfo(DestinationPath);

                //Calculate the number of file in the source directory and the total size of it
                int nbFiles = Information.CompleteFilesNumber(diSource);
                long directorySize = Information.CompleteSize(diSource);

                Log.FileToSaveFound(this, nbFiles, diSource, directorySize);

                lock (Model.sync)
                {
                    CreateProgress(nbFiles, directorySize, nbFiles, 0, directorySize);
                    IsActive = true;
                }
                Model.OnSaveWorkUpdate();

                //initiate Copy from the source directory to the target directory
                Log.StartCopy(this);
                CompleteCopyAll(diSource, diTarget);


                lock (Model.sync)
                {
                    Progress.IsEncrypting = true;
                }
                Model.OnSaveWorkUpdate();

                //Start encryption file
                Log.StartEncryption(Index);
                EncryptFiles();
                Log.EndEncryption(Index);

                //Closing the complete save protocol
                lock (Model.sync)
                {
                    //DeleteProgress();
                    Progress.IsEncrypting = false;
                    IsActive = false;
                }
                Model.OnSaveWorkUpdate();

                Log.EndSaveProgram(Index);
            }
            else
            {
                //The Source Directory has not been found
                Model.OnUpdateModelError("directory");
            }
        }

         
        /// Copy each files from a directory and do the same for each subdirectory using recursion
         
        /// <param name="_nb">Index of the save work</param>
        /// <param name="_source">source directory path</param>
        /// <param name="_target">target destination directory path</param>
        private void CompleteCopyAll(DirectoryInfo _source, DirectoryInfo _target)
        {
            //Check of the different parameter that can stop or cancel the work (parameters stored in the Progress Update)
            if (Progress.Cancelled) return;
            bool softwareIsLaunched = false;
            while(Progress.IsPaused || Information.CheckIfSoftwareIsLaunched(Setting.softwareString))
            {
                if (Progress.Cancelled) return;
                if (Information.CheckIfSoftwareIsLaunched(Setting.softwareString) && !softwareIsLaunched)
                {
                    Model.OnUpdateModelError("software");
                    softwareIsLaunched = true;
                }
            }
            if (softwareIsLaunched) Model.OnUpdateModelError("resume");

            //First create the new target directory where all the files are saved later on
            Log.CreateDirectoryLogLine(this, _target);
            Directory.CreateDirectory(_target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in _source.GetFiles())
            {
                lock (Model.sync)
                {
                    Progress.CurrentSourceFilePath = fi.FullName;
                    Progress.CurrentDestinationFilePath = Path.Combine(_target.FullName, fi.Name);
                }
                Model.OnSaveWorkUpdate();

                string elapsedTime = "";

                if(fi.Length >= Setting.maxTransferSize)
                {
                    lock (ProgressionSave.taken)
                    {
                        Log.StartCopyFileLogLine(this, fi);

                        //Copy the file and measure execution time
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        fi.CopyTo(Path.Combine(_target.FullName, fi.Name), true);
                        watch.Stop();
                        elapsedTime = watch.Elapsed.TotalSeconds.ToString();
                    }
                }
                else
                {
                    Log.StartCopyFileLogLine(this, fi);

                    //Copy the file and measure execution time
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    fi.CopyTo(Path.Combine(_target.FullName, fi.Name), true);
                    watch.Stop();
                    elapsedTime = watch.Elapsed.TotalSeconds.ToString();
                }

                


                lock (Model.sync)
                {
                    Progress.FilesRemaining--;
                    Progress.SizeRemaining -= fi.Length;
                    Progress.UpdateProgressState();
                }
                
                Model.OnSaveWorkUpdate();
                Log.FinishCopyFileLogLine(this, fi, elapsedTime);

                //Check of the different parameter that can stop or cancel the work (parameters stored in the Progress Update)
                if (Progress.Cancelled) return;
                softwareIsLaunched = false;
                while (Progress.IsPaused || Information.CheckIfSoftwareIsLaunched(Setting.softwareString))
                {
                    if (Progress.Cancelled) return;
                    if (Information.CheckIfSoftwareIsLaunched(Setting.softwareString) && !softwareIsLaunched)
                    {
                        Model.OnUpdateModelError("software");
                        softwareIsLaunched = true;
                    }
                }
                if (softwareIsLaunched) Model.OnUpdateModelError("resume");
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in _source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    _target.CreateSubdirectory(diSourceSubDir.Name);
                Log.EnterSubdirectoryLogLine(this, diSourceSubDir);
                CompleteCopyAll(diSourceSubDir, nextTargetSubDir);
                Log.ExitSubdirectoryLogLine(this, diSourceSubDir);
            }
        }



         
        /// Encrypt selected files 
         
        public void EncryptFiles()
        {
            if (Progress.Cancelled) return;
            if (ExtentionToEncryptList != null && Directory.Exists(DestinationPath))
            {
                // If we encrypt all files
                if (extentionToEncryptList.Contains(Extension.ALL))
                {
                    // Find all files
                    string[] filesPathToEncrypt = Directory.GetFiles(destinationPath, "*.*", SearchOption.AllDirectories);
                    // For each files
                    foreach (string files in filesPathToEncrypt)
                    {
                        if (Progress.Cancelled) return;
                        //Console.WriteLine(files);
                        // Encrypt File
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        CryptoSoft.CryptoSoftTools.CryptoSoftEncryption(files);
                        watch.Stop();

                        Log.EncryptedFile(this, files, watch.Elapsed.TotalSeconds.ToString());
                    }
                }

                // for each exntensions in the list
                foreach (Extension extension in extentionToEncryptList)
                {
                    // Adjusts the format of the extension
                    string extensionReformated = "*." + extension.ToString() + "*";
                    // Find all files in directory with aimed extensions
                    string[] filesPathToEncrypt = Directory.GetFiles(destinationPath, extensionReformated, SearchOption.AllDirectories);
                    // For each files with aimed extensions
                    foreach (string files in filesPathToEncrypt)
                    {
                        if (Progress.Cancelled) return;
                        //Console.WriteLine(files);
                        // Encrypt File
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        CryptoSoft.CryptoSoftTools.CryptoSoftEncryption(files);
                        watch.Stop();

                        Log.EncryptedFile(this, files, watch.Elapsed.TotalSeconds.ToString());
                    }
                }
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

    }
}
