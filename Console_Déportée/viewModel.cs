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

namespace Console_Déportée
{
    class viewModel : INotifyPropertyChanged
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Global variable Part
        /// 


         
        /// String content to print in the view
         
        private string _texte;
        public string Texte
        {
            get { return _texte; }
            set
            {
                if (_texte != value)
                {
                    _texte = value;
                    OnPropertyChanged();
                }
            }
        }

         
        /// String save name to print in the view
         
        private string _saveName;
        public string SaveName
        {
            get { return _saveName; }
            set
            {
                if (_saveName != value)
                {
                    _saveName = value;
                    OnPropertyChanged();
                }
            }
        }


         
        /// OnPropertyChanged
         
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Socket Part
        /// 

        // Bool for singleton like 
        public static bool ReceiveActivated = false;
        public static bool ConnectionActivated = false;

        // Socket for connection
        public static Socket socketLog;

         
        /// Lauchn a socket creation and connection to the other application
         
        public static void SocketConnection()
        {
            // Singleton like
            if (ConnectionActivated == false)
            {
                ConnectionActivated = true;

                // ckeck if the socket is correctly created and connectd
                bool socketCreated = false;
                while (socketCreated == false)
                {
                    socketCreated = true;
                    try
                    {
                        // Create a socket
                        socketLog = SocketCreation();
                    }
                    catch (Exception)
                    {
                        socketCreated = false;
                    }
                }

                // If the socket is created then start listening
                ReceiveSocket();
            }
        }


         
        /// Create a Socket
         
        /// <returns>created socket</returns>
        private static Socket SocketCreation()
        {
            // Create a socket
            Socket socketEcoute = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Param the socket with IP adress and port
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(address, 8080);

            // Listen the the other application
            socketEcoute.Connect(endPoint);

            //Console.WriteLine($"LOG Votre Serveur en écoute sur {address}");

            // Return the socket
            return socketEcoute;
        }


         
        /// Start to receive socket
         
        public static void ReceiveSocket()
        {
            // Singleton like
            if (ReceiveActivated == false)
            {
                ReceiveActivated = true;

                // task to 
                Task WaitForMessage = Task.Run(() =>
                {
                    try
                    {
                        while (true)
                        {
                            // Create a new buffer
                            byte[] buffer = new byte[512];

                            // receive the message from the server
                            int receivedBytes = socketLog.Receive(buffer);

                            // Encode the server message
                            string messageServeur = Encoding.UTF8.GetString(buffer, 0, receivedBytes);


                            GestionContent(messageServeur);

                            // Send the message content to ReturnContent to show it on the console
                            //ReturnContent(messageServeurDecapsulated);

                            //Console.WriteLine(messageServeur);
                        }
                    }
                    catch (SocketException)
                    {
                        // The server application is closed
                        //ReturnContent("La connexion à été fermée par le serveur.");

                        //Console.WriteLine("La connexion a été fermée par le serveur.");
                    }
                });
            }
        }


         
        /// Create connection and send a message to an other application
         
        /// <param name="content">Content to send</param>
        public static void SendSocket(string content)
        {
            // Task to send log with socket
            Task SendLogs = Task.Run(() =>
            {

                // Try to send log
                try
                {
                    // Encode the log
                    byte[] message = Encoding.UTF8.GetBytes(content);

                    // Send the message
                    if (socketLog != null)
                    {
                        socketLog.Send(message);
                        //Console.WriteLine("Message envoyé");
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine("Message non envoyé ");
                }
            });
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Content Part
        /// 

        // List of save name
        public static List<string> saveNameForComboBox = new List<string>();
        // List of content added in the background
        public static List<string> ContentBack = new List<string>();
        // selected save in the view
        public static string saveSelected;
        // Actual position save in the view
        public static int actualPosition = 0;

         
        /// Deal with the received message
         
        /// <param name="contentMessage">Content of the received message</param>
        public static void GestionContent(string contentMessage)
        {
            // slipt the message in to part
            string[] collection = contentMessage.Split('&');

            // First part the id of the save
            string nameId = collection[0];
            //Second part the content of the message
            string contentPart = collection[1];

            // If the first part isn't in the list for save name
            if (saveNameForComboBox.Contains(collection[0]) == false)
            {
                // then add it
                saveNameForComboBox.Add(collection[0]);
            }

            // Call a function to deal with received log in background
            GestionMultiSaveBack(nameId, contentPart);
        }


         
        /// Deal with received log in background
         
        /// <param name="saveName">Name of the save</param>
        /// <param name="content">Content of the log</param>
        public static void GestionMultiSaveBack(string saveName, string content)
        {
            // bool to check if it's the first time we add a line to this save
            bool added = false;

            // for each content in the background list
            int idInList = 0;
            foreach (string contentList in ContentBack)
            {
                // slipt the text
                string[] collecBack = contentList.Split('&');

                // Add the content if the save is already is the background list
                if (collecBack[0] == saveName)
                {
                    // good form
                    string test = ContentBack[idInList] + "\n" + content;
                    // Add the content if the save is already is the background list
                    ContentBack[idInList] = test;

                    // we add the text
                    added = true;

                    //Console.WriteLine(test);

                    // break the foreach
                    break;
                }
                //int + 1 to check have the correct id in the list
                idInList++;
            }

            // if it's the first time we add a line to this save
            if (added == false)
            {
                // good form
                string addNewLine = saveName + "&" + content;
                // add a new line to the list
                ContentBack.Add(addNewLine);
                //Console.WriteLine(addNewLine);
            }
        }


         
        /// Print log when the user chose a save in the view
         
        /// <returns></returns>
        public static string PrintLogForCorrespodingSave()
        {
            // A lot new variables to avoid to use a variables with another thread
            string returnString = "";
            string precedentSelected = saveSelected;

            // if there are save
            if (ContentBack.Count != 0)
            {
                // A lot new variables to avoid to use a variables with another thread
                List<string> listToPrint = ContentBack;
                // for each strig in the background list
                foreach (string contentLine in listToPrint)
                {
                    // A lot new variables to avoid to use a variables with another thread
                    string test = contentLine;
                    string[] collecBack = test.Split('&');

                    // if the selected save matches with a save id in the background list
                    if (collecBack[0] == precedentSelected)
                    {
                        // return the whole text in the background list
                        returnString = collecBack[1];
                        // break the foreach
                        break;
                    }
                }
            }
            // return the content
            return returnString;
        }


         
        /// Return the save name in the texbox in the view
         
        /// <returns></returns>
        public static string ReturnSaveName()
        {
            // save name to print in the view
            string saveNameToprint = "";

            //if the save name list isn't null
            if (saveNameForComboBox.Count != 0)
            {
                // if we reach the end of the list
                if (saveNameForComboBox.Count - 1 < actualPosition)
                {
                    //restart from the begining
                    saveNameToprint = saveNameForComboBox[0];
                    saveSelected = saveNameToprint;
                    actualPosition = 0;

                } // if we reach the start of the list
                else if (actualPosition == -1)
                {
                    // restart from the end
                    saveNameToprint = saveNameForComboBox[saveNameForComboBox.Count - 1];
                    saveSelected = saveNameToprint;
                    actualPosition = saveNameForComboBox.Count - 1;

                } // else act normal and print the corresponding save name
                else
                {
                    saveNameToprint = saveNameForComboBox[actualPosition];
                    saveSelected = saveNameToprint;
                }
            }

            // return the save name
            return saveNameToprint;
        }
    }
}
