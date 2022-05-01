using Newtonsoft.Json;
using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EasySave_2._0
{

    public delegate void SaveWorkUpdateDelegate();

    public delegate void UpdateGlobalProgress();

    public delegate void UpdateModelError(string _errorString);

    public delegate void ResumeSaveDelegate(int _nb);

    public delegate void PauseSaveDelegate(int _nb);

    public delegate void CancelSaveDelegate(int _nb);


     
    /// Program data model class
     
    class Model : INotifyPropertyChanged
    {

         
        /// Model Class constructor, initiate Save Work in a list (no parameters)
         
        public Model()
        {
            //Asign the delegate
            OnSaveWorkUpdate = UpdateSaveFile;
            OnProgressUpdate = UpdateAllSaveProgress;
            OnUpdateModelError = SetModelError;
            OnSocketResumeSave = ResumeSave;
            OnSocketPauseSave = PauseSave;
            OnSocketCancelSave = CancelSave;
            GlobalProgress = 0;
            //Initiate new settings
            ModelSettings = new Setting();
            //Check if a setting file already exists, if false then create a new one, if true get the different value from it and apply it to the setting object (ModelSettings).
            if (!File.Exists("settings.json"))
            {
                ModelSettings.MaxTransferSize = 10000;
                ModelSettings.SoftwareString = "";
                ModelSettings.PriorityExtension = new List<Extension>();
                UpdateSettingsFile();
            }
            else
            {
                string settingsFile = File.ReadAllText("settings.json");
                var tempWorkList = JsonConvert.DeserializeObject<Setting>(settingsFile);
                ModelSettings = tempWorkList;
                UpdateSettingsFile();
            }

            WorkList = new List<InformationSaveWork>();
            //If the state file has not been initialized then create 5 SaveWork object from nothing
            if (!File.Exists("stateFile.json"))
            {
                WorkList.Add(new CompleteSave("Default", "test", "test", null, SaveWorkType.complete));
                UpdateSaveFile();
            }
            //Then if the State file already exist, use the objects in it to create the WorkList
            else
            {
                string stateFile = File.ReadAllText("stateFile.json");
                var tempWorkList = JsonConvert.DeserializeObject<List<CompleteSave>>(stateFile);

                foreach (CompleteSave work in tempWorkList)
                {
                    if (work.Type == SaveWorkType.complete)
                    {
                        CreateCompleteWork(work.Name, work.SourcePath, work.DestinationPath, work.ExtentionToEncryptList);
                    }
                    else if (work.Type == SaveWorkType.differencial)
                    {
                        CreateDifferencialWork(work.Name, work.SourcePath, work.DestinationPath, work.ExtentionToEncryptList);
                    }
                }
                UpdateSaveFile();
            }

            Log.InitSoftwareLogLine();
            
        }

        //Thread friendly / safe object for locking
        public static object sync = new object();

        public static SaveWorkUpdateDelegate OnSaveWorkUpdate;

        public static UpdateGlobalProgress OnProgressUpdate;

        public static UpdateModelError OnUpdateModelError;

        //Store all save works
        private List<InformationSaveWork> workList;

         
        /// The work list in model
         
        public List<InformationSaveWork> WorkList
        {
            get { return workList; }
            set 
            { 
                workList = value;
                OnPropertyChanged("WorkList");
            }
        }

        private string modelError;

        /// Model Error handle string

        public string ModelError
        {
            get { return modelError; }
            set
            {
                modelError = value;
                OnPropertyChanged("ModelError");
            }
        }

        private Setting modelSettings;
        /// Setting object
        public Setting ModelSettings
        {
            get { return modelSettings; }
            set 
            {
                modelSettings = value;
                OnPropertyChanged("ModelSettings");
            }
        }

         
        /// Model Error String update method
         
        /// <param name="_errorString"></param>
        public void SetModelError(string _errorString)
        {
            ModelError = _errorString;
        }

        private double globalProgress;
        /// Global Progress State for a all save process
        public double GlobalProgress
        {
            get { return globalProgress; }
            set 
            { 
                globalProgress = value;
                OnPropertyChanged("GlobalProgress");
            }
        }

        /// Create a save work (with a differential save algorithm)

        /// <param name="NameProcedure">Name of the work (must be different from existing ones)</param>
        /// <param name="_source">The Source path to save</param>
        /// <param name="_destination">The Target destination to save files in</param>
        public void CreateDifferencialWork(string NameProcedure, string _source, string _destination, List<Extension> _extension)
        {
            DifferentialSave work = new DifferentialSave(NameProcedure, _source, _destination, _extension, SaveWorkType.differencial);
            WorkList.Add(work);
            SetWorkIndex();
            UpdateSaveFile();
            Log.CreateWorkLogLine(work);
        }

        /// Create a save work (with a complete save algorithm)

        /// <param name="NameProcedure">Name of the work (must be different from existing ones)</param>
        /// <param name="_source">The Source path to save</param>
        /// <param name="_destination">The Target destination to save files in</param>
        public void CreateCompleteWork(string NameProcedure, string _source, string _destination, List<Extension> _extension)
        {
            CompleteSave work = new CompleteSave(NameProcedure, _source, _destination, _extension, SaveWorkType.complete);
            WorkList.Add(work);
            SetWorkIndex();
            UpdateSaveFile();
            Log.CreateWorkLogLine(work);
        }
         
        /// Modify value of save works objects stored in workList, if there is any null parameters the value attached isn't changed
         
        /// <param name="_nb">Index of the work you want to change in the list</param>
        /// <param name="NameProcedure">New name to apply to the work</param>
        /// <param name="SourcePath">New source path to apply to the work</param>
        /// <param name="DestinationPath">New target destination path to apply to the work</param>
        /// <param name="_type">New type of save work to apply to the work</param>
        public void ChangeWork(int _nb, string NameProcedure, string SourcePath, string DestinationPath, SaveWorkType _type, List<Extension> _extension)
        {
            if (_type != WorkList[_nb].Type && _type == SaveWorkType.complete)
            {
                WorkList[_nb] = new CompleteSave(NameProcedure, SourcePath, DestinationPath, _extension, SaveWorkType.complete);
            }
            else if (_type != WorkList[_nb].Type && _type == SaveWorkType.differencial)
            {
                WorkList[_nb] = new DifferentialSave(NameProcedure, SourcePath, DestinationPath, _extension, SaveWorkType.differencial);
            }
            else
            {
                if (NameProcedure != "") { WorkList[_nb].Name = NameProcedure; }
                if (SourcePath != "") { WorkList[_nb].SourcePath = SourcePath; }
                if (DestinationPath != "") { WorkList[_nb].DestinationPath = DestinationPath; }
                WorkList[_nb].ExtentionToEncryptList = _extension;
            }
            SetWorkIndex();

            UpdateSaveFile();
            Log.ChangeWorkLogLine(WorkList[_nb]);
        }

         
        /// Can delete a save work (set to null)
         
        /// <param name="_nb">Index of the work in the list to delete</param>
        public void DeleteWork(int _nb)
        {
            WorkList.RemoveAt(_nb);
            SetWorkIndex();
            UpdateSaveFile();
            Log.DeleteWorkLogLine(_nb);
        }

         
        /// Asign index to work in worklist
         
        public void SetWorkIndex()
        {
            foreach (InformationSaveWork li in WorkList)
            {
                li.Index = WorkList.IndexOf(li);
            }
        }

         
        /// Can initiate a type of save from the numbers of the save work in workList.
         
        /// <param name="_nb">Index of the work in the list to execute the save process</param>
        public void DoSave(int _nb)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(WorkList[_nb].Save));
        }

        public static PauseSaveDelegate OnSocketPauseSave;


        /// Resume a specific save

        /// <param name="_nb">Index of the work in the list</param>
        public void ResumeSave(int _nb)
        {
            lock (sync)
            {
                if (WorkList[_nb].Progress != null && WorkList[_nb].Progress.IsPaused != false)
                {
                    WorkList[_nb].Progress.IsPaused = false;
                    Log.SaveResumed(_nb);
                }
            }
        }

        public static CancelSaveDelegate OnSocketCancelSave;

        /// Pause a specific save

        /// <param name="_nb">Index of the work in the list</param>
        public void PauseSave(int _nb)
        {
            lock (sync)
            {
                if (WorkList[_nb].Progress != null && WorkList[_nb].Progress.IsPaused != true)
                {
                    WorkList[_nb].Progress.IsPaused = true;
                    Log.SavePaused(_nb);
                }
            }
        }

        public static ResumeSaveDelegate OnSocketResumeSave;


         
        /// Cancel a specific save
         
        /// <param name="_nb">Index of the work in the list</param>
        public void CancelSave(int _nb)
        {
            lock (sync)
            {
                if (WorkList[_nb].Progress != null)
                {
                    WorkList[_nb].Progress.Cancelled = true;
                    Log.SaveCancelled(_nb);
                }
            }
        }

         
        /// Initiate all save in work list
         
        public void DoAllSave()
        {
            foreach (InformationSaveWork work in WorkList)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(work.Save));
            }
        }

         
        /// Get the global save progress percentage
         
        /// <returns></returns>
        public void UpdateAllSaveProgress()
        {
            double progressCount = 0;
            foreach (InformationSaveWork work in WorkList)
            {
                if (work.Progress != null)
                    progressCount += work.Progress.ProgressState;
            }
            GlobalProgress =  progressCount / WorkList.Count;
        }


        /// Resume a specific save

        public void ResumeAllSave()
        {
            lock (sync)
            {
                foreach (InformationSaveWork work in WorkList)
                {
                    if (work.Progress != null && work.Progress.IsPaused != false) work.Progress.IsPaused = false;
                }
            }
        }

        /// Pause a specific save
        public void PauseAllSave()
        {
            lock (sync)
            {
                foreach (InformationSaveWork work in WorkList)
                {
                    if (work.Progress != null && work.Progress.IsPaused != true) work.Progress.IsPaused = true;
                } 
            }
        }

        /// Cancel a specific save
         
        public void CancelAllSave()
        {
            lock (sync)
            {
                foreach (InformationSaveWork work in WorkList)
                {
                    if (work.Progress != null) work.Progress.Cancelled = true;
                }
            }
        }

         
        /// Update the state file with the work list value
         
        /// <param name="_nb">Index of the save work process to update</param>
        public void UpdateSaveFile()
        {
            lock (sync)
            {
                //Convert the work list to a json string then write it in a json file
                var convertedJson = JsonConvert.SerializeObject(WorkList, Formatting.Indented);
                File.WriteAllText("stateFile.json", convertedJson);
            }
        }

         
        /// Update Setting file method (write the setting object in a json file)
         
        public void UpdateSettingsFile()
        {
            lock (sync)
            {
                //Convert the settings to a json string then write it in a json file
                var convertedJson = JsonConvert.SerializeObject(ModelSettings, Formatting.Indented);
                File.WriteAllText("settings.json", convertedJson);
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
