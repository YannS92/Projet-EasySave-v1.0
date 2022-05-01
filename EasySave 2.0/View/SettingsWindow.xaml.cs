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

namespace EasySave_2._0
{
     
    /// Interaction logic for MainWindow.xaml Settings frame
     
    public partial class BaseWindow : Window
    {
       #region Methods                

         
        /// Only permits numbers to be entered in the text field
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ConfirmSettings_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Visibility = Visibility.Collapsed;
            VM.Model.ModelSettings.MaxTransferSize = Int32.Parse(MaxSizeSettingsForm.Text);
            VM.Model.ModelSettings.SoftwareString = SoftwareForm.Text;

            List<Extension> extensionCheckedList = new List<Extension>();
            foreach (CheckBox _checkBox in SettingsCheckBoxList)
            {
                if (_checkBox.IsChecked == true)
                {
                    Enum.TryParse(_checkBox.Name.ToLower().Substring(0, _checkBox.Name.Length - 8), out Extension _extension);
                    extensionCheckedList.Add(_extension);
                }
            }
            VM.Model.ModelSettings.PriorityExtension = extensionCheckedList;
            VM.Model.UpdateSettingsFile();
        }

        private void BackSettings_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Visibility = Visibility.Collapsed;
            
            foreach (CheckBox _checkbox in SettingsCheckBoxList)
            {
                _checkbox.IsChecked = false;
            }

            foreach (Extension _extension in VM.Model.ModelSettings.PriorityExtension)
            {
                CheckBox _checkBox = FindName(_extension.ToString().First().ToString().ToUpper() + _extension.ToString().Substring(1) + "Settings") as CheckBox;
                _checkBox.IsChecked = true;
            }

            MaxSizeSettingsForm.Text = VM.Model.ModelSettings.MaxTransferSize.ToString();
            SoftwareForm.Text = VM.Model.ModelSettings.SoftwareString;
        }

        #endregion
    }
}
