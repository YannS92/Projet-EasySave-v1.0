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
     
    /// Interaction logic for MainWindow.xaml
     
    public partial class BaseWindow : Window
    {

        #region Variables

        ViewModel VM;

        private List<UIElement> formElementList;
         
        /// List of Elements in the form.
         
        public List<UIElement> FormElementList { get => formElementList; set => formElementList = value; }

        private List<UIElement> optionButtonList;
         
        /// List of option Buttons (Launch, Modify...) + the SaveList ListView.
         
        public List<UIElement> OptionButtonList { get => optionButtonList; set => optionButtonList = value; }

        private List<UIElement> confirmButtonList;
         
        /// List of Buttons Confirm and Back.
         
        public List<UIElement> ConfirmButtonList { get => confirmButtonList; set => confirmButtonList = value; }

        private List<UIElement> selectionButtonList;
         
        /// List of Buttons Confirm and Back.
         
        public List<UIElement> SelectionButtonList { get => selectionButtonList; set => selectionButtonList = value; }

        private List<UIElement> checkBoxList;
         
        /// List of extension checkboxes.
         
        public List<UIElement> CheckBoxList { get => checkBoxList; set => checkBoxList = value; }

        private List<UIElement> settingsCheckBoxList;
         
        /// List of extension checkboxes in settings frame.
         
        public List<UIElement> SettingsCheckBoxList { get => settingsCheckBoxList; set => settingsCheckBoxList = value; }

        private bool isAnItemSelected = false;
         
        /// Keeps track of the item selection status
         
        public bool IsAnItemSelected { get => isAnItemSelected; set => isAnItemSelected = value; }

        private bool firstTimeSelection = true;
         
        /// Prevents from the message box showing up on startup
         
        public bool FirstTimeSelection { get => firstTimeSelection; set => firstTimeSelection = value; }

        private bool allSaves = false;
         
        /// Prevents from the message box showing up on startup
         
        public bool AllSaves { get => allSaves; set => allSaves = value; }


        #endregion

        #region Constructor

         
        /// View Constructor
         
        public BaseWindow()
        {
            VM = new ViewModel();
            DataContext = VM;

            InitializeComponent();
            
            FormElementList = new List<UIElement>
            {
                SaveNameForm,
                SaveSourcePathForm,
                SaveDestinationPathForm,
                SaveTypeForm
            };

            OptionButtonList = new List<UIElement>
            {
                LaunchAllSave,
                Create,
                SaveList
            };

            ConfirmButtonList = new List<UIElement>
            {
                Confirm,
                Back
            };

            SelectionButtonList = new List<UIElement>
            {
                ModifySave,
                LaunchSave,
                DeleteSave
            };

            CheckBoxList = new List<UIElement>
            {
                Txt,
                Rar,
                Zip,
                Docx,
                Mp4,
                Pptx,
                Jpg,
                Png,
                Pdf,
                Exe,
                Iso
            };

            SettingsCheckBoxList = new List<UIElement>
            {
                TxtSettings,
                RarSettings,
                ZipSettings,
                DocxSettings,
                Mp4Settings,
                PptxSettings,
                JpgSettings,
                PngSettings,
                PdfSettings,
                ExeSettings,
                IsoSettings
            };

            if (Settings.Default.languageCode == "en-US")
                LanguageSelection.SelectedIndex = 0;
            else
                LanguageSelection.SelectedIndex = 1;


            foreach (Extension _extension in VM.Model.ModelSettings.PriorityExtension)
            {
                CheckBox _checkBox = FindName(_extension.ToString().First().ToString().ToUpper() + _extension.ToString().Substring(1) + "Settings") as CheckBox;
                _checkBox.IsChecked = true;
            }
            MaxSizeSettingsForm.Text = VM.Model.ModelSettings.MaxTransferSize.ToString();
            SoftwareForm.Text = VM.Model.ModelSettings.SoftwareString;

            VM.Model.PropertyChanged += Model_PropertyChanged;
        }

        #endregion

        #region Methods

         
        /// Informs the user of the language change on next startup
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!firstTimeSelection)
            {
                if (LanguageSelection.SelectedIndex == 0)
                {
                    Settings.Default.languageCode = "en-US";
                    MessageBox.Show("The change has been taken into account and will be effective on the next application startup.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    Settings.Default.languageCode = "fr-FR";
                    MessageBox.Show("Le changement a bien été pris en compte et sera effectif au prochain démarrage de l'application.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Properties.Settings.Default.Save();
            }
            else
            {
                firstTimeSelection = false;
            }
        }

         
        /// Shows the settings menu
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalSettings.Visibility = Visibility.Visible;
        }

        #endregion

    }
}
