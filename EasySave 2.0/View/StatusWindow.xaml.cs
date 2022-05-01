using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Globalization;
using EasySave_2._0.Properties;
using System.ComponentModel;

namespace EasySave_2._0
{
     
    /// Interaction logic for MainWindow.xaml Status frame
     
    public partial class BaseWindow : Window
    {
        
        #region Methods

        #region Update Labels

         
        /// Changes the curent save label content
         
        /// <param name="_currentSaveName">Name of the current save procedure</param>
        private void ChangeCurrentSaveLabel(string _currentSaveName)
        {
            CurrentSaveLabel.Content = _currentSaveName;
        }

         
        /// Changes the curent save status label content
         
        /// <param name="_currentSaveStatus">Status of the current save procedure</param>
        private void ChangeSaveStatusLabel(SaveStatusEnum _currentSaveStatus)
        {
            this.Dispatcher.Invoke(() =>
            {
                switch (_currentSaveStatus)
                {
                    case SaveStatusEnum.running:
                        SaveStatusLabel.Content = Properties.Langs.Lang.Running;
                        break;
                    case SaveStatusEnum.paused:
                        SaveStatusLabel.Content = Properties.Langs.Lang.Paused;
                        break;
                    case SaveStatusEnum.encryption:
                        SaveStatusLabel.Content = Properties.Langs.Lang.Encryption;
                        break;
                    case SaveStatusEnum.complete:
                        SaveStatusLabel.Content = Properties.Langs.Lang.Complete;
                        break;
                }
            });
        }

         
        /// Changes the save progress label content.
         
        /// <param name="_saveProgress">percentage completion of the save procedure</param>
        private void ChangeSaveProgressLabel(int _saveProgress)
        {
            this.Dispatcher.Invoke(() =>
            {
                SaveProgressLabel.Content = _saveProgress + " %";
                if (_saveProgress == 100)
                {
                    ChangeSaveStatusLabel(SaveStatusEnum.complete);
                    PauseSaveStatus.IsEnabled = false;
                    ResumeSaveStatus.IsEnabled = false;
                    CancelSaveStatus.Visibility = Visibility.Collapsed;
                    CloseSaveStatus.Visibility = Visibility.Visible;
                }
            });
        }

        #endregion

        #region Buttons

         
        /// Closes save status frame after save completion
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseSaveStatus_Click(object sender, RoutedEventArgs e)
        {
            ChangeSaveStatusLabel(SaveStatusEnum.complete);
            CloseSaveStatus.Visibility = Visibility.Collapsed;
            CancelSaveStatus.Visibility = Visibility.Visible;
            SaveStatus.Visibility = Visibility.Collapsed;
        }

         
        /// Informs the Model of the pause request, and change UIElements accordingly.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseSaveStatus_Click(object sender, RoutedEventArgs e)
        {
            PauseSaveStatus.IsEnabled = false;
            ResumeSaveStatus.IsEnabled = true;
            ChangeSaveStatusLabel(SaveStatusEnum.paused);

            if (!AllSaves)
            {
                InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;
                VM.PauseSaveProcedure(selectedItem.Index, true);
            }
            else
            {
                VM.PauseAllSaveProcedures(true);
            }
        }

          
        /// Informs the Model of the resume request, and change UIElements accordingly.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResumeSaveStatus_Click(object sender, RoutedEventArgs e)
        {
            PauseSaveStatus.IsEnabled = true;
            ResumeSaveStatus.IsEnabled = false;
            ChangeSaveStatusLabel(SaveStatusEnum.running);

            if (!AllSaves)
            {
                InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;
                VM.PauseSaveProcedure(selectedItem.Index, false);
            }
            else
            {
                VM.PauseAllSaveProcedures(false);
            }

        }

         
        /// Informs the Model of the cancel request, and closes the save status frame.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSaveStatus_Click(object sender, RoutedEventArgs e)
        {
            SaveStatus.Visibility = Visibility.Collapsed;

            if (!AllSaves)
            {
                InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;
                VM.CancelSaveProcedure(selectedItem.Index);
            }
            else
            {
                VM.CancelAllSaveProcedures();
            }

        }

        #endregion

         
        /// Called when a property is changed in ProgressionSave Object
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Progress_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ProgressState")
            {
                var property = sender.GetType().GetProperty(e.PropertyName);
                double _propertyValue = (double)property.GetValue(sender, null);
                int _percentage = Convert.ToInt32(Math.Floor(_propertyValue));
                ChangeSaveProgressLabel(_percentage);
            }
            else if (e.PropertyName == "IsEncrypting")
            {
                var property = sender.GetType().GetProperty(e.PropertyName);
                bool _propertyValue = (bool)property.GetValue(sender, null);

                this.Dispatcher.Invoke(() =>
                {
                    if (_propertyValue)
                    {
                        ChangeSaveStatusLabel(SaveStatusEnum.encryption);
                        PauseSaveStatus.IsEnabled = false;
                        ResumeSaveStatus.IsEnabled = false;

                    }
                    else if (!_propertyValue)
                    {
                        ChangeSaveStatusLabel(SaveStatusEnum.complete);
                        CancelSaveStatus.Visibility = Visibility.Collapsed;
                        CloseSaveStatus.Visibility = Visibility.Visible;
                    }
                });
            }
        }

         
        /// Called when a property is changed in the Model
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GlobalProgress" && AllSaves)
            {
                var _property = sender.GetType().GetProperty(e.PropertyName);
                double _propertyValue = (double)_property.GetValue(sender, null);
                int _percentage = Convert.ToInt32(Math.Floor(_propertyValue));
                ChangeSaveProgressLabel(_percentage);
            }
            else if (e.PropertyName == "ModelError")
            {
                var _property = sender.GetType().GetProperty(e.PropertyName);
                string _propertyValue = (string)_property.GetValue(sender, null);

                switch (_propertyValue)
                {
                    case "software":
                        ThreadPool.QueueUserWorkItem(new WaitCallback(MessageBoxSoftware));
                        this.Dispatcher.Invoke(() =>
                        {
                            PauseSaveStatus.IsEnabled = false;
                            ResumeSaveStatus.IsEnabled = false;
                            ChangeSaveStatusLabel(SaveStatusEnum.paused);
                        });
                        break;
                    case "resume":
                        this.Dispatcher.Invoke(() =>
                        {
                            PauseSaveStatus.IsEnabled = false;
                            ResumeSaveStatus.IsEnabled = true;
                            ChangeSaveStatusLabel(SaveStatusEnum.running);
                        });
                        break;
                    case "directory":
                        if (AllSaves)
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(MessageBoxDirectoryAll));
                        }
                        else
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                SaveStatus.Visibility = Visibility.Collapsed;
                                PauseSaveStatus.IsEnabled = false;
                                ResumeSaveStatus.IsEnabled = false;
                            });
                            ThreadPool.QueueUserWorkItem(new WaitCallback(MessageBoxDirectorySingle));
                        }
                        break;
                }
            }
        }

        #endregion
    }
}
