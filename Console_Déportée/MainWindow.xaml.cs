using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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

namespace Console_Déportée
{
     
    /// Logique d'interaction pour MainWindow.xaml
     
    public partial class MainWindow
    {

        // Create viewModel
        viewModel viewModel = new viewModel();

        public MainWindow()
        {
            Thread.Sleep(5000);

            // Create View
            InitializeComponent();
            this.DataContext = viewModel;

            
            // Print text on the view
            viewModel.Texte = "Client de visualisation des logs de EasySave: Aucune connexion ...";

            // Run task to print messages
            Task AffichageConsoleAsync = Task.Run(() =>
            {
                // Create a socket
                viewModel.SocketConnection();


                while (true)
                {
                    // Print the logs
                    viewModel.Texte = viewModel.PrintLogForCorrespodingSave();

                    // refresh time
                    Thread.Sleep(700);

                }
            });



            // Run task to print messages
            Task AffichageSaveAsync = Task.Run(() =>
            {

                while (true)
                {
                    // Print the save name
                    viewModel.SaveName = viewModel.ReturnSaveName();
                    // refresh time
                    Thread.Sleep(200);
                }
            });
            
        }


         
        /// Push PlaySaveButton_Click
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResumeSaveButton_Click(object sender, RoutedEventArgs e)
        {

            if (viewModel.SaveName != null)
            {
                string PlayOrder = viewModel.SaveName + "&" + "Play Button pushed";
                viewModel.SendSocket(PlayOrder);
            }
        }


         
        /// Push PauseSaveButton_Click
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PauseSaveButton_Click(object sender, RoutedEventArgs e)
        {

            if (viewModel.SaveName != null)
            {
                string PauseOrder = viewModel.SaveName + "&" + "Pause Button pushed";
                viewModel.SendSocket(PauseOrder);
            }
        }


         
        /// Push StopSaveButton_Click
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.SaveName != null)
            {
                string StopOrder = viewModel.SaveName + "&" + "Stop Button pushed";
                viewModel.SendSocket(StopOrder);
            }
        }


         
        /// Push PrecButton_Click
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrecButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.actualPosition--;
        }


         
        /// Push NextButton_Click_1
         
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextButton_Click_1(object sender, RoutedEventArgs e)
        {
            viewModel.actualPosition++;
        }
    }
}
