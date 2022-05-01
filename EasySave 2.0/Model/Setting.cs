using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EasySave_2._0
{
     
    /// Setting class for Model
     
    public class Setting : INotifyPropertyChanged
    {
        public static long maxTransferSize;
        /// Max multithreading transfer size
        public long MaxTransferSize
        {
            get { return maxTransferSize; }
            set
            {
                maxTransferSize = value;
                OnPropertyChanged("MaxTransferSize");
            }
        }
        private List<Extension> priorityExtension;
        /// Priority extension to save 
        public List<Extension> PriorityExtension
        {
            get { return priorityExtension; }
            set
            {
                priorityExtension = value;
                OnPropertyChanged("PriorityExtension");
            }
        }

        public static string softwareString;
         
        /// Business software name string
         
        public string SoftwareString
        {
            get { return softwareString; }
            set
            {
                softwareString = value;
                OnPropertyChanged("SoftwareString");
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
