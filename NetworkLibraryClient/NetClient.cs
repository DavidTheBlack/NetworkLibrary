//Unity Network Client Plugin. 
//It provide a low level network communication interface using socket
//@Copyright Davide Galdo 

/* 
 * 1-Open Socket Connection 
 * 2-Open Protocol Connection 
 * 3-Receive Data
 * 4-Close Protocol connection
 * 5-Close Socket Connection
 */

/*
 * TODO
 * Implementa la funzione di connessione a livello protocollo.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using NetworkLibraryMessages;
using NetworkStateObject;
using System.Globalization;





namespace NetworkLibraryClient
{
    public class NetClient
    {
        #region Attributes
        //Received data string
        public string _receivedMex = string.Empty;                 
        //Received message 
        public string receivedMex
        {
            get { return _receivedMex; }
        }
        //Used to log special event
        private string _log=string.Empty;
        public string log
        {
            get { return _log; }
        }
        
        //Server IP
        private string _serverIP;
        public string serverIP
        {
            get { return _serverIP; }
            set { _serverIP = value; }
        }
        //Server Port
        private int _serverPort;
        public int serverPort
        {
            get { return _serverPort; }
            set { _serverPort = value; }
        }
        //Server Socket
        private Socket _server;
        private bool _serverIsConnected = false;
        private object netObj;

        //True if server is connected
        public bool serverIsConnected
        {
            get
            {
                return this._serverIsConnected;
            }
        }

        #endregion

        #region Events



        /// <summary>
        /// Evento lanciato quando lo stato del client cambia //Connesso o Disconnesso
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public delegate void ConnectionStateChangedEventHandler(object source, EventArgs e);
        public event ConnectionStateChangedEventHandler connectionStateChanged;
        protected virtual void OnConnectionStateChanged()
        {
            //Cambio lo stato della connessione
            this._serverIsConnected = !this._serverIsConnected;
            if (connectionStateChanged != null)
            {
                connectionStateChanged(this, EventArgs.Empty);
            }
        }
   
        /// <summary>
        /// Evento lanciato quando un nuovo messaggio è consegnato
        /// </summary>
        public delegate void MessageReceivedEventHandler(object source, EventArgs e);
        public event MessageReceivedEventHandler messageReceived;
        protected virtual void OnMessageReceived()
        {
            if (messageReceived != null)
            {
                messageReceived(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Methods


        //Constructor
        public NetClient(string ip, int port)
        {
            _serverIP = ip;
            _serverPort = port;
        }
        
        //Socket connection method
        public void OpenTcpConnection()
        {
            try
            {
                
                
                //Create the remote endpoint for the socket
                IPEndPoint remoteEP=new IPEndPoint(IPAddress.Parse(_serverIP),_serverPort);
                ////Create TCP/IP socket
                //netobj.clientSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ////Start connection to remote endpoint
                //netobj.clientSocket.BeginConnect(remoteEP, new AsyncCallback(TcpConnectionCallback), netobj);
                //Crea socket verso server
                Socket socket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //avvia connessione verso endpoint remoto
                socket.BeginConnect(remoteEP, new AsyncCallback(TcpConnectionCallback), socket);
            }
            catch (Exception ex)
            {
                this._log="Server is not active. \n Please start server and try again. \n" + ex.ToString();
            }
        }

        /// <summary>
        /// Close the tcp connection
        /// </summary>
        public void closeConnection()
        {
            if (this._serverIsConnected)
            {
                this.SendMessage(NetMessages.disconnectRequest);
                System.Threading.Thread.Sleep(500);
                OnConnectionStateChanged();
                this._server.Shutdown(SocketShutdown.Both);
                this._server.Close();
            }

        }

        //Async connection callback method 
        private void TcpConnectionCallback(IAsyncResult ar)
        {
            try
            {
                //Retrive network information
                Socket handler = (Socket)ar.AsyncState;

                //complete the connections
                handler.EndConnect(ar);
                //Salva handler
                this._server = handler;
                NetStateObject netStateObject = new NetStateObject();
                netStateObject.socket=handler;
                
                //Start to listen for data from the server
                handler.BeginReceive(netStateObject.recBuffer, 0, netStateObject.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), netStateObject);
                
                this._log = "Connection created";
                //Segnalo avvenuta connessione al server
                OnConnectionStateChanged();
                //Attendo 1,5sec prima di ritornare
                System.Threading.Thread.Sleep(1500);
            }
            catch (Exception ex)
            {
                this._log = "Error: " + ex.Message;
            }
        }

        //Async received data callback method
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the NetObject and the client socket from the asynchronous state object
                NetStateObject netStateObj = (NetStateObject)ar.AsyncState;
                Socket handler = netStateObj.socket;
                //read data from the remote server
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    /* Versione con string builder
                     * netObj.receivedMex.Append(Encoding.ASCII.GetString(netObj.recBuffer, 0, bytesRead));
                     */
                    netStateObj.receivedMex = Encoding.ASCII.GetString(netStateObj.recBuffer, 0, bytesRead);
                    // Verifico che il messaggio sia arrivato tutto cercando il trailer
                    //string recMex = netObj.receivedMex.ToString();
                    if (netStateObj.receivedMex.IndexOf(NetMessages.messageTrailer) > -1)
                    {
                        //Se l'intero messaggio è arrivato allora lo rendo disponibile in uscita e sollevo evento
                        this._receivedMex = netStateObj.receivedMex;
                        OnMessageReceived();
                        //Rimango in ascolto di altri messaggi
                        netStateObj.socket.BeginReceive(netStateObj.recBuffer, 0, netStateObj.bufferSize, 0, new AsyncCallback(ReceiveCallback), netObj);
                    }
                }
            }
            catch (Exception ex)
            {
                _log = "Error: " + ex.Message;
            }
        }

 
        /// <summary>
        /// Funzione usata per spedire dati al client
        /// </summary>
        /// <param name="data1">Primo dato</param>
        /// <param name="data2">Secondo dato</param>
        /// <param name="data3">Terzo dato</param>
        public void SendData(float data1, float data2, float data3)
        {
            if (this._server.Connected)
            {
                string message = NetMessages.userPositionHeader + data1.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data2.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data3.ToString(CultureInfo.InvariantCulture) + NetMessages.messageTrailer;
                Byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._server.Send(sendingData, sendingData.Length, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to send data {0}", ex.ToString());
                }

            }
        }

        /// <summary>
        /// Funzione usata per spedire messaggi di protocollo
        /// </summary>
        /// <param name="mess">messaggio di protocollo da inviare</param>
        public void SendMessage(string mess)
        {
            if (this._serverIsConnected)
            {
                string message = NetMessages.messageHeader + NetMessages.token + mess + NetMessages.token + NetMessages.messageTrailer;
                byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._server.Send(sendingData, sendingData.Length, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to send message {0}", ex.ToString());
                }

            }
        }


        /// <summary>
        /// Method to communicate server to send position
        /// </summary>
        public void readyToReceive()
        {
            if (this._serverIsConnected)
            {
                this.SendMessage(NetMessages.readyToReceive);
            }
            
        }


        
        #endregion

        #region old methods




        //private void  getData(IAsyncResult ar)
        //{
        //    int BytesRead;
        //    try
        //    {
        //        // Finish asynchronous read into readBuffer and return number of bytes read.
        //        BytesRead = client.GetStream().EndRead(ar);
        //        if (BytesRead < 1)
        //        {
        //            res = "Disconnected";
        //            return;
        //        }
        //        // Convert the byte array the message was saved into
        //        _receivedData = Encoding.ASCII.GetString(readBuffer, 0, BytesRead);
        //        //Removing the final token "<EOF>"
        //        _receivedData.Replace("<EOF>", "");
        //        // Start a new asynchronous read into readBuffer.
        //        client.GetStream().BeginRead(readBuffer, 0, READ_BUFFER_SIZE, new AsyncCallback(getData), null);

        //    }
        //    catch (Exception ex)
        //    {
        //        res = ("Error \n"+ ex.ToString());
        //        return;
        //    }
        //}

        //// Process the command received from the server, and take appropriate action.
        //private void ProcessCommands(string strMessage)
        //{
        //    string[] dataArray;
        //    // Message parts are divided by "|"  Break the string into an array accordingly.
        //    dataArray = strMessage.Split((char)124);
        //    // dataArray(0) is the command.
        //    switch (dataArray[0])
        //    {
        //        case "DATA":
        //            // Server sent the position of the user
        //            /*IMPLEMENT
        //             * Estrapola la posizione dal pacchetto e la rende disponibile adi dati dal pacchetto 
                     
        //             */
        //            break;
        //    }
        //}

        //OLD VERSION
        //private void ReceiveCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        //Retrieve the NetObject and the client socket from the asynchronous state object
        //        NetObject netObj = (NetObject)ar.AsyncState;

        //        //read data from the remote server
        //        int bytesRead = netObj.clientSocket.EndReceive(ar);

        //        if (bytesRead != 0) { 
        //            //Encoding received buffer in ascii 
        //            this._receivedMex=Encoding.ASCII.GetString(netObj.recBuffer,0,bytesRead);
        //            //Signal that all bytes have been received
        //            OnMessageReceived.Set();
        //        }
        //        //Get other data.
        //        netObj.clientSocket.BeginReceive(netObj.recBuffer, 0, netObj.bufferSize, 0, new AsyncCallback(ReceiveCallback), netObj);

        //    }
        //    catch (Exception ex)
        //    {
        //        _log = "Error: " + ex.Message;
        //    }
        //}

        
        #endregion
    }

}

