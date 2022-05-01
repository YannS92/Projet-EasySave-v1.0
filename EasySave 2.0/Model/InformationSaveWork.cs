using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EasySave_2._0
{
    interface InformationSaveWork
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string CreationTime { get; set; }
        public List<Extension> ExtentionToEncryptList { get; set; }
        public SaveWorkType Type { get; }
        public bool IsActive { get; set; }
        public ProgressionSave Progress { get; set; }

        public void Save(object obj);
        public void CreateProgress(int _totalFilesNumber, long _totalSize, int _filesRemaining, int _progressState, long _sizeRemaining);
        public void DeleteProgress();
        public void EncryptFiles();
    }
}
