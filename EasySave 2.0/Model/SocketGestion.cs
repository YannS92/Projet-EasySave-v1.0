using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasySave_2._0
{
    public static class SocketGestion
    {

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Log to send Part
        /// 

        // bool for singleton 
        public static bool ConnectionActivated = false;
        public static bool SendActivated = false;
        public static bool ReceiveActivated = false;

        // List of Log to send
        public static List<string> logToSend = new List<string>();
        public static int positionInList = 0;
        public static string lastLog;

        // Socket for connection with the other application
        public static Socket socketLog;


         
        /// Will set up the log for sending purpose
         
        /// <param name="contentToSend">Content of the message to send</param>
        public static void ToSendLog(int idSave, string contentLog)
        {
            // Create a string 
            string contentToSend = "sv" + idSave + "&" + contentLog;  

            // Ad content to send list
            logToSend.Add(contentToSend);

            // Avoid to block 
            Task NoBlock = Task.Run(() =>
            {
                // Create a socket connection
                SocketConnection();

                // Start send list
                GestionListLog();
            });

        }


        public static void GestionListLog()
        {
            // Must be laucnched a single time
            if (SendActivated == false)
            {
                SendActivated = true;

                // Run task to read list
                Task SendBoucle = Task.Run(() =>
                {
                    while (true)
                    {
                        if (socketLog != null)
                        {
                            // If end of the list 
                            if (logToSend[logToSend.Count - 1] == lastLog)
                            {
                                // Wait & restart to check if there are new message
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                // Send the first message received that is not already sended
                                SendSocket(logToSend[positionInList]);

                                // Save the last message send
                                lastLog = logToSend[positionInList];
                                // Go to the next id in the list
                                positionInList++;

                                // Sleep to avoid collision with sockets
                                Thread.Sleep(100);
                            }
                        }

                    }
                });
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Socket Part
        /// 

         
        /// Create a socket connection with the other application
         
        public static void SocketConnection()
        {
            // singleton like
            if (ConnectionActivated == false)
            {
                ConnectionActivated = true;

                // Create a socket 
                Socket createdSocket = SocketCreation();

                //Accept connection with the other application
                socketLog = ConnectionAccept(createdSocket);

                // Start to receive socket from the other application
                ReceiveSocket();
            }
        }


         
        /// Create a Socket
         
        /// <returns>A created Socket</returns>
        private static Socket SocketCreation()
        {
            // Create a socket
            Socket socketEcoute = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Param the socket with IP adress and port
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(address, 8080);

            // Listen the the other application
            socketEcoute.Bind(endPoint);
            socketEcoute.Listen(10);

            //Console.WriteLine($"LOG Votre Serveur en écoute sur {address}");

            // Return the socket
            return socketEcoute;
        }


         
        /// Accept connection with the other application
         
        /// <param name="socketEcoute">Socket created</param>
        /// <returns>socket connected</returns>
        private static Socket ConnectionAccept(Socket socketEcoute)
        {
            //Accept connection with the other application
            Socket socketClient = socketEcoute.Accept();

            //Console.WriteLine($"Connexion établie avec : {socketClient.LocalEndPoint.ToString()}");

            //return socket connected
            return socketClient;
        }


         
        /// Create connection and send a message to an other application
         
        /// <param name="content">Content to send to the other application (logs)</param>
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
                    socketLog.Send(message);

                }
                catch (Exception)
                {
                    //Console.WriteLine("Message non envoyé ");
                }
            });
        }



         
        /// Run task to receive socket from the other application
         
        private static void ReceiveSocket()
        {
            // singleton like
            if (ReceiveActivated == false)
            {
                ReceiveActivated = true;

                // Task to avoid to block the main application
                Task WaitForMessage = Task.Run(() =>
                {
                    // try to receive message as long as the other application isn't close
                    try
                    {
                        // While to liste endlessly
                        while (true)
                        {
                            // create a new byte
                            byte[] buffer = new byte[512];

                            // receive byte message (socket)
                            int receivedBytes = socketLog.Receive(buffer);

                            // Encode the message
                            string messageClient = Encoding.UTF8.GetString(buffer, 0, receivedBytes);

                            // Act differently according to the order receive
                            GestionReceptionSocket(messageClient);
                        }
                    }
                    catch (SocketException)
                    {
                        //Console.WriteLine("No connection");
                    }
                });
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// Order Received Part
        /// 

         
        /// Deal the order reception
         
        /// <param name="contentMessage">Content of the received message</param>
        public static void GestionReceptionSocket(string contentMessage)
        {
            // slipt the message in to part
            string[] collecContent = contentMessage.Split('&');

            // First part the id of the save
            string stringIdSave = collecContent[0];
            // subtring to have 0 inted of sv0
            stringIdSave = stringIdSave.Substring(2);
            //turn into stringIdSave an int
            int idSave = Int32.Parse(stringIdSave);


            //Second part the content of the message
            string orderContent = collecContent[1];

            // If the content order to play the save
            if (orderContent == "Play Button pushed")
            {
                // Call function with id
                //Console.WriteLine(idSave + " doit " + orderContent);
                Model.OnSocketResumeSave(idSave);

            } // If the content order to pause  the save
            else if (orderContent == "Pause Button pushed")
            {
                // Call function with id
                //Console.WriteLine(idSave + " doit " + orderContent);
                Model.OnSocketPauseSave(idSave); 

            } // If the content order to stop the save
            else if (orderContent == "Stop Button pushed")
            {
                // Call function with id
                //Console.WriteLine(idSave + " doit " + orderContent);
                Model.OnSocketCancelSave(idSave);

            } // Content receive but not with a correct order
            else
            {
                //Console.WriteLine("Message reçu mais non analysable: " + contentMessage);
            }
        }




    }
}
