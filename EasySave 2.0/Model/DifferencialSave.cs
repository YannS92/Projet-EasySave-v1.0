using Newtonsoft.Json;
using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace EasySave_2._0
{
    /// Differencial Save Work Class (implementing InformationSaveWork)
    class DifferentialSave : InformationSaveWork, INotifyPropertyChanged
    {
        private int index;
         
        /// Index of the save work
         
        public int Index
        {
            get { return index; }
            set 
            { 
                index = value;
                OnPropertyChanged("Index");
            }
        }

        private string name;
         
        /// Name of the save work
         
        public string Name
        {
            get { return name; }
            set 
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private string sourcePath;
         
        /// Directory Source Path to save
         
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
         
        /// Target Directory to save files in
         
        public string DestinationPath
        {
            get { return destinationPath; }
            set 
            {
                destinationPath = value;
                OnPropertyChanged("DestinationPath");
            }
        }


        private SaveWorkType type;
         
        /// Type of Save Work (complete or differential)
         
        public SaveWorkType Type
        {
            get { return type; }
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        private List<Extension> extentionToEncryptList;

        /// Extension List to encrypt after the save is done

        public List<Extension> ExtentionToEncryptList
        {
            get { return extentionToEncryptList; }
            set
            {
                extentionToEncryptList = value;
                OnPropertyChanged("ExtensionToEncryptList");
            }
        }

        private string creationTime;
         
        /// Creation time of the save work
         
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
         
        /// Progress object of the save work
         
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

        /// Boolean of the saving activity (true if the save is active, false if not)

        public bool IsActive
        {
            get { return isActive; }
            set
            {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        /// Differencial Save Work constructor

        /// <param name="NameProcedure">Save Work Name</param>
        /// <param name="_source">Directory Source Path</param>
        /// <param name="_target">Directory Target Destination Path</param>
        /// <param name="_extension">Extension List to Encrypt</param>
        /// <param name="_type">Save Work Type</param>
        public DifferentialSave(string NameProcedure, string _source, string _target, List<Extension> _extension, SaveWorkType _type)
        {
            Name = NameProcedure;
            SourcePath = _source;
            DestinationPath = _target;
            type = _type;
            CreationTime = DateTime.Now.ToString();
            ExtentionToEncryptList = _extension;
            IsActive = false;
            //Progress = null;
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

         
        /// Delete the ProgressionSave object when the saving protocol stops
         
        public void DeleteProgress()
        {
            Progress = null;
        }

         
        /// Launch the saving process
         
        /// <param name="obj">Thread friendly object</param>
        public void Save(object obj)
        {
            Log.StartSaveLogLine(this);
            Log.LaunchingSaveLogLine(Index);
            DifferencialCopy();
        }

         
        /// Do a differencial copy from a folder to another
         
        private void DifferencialCopy()
        {
            if (Directory.Exists(SourcePath))
            {
                //Search directory info from source and target path
                var diSource = new DirectoryInfo(SourcePath);
                var diTarget = new DirectoryInfo(DestinationPath);

                //Calculate the number of file in the source directory and the total size of it (of all )
                int nbFiles = Information.DifferencialFilesNumber(diSource, diTarget);
                long directorySize = Information.DifferencialSize(diSource, diTarget);

                //If there is at least one file to save then initiate the differencial saving protocol
                if (nbFiles != 0)
                {
                    Log.FileToSaveFound(this, nbFiles, diSource, directorySize);

                    lock (Model.sync)
                    {
                        CreateProgress(nbFiles, directorySize, nbFiles, 0, directorySize);
                        IsActive = true;
                    }

                    Model.OnSaveWorkUpdate();

                    //initiate Copy from the source directory to the target directory (only the file / directory that has been modified or are new)
                    Log.StartCopy(this);
                    DifferencialCopyAll(diSource, diTarget);


                    lock (Model.sync)
                    {
                        Progress.IsEncrypting = true;
                    }
                    Model.OnSaveWorkUpdate();

                    Log.StartEncryption(Index);
                    EncryptFiles();
                    Log.EndEncryption(Index);

                    lock (Model.sync)
                    {
                        //DeleteProgress();
                        Progress.IsEncrypting = false;
                        IsActive = false;
                    }
                    Model.OnSaveWorkUpdate();

                    Log.EndSaveProgram(Index);
                }
                //If there is no file to save then cancel the saving protocol
                else
                {
                    Log.NoFilesFound(Index);
                }
            }
            else
            {
                Model.OnUpdateModelError("directory");
            }
        }

         
        /// Copy each files (that has been modified since the last save) from a directory, and do the same for each subdirectory using recursion
         
        /// <param name="_nb">Index of the save work</param>
        /// <param name="_source">source directory path</param>
        /// <param name="_target">target destination directory path</param>
        private void DifferencialCopyAll(DirectoryInfo _source, DirectoryInfo _target)
        {

            if (Progress.Cancelled) return;
            bool softwareIsLaunched = false;
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

            Directory.CreateDirectory(_target.FullName);
            Log.CreateDirectoryLogLine(this, _target);

            // Copy each file into the new directory.
            foreach (FileInfo fi in _source.GetFiles())
            {
                //Calculate the path of the future file we need to save
                string targetPath = Path.Combine(_target.FullName, fi.Name);

                //Check if the file already exist or not (new one), and verify if it has been modified or not
                if (!File.Exists(targetPath) || fi.LastWriteTime != File.GetLastWriteTime(targetPath))
                {
                    lock (Model.sync)
                    {
                        Progress.CurrentSourceFilePath = fi.FullName;
                        Progress.CurrentDestinationFilePath = Path.Combine(_target.FullName, fi.Name);
                    }
                    Model.OnSaveWorkUpdate();
                    Log.StartCopyFileLogLine(this, fi);

                    string elapsedTime = "";

                    if (fi.Length >= Setting.maxTransferSize)
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


            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in _source.GetDirectories())
            {
                string targetDirectoryPath = Path.Combine(_target.FullName, diSourceSubDir.Name);
                Log.EnterSubdirectoryLogLine(this, diSourceSubDir);

                //Check if the directory already exist to decide if it is required to create a new one or not
                if (!Directory.Exists(targetDirectoryPath))
                {
                    DirectoryInfo nextTargetSubDir = _target.CreateSubdirectory(diSourceSubDir.Name);
                    DifferencialCopyAll(diSourceSubDir, nextTargetSubDir);
                }
                else
                {
                    DirectoryInfo nextTargetSubDir = new DirectoryInfo(targetDirectoryPath);
                    DifferencialCopyAll(diSourceSubDir, nextTargetSubDir);
                }

                Log.ExitSubdirectoryLogLine(this, diSourceSubDir);

            }
        }


         
        /// Encrypt selected files 
         
        public void EncryptFiles()
        {
            // If we encrypt all files
            if (extentionToEncryptList.Contains(Extension.ALL))
            {
                // Find all files
                string[] filesPathToEncrypt = Directory.GetFiles(destinationPath, "*.*", SearchOption.AllDirectories);
                // For each files
                foreach (string files in filesPathToEncrypt)
                {
                    Console.WriteLine(files);
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
                    Console.WriteLine(files);
                    // Encrypt File

                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    CryptoSoft.CryptoSoftTools.CryptoSoftEncryption(files);
                    watch.Stop();

                    Log.EncryptedFile(this, files, watch.Elapsed.TotalSeconds.ToString());
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
