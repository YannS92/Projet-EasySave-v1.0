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
     
    /// Interaction logic for MainWindow.xaml Main frame
     
    public partial class BaseWindow : Window
    {

        #region Methods

        #region Option Buttons

         
        /// Creation of a new save / Visual duties
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            ChangeUIElementEnableState(FormElementList, true);
            ChangeUIElementEnableState(OptionButtonList, false);
            ChangeUIElementEnableState(SelectionButtonList, false);
            ChangeUIElementEnableState(CheckBoxList, true);
            ALL.IsEnabled = true;
            ChangeUIElementVisibilityState(ConfirmButtonList, Visibility.Visible);
            ClearForm();
            Confirm.Click -= ConfirmModifyClick;
            Confirm.Click -= ConfirmCreateClick;
            Confirm.Click += ConfirmCreateClick;
        }

         
        /// Deletion of the selected save
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSave_Click(object sender, RoutedEventArgs e)
        {
            InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;
            VM.DeleteSaveProcedure(selectedItem.Index);

            ChangeUIElementEnableState(SelectionButtonList, false);

            SaveList.Items.Refresh();
            IsAnItemSelected = false;
        }

         
        /// Fill in the form with Save object info.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifySave_Click(object sender, RoutedEventArgs e)
        {
            InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;
            SaveNameForm.Text = selectedItem.Name;
            SaveSourcePathForm.Text = selectedItem.SourcePath;
            SaveDestinationPathForm.Text = selectedItem.DestinationPath;

            if (selectedItem.Type == SaveWorkType.complete)
            {
                SaveTypeForm.SelectedIndex = 0;
            }
            else
            {
                SaveTypeForm.SelectedIndex = 1;
            }

            if (selectedItem.ExtentionToEncryptList != null)
            {
                foreach (Extension _extension in selectedItem.ExtentionToEncryptList)
                {
                    CheckBox _checkBox = FindName(_extension.ToString().First().ToString().ToUpper() + _extension.ToString().Substring(1)) as CheckBox;
                    _checkBox.IsChecked = true;
                }

                if (ALL.IsChecked == true)
                {
                    ChangeUIElementEnableState(CheckBoxList, false);
                }
                else
                {
                    ChangeUIElementEnableState(CheckBoxList, true);
                }
            }

            ChangeUIElementEnableState(FormElementList, true);
            ChangeUIElementEnableState(OptionButtonList, false);
            ChangeUIElementEnableState(SelectionButtonList, false);
            ALL.IsEnabled = true;
            ChangeUIElementVisibilityState(ConfirmButtonList, Visibility.Visible);

            Confirm.Click -= ConfirmModifyClick;
            Confirm.Click -= ConfirmCreateClick;
            Confirm.Click += ConfirmModifyClick;
            
        }

         
        /// Launch the selected save
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LaunchSave_Click(object sender, RoutedEventArgs e)
        {
            InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;

            AllSaves = false;
            SaveStatus.Visibility = Visibility.Visible;
            PauseSaveStatus.IsEnabled = true;
            ResumeSaveStatus.IsEnabled = false;
            ChangeCurrentSaveLabel(selectedItem.Name);
            ChangeSaveStatusLabel(SaveStatusEnum.running);
            ChangeSaveProgressLabel(0);

            VM.LaunchSaveProcedure(selectedItem.Index);


            while (!VM.Model.WorkList[selectedItem.Index].IsActive)
            {

            }

            VM.Model.WorkList[selectedItem.Index].Progress.PropertyChanged += Progress_PropertyChanged;

        }

        private void LaunchAllSaves_Click(object sender, RoutedEventArgs e)
        {

            AllSaves = true;
            SaveStatus.Visibility = Visibility.Visible;
            PauseSaveStatus.IsEnabled = true;
            ResumeSaveStatus.IsEnabled = false;
            ChangeCurrentSaveLabel("All of them");
            ChangeSaveStatusLabel(SaveStatusEnum.running);
            ChangeSaveProgressLabel(0);

            VM.LaunchAllSaveProcedures();
        }

        #endregion

        #region Confirm Buttons

         
        /// Confirm changes and save them to the Save object.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfirmCreateClick(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(SaveNameForm.Text, @"^[a-zA-Z0-9 '_]{3,50}$") || !Regex.IsMatch(SaveSourcePathForm.Text, @"^[a-zA-Z]:(?:[\/\\][a-zA-Z0-9 _#]+)*$") || !Regex.IsMatch(SaveDestinationPathForm.Text, @"^[a-zA-Z]:(?:[\/\\][a-zA-Z0-9 _#]+)*$") || SaveTypeForm.SelectedItem == null)
            {
                MessageBox.Show(Properties.Langs.Lang.ConfirmBoxFormError, Properties.Langs.Lang.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                
                ChangeUIElementVisibilityState(confirmButtonList, Visibility.Hidden);

                InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;

                List<Extension> extensionList = new List<Extension>();
                if (ALL.IsChecked == false)
                {
                    foreach (CheckBox _checkBox in CheckBoxList)
                    {
                        if (_checkBox.IsChecked == true)
                        {
                            Enum.TryParse(_checkBox.Name.ToLower(), out Extension _extension);
                            extensionList.Add(_extension);
                        }
                    }
                }
                else
                {
                    extensionList.Add(Extension.ALL);
                }

                SaveWorkType saveType = SaveWorkType.complete;
                if (SaveTypeForm.SelectedIndex != 0) { saveType = SaveWorkType.differencial; }

                VM.CreateSaveProcedure(SaveNameForm.Text, SaveSourcePathForm.Text.Replace("\\", "/"), SaveDestinationPathForm.Text.Replace("\\", "/"), saveType, extensionList);

                ClearForm();
                ChangeUIElementEnableState(FormElementList, false);
                ChangeUIElementEnableState(OptionButtonList, true);
                ChangeUIElementEnableState(SelectionButtonList, true);
                ALL.IsEnabled = false;
                ChangeUIElementEnableState(CheckBoxList, false);

                ChangeUIElementEnableState(SelectionButtonList, false);

                SaveList.Items.Refresh();
                IsAnItemSelected = false;
            }
        }

         
        /// Confirm changes and save them to the existing object.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConfirmModifyClick(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(SaveNameForm.Text, @"^[a-zA-Z0-9 '_]{3,50}$") || !Regex.IsMatch(SaveSourcePathForm.Text, @"^[a-zA-Z]:(?:[\/\\][a-zA-Z0-9 _#]+)*$") || !Regex.IsMatch(SaveDestinationPathForm.Text, @"^[a-zA-Z]:(?:[\/\\][a-zA-Z0-9 _#]+)*$") || SaveTypeForm.SelectedItem == null)
            {
                MessageBox.Show(Properties.Langs.Lang.ConfirmBoxFormError, Properties.Langs.Lang.Warning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {

                ChangeUIElementVisibilityState(confirmButtonList, Visibility.Hidden);

                InformationSaveWork selectedItem = (InformationSaveWork)SaveList.SelectedItem;

                List<Extension> extensionList = new List<Extension>();
                if (ALL.IsChecked == false)
                {
                    foreach (CheckBox _checkBox in CheckBoxList)
                    {
                        if (_checkBox.IsChecked == true)
                        {
                            Enum.TryParse(_checkBox.Name.ToLower(), out Extension _extension);
                            extensionList.Add(_extension);
                        }
                    }
                }
                else
                {
                    extensionList.Add(Extension.ALL);
                }

                SaveWorkType saveType = SaveWorkType.complete;
                if (SaveTypeForm.SelectedIndex != 0) { saveType = SaveWorkType.differencial; }

                VM.ModifySaveProcedure(selectedItem.Index, SaveNameForm.Text, SaveSourcePathForm.Text.Replace("\\", "/"), SaveDestinationPathForm.Text.Replace("\\", "/"), saveType, extensionList);

                ClearForm();
                ChangeUIElementEnableState(FormElementList, false);
                ChangeUIElementEnableState(OptionButtonList, true);
                ChangeUIElementEnableState(SelectionButtonList, true);
                ALL.IsEnabled = false;
                ChangeUIElementEnableState(CheckBoxList, false);

                ChangeUIElementEnableState(SelectionButtonList, false);

                SaveList.Items.Refresh();

                SaveList.SelectedItem = null;

                IsAnItemSelected = false;

            }
        }

         
        /// Cancel any changes and go back to inital state of the UI.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            ChangeUIElementEnableState(FormElementList, false);
            ChangeUIElementEnableState(optionButtonList, true);
            ChangeUIElementEnableState(selectionButtonList, true);
            ALL.IsEnabled = false;
            ChangeUIElementEnableState(checkBoxList, false);
            ChangeUIElementVisibilityState(confirmButtonList, Visibility.Hidden);
        }

        #endregion

        #region Change UIElement state

         
        /// Controls the first time an item is selected
         
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ItemSelected(object sender, SelectionChangedEventArgs args)
        {
            if (!IsAnItemSelected)
            {
                IsAnItemSelected = true;
                ChangeUIElementEnableState(SelectionButtonList, true);
            }
        }

         
        /// Empty all of the form fields
         
        private void ClearForm()
        {
            ALL.IsChecked = false;
            SaveNameForm.Text = "";
            SaveSourcePathForm.Text = "";
            SaveDestinationPathForm.Text = "";
            SaveTypeForm.SelectedIndex = -1;
            foreach (CheckBox _checkBox in CheckBoxList)
            {
                _checkBox.IsChecked = false;
            }
        }

         
        /// Enable or disable elements inside an UIElement Collection.
         
        /// <param name="_elementCollection">The UIELement Collection you want to enable/disable.</param>
        /// <param name="_choice">Choose if you want to enable (true) the UIElements or not (false).</param>
        private void ChangeUIElementEnableState(List<UIElement> _elementList, bool _choice)
        {
            if (!(_elementList == SelectionButtonList && IsAnItemSelected == false))
            {
                foreach (UIElement element in _elementList)
                {
                    element.IsEnabled = _choice;
                }

            }
        }

         
        /// Set the visibility of elements inside an UIElement Collection
         
        /// <param name="_elementList">The UIELement Collection you want to show/hide.</param>
        /// <param name="_choice">Choose the visibility state Visible, Hidden or Collapsed.</param>
        private void ChangeUIElementVisibilityState(List<UIElement> _elementList, Visibility _choice)
        {
            foreach (UIElement element in _elementList)
            {
                element.Visibility = _choice;
            }
        }

         
        /// Disable checkboxes when All is ticked.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ALL_Checked(object sender, RoutedEventArgs e)
        {
            ChangeUIElementEnableState(CheckBoxList, false);
        }

         
        /// Enable checkboxes when All is unticked.
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ALL_Unchecked(object sender, RoutedEventArgs e)
        {
            ChangeUIElementEnableState(CheckBoxList, true);
        }

        #endregion

        #endregion
             
    }
}
