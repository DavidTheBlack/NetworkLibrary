//Unity Network Plugin. 
//It provide a low level network communication interface using TCP socket
//@Copyright Davide Galdo 2017

using System;
using System.Text;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using NetworkLibraryMessages;
using NetworkStateObject;

namespace NetworkObj
{
    public class NetworkObject
    {

        #region proprieties
        private string _locHostName;
        private IPAddress[] _localAddresses;
        private IPAddress _localAddressV4;
        public IPAddress localAddressV4
        {
            get { return _localAddressV4; }
        }

        
        /// <summary>
        /// Listening connection port
        /// </summary>
        private int _localPort;

        /// <summary>
        /// Remote Socket 
        /// </summary>
        private Socket _remote;

        private string _remoteIP;
        /// <summary>
        /// Remote IP
        /// </summary>
        public string remoteIP
        {
            get { return _remoteIP; }
            set { _remoteIP = value; }
        }
        
        private int _remotePort;
        /// <summary>
        /// Remote Port
        /// </summary>
        public int remotePort
        {
            get { return _remotePort; }
            set { _remotePort = value; }
        }
        
        private bool _remoteIsConnected = false;
        /// <summary>
        /// State of the connection
        /// </summary>
        public bool remoteIsConnected
        {
            get { return this._remoteIsConnected; }
        }
        
        private string _log = string.Empty;
        /// <summary>
        /// Used to log special event
        /// </summary>
        public string log
        {
            get { return _log; }
        }

        private string _receivedMex = string.Empty;
        /// <summary>
        /// Received message
        /// </summary>
        public string receivedMex
        {
            get { return _receivedMex; }
        }

        #endregion

        #region events


        /// <summary>
        /// Evento lanciato quando lo stato del client cambia //Connesso o Disconnesso
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public delegate void ConnectionStateChangedEventHandler(object source, EventArgs e);
        public event ConnectionStateChangedEventHandler connectionStateChanged;
        protected virtual void OnConnectionStateChanged()
        {
            //Aggiorno lo stato della connessione tcp
            if (this._remote != null)            
                _remoteIsConnected = _remote.Connected;
            else
                _remoteIsConnected = false;
                       
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

        #region methods

        /// <summary>
        /// Use this constructor if you want to create a network object acting as a server
        /// The object start immediately to wait fo a connection from one client
        /// </summary>
        /// <param name="localPort">Local tcp port opened</param>
        public NetworkObject(int localPort)
        {
            this._localPort = localPort;
            WaitForTcpConnection();

        }

        /// <summary>
        /// Methods to put the object in "wait for connection" state 
        /// </summary>
        public void WaitForTcpConnection()
        {

            try
            {
                //Resolve the host name
                _locHostName = Dns.GetHostName();
                //Get all IP addresses of the host 
                _localAddresses = Dns.GetHostAddresses(_locHostName);
                //keep the first IPv4 Address from the configuration
                for (int i = 0; i < _localAddresses.Length; i++)
                {
                    if (IPAddress.Parse(_localAddresses[i].ToString()).AddressFamily == AddressFamily.InterNetwork)
                    {
                        _localAddressV4 = _localAddresses[i];
                    }
                }
            }
            catch (Exception ex)
            {
                _log = "Error trying to get local address; " + ex.ToString();
            }

            // Verify we got an IP address. Tell the user if we did
            if (_localAddressV4 == null)
            {
                _log = "Unable to get local address";
                throw new Exception();
            }

            //Instantiate the local socket 
            _remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _remote.Bind(new IPEndPoint(_localAddressV4, _localPort));
            _remote.Listen(1); //Accept 1 client connection per time
            _remote.BeginAccept(new AsyncCallback(this.TcpConnectionCallbackServer), _remote);


        }

        /// <summary>
        /// Finalizza la connessione, quando la connessione TCP è stabilita solleva evento OnConnectionStateChanged
        /// </summary>
        /// <param name="ar"></param>
        private void TcpConnectionCallbackServer(IAsyncResult ar)
        {
            try
            {
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                //Raise the connection state change event
                OnConnectionStateChanged();

                this._log = "Connection created";

                NetStateObject netStateObject = new NetStateObject();
                netStateObject.socket = handler;

                //Begin receive for remote messages
                handler.BeginReceive(netStateObject.recBuffer, 0, netStateObject.bufferSize, 0,
                new AsyncCallback(ReceiveCallback), netStateObject);

                //Wait 0.5sec before return
                System.Threading.Thread.Sleep(500);

            }
            catch (Exception ex)
            {
                this._log = "Error: " + ex.Message;
            }
        }



        /// <summary>
        /// Use this constructor if you want to create a client network object
        /// The object start immediately a connection to the server
        /// </summary>
        /// <param name="remoteIp"> Ip of the server</param>
        /// <param name="remotePort"> tcp port of the server</param>
        public NetworkObject(string remoteIp, int remotePort)
        {
            _remoteIP = remoteIp;
            _remotePort = remotePort;
            this.OpenTcpConnection();


        }

        /// <summary>
        /// Methods to start connection to a server
        /// </summary>
        public void OpenTcpConnection()
        {
            try
            {
                //Create the remote endpoint for the socket
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(_remoteIP), _remotePort);
                ////Create TCP/IP socket
                _remote = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ////Start connection to remote endpoint
                _remote.BeginConnect(remoteEP, new AsyncCallback(TcpConnectionCallbackClient), _remote);
            }
            catch (Exception ex)
            {
                this._log = "Server is not active. \n Please start server and try again. \n" + ex.ToString();
            }
        }

        /// <summary>
        /// Terminate the connection as client and start listening for protocol messages
        /// </summary>
        /// <param name="ar"></param>
        private void TcpConnectionCallbackClient(IAsyncResult ar)
        {
            try
            {
                //Retrive socket information
                Socket handler = (Socket)ar.AsyncState;

                //complete the connections
                handler.EndConnect(ar);

                //Raise the connection state change event
                OnConnectionStateChanged();

                this._log = "Connection created";

                NetStateObject netStateObject = new NetStateObject();
                netStateObject.socket = handler;

                //Start to listen for data from the server
                handler.BeginReceive(netStateObject.recBuffer, 0, netStateObject.bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), netStateObject);

                //Wait 0.5sec before return
                System.Threading.Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                this._log = "Error: " + ex.Message;
            }
        }


    //@TODO Continue modify from here onward!

        //Async received data callback method
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                //Retrieve the NetObject and the client socket from the asynchronous state object
                NetStateObject netSatetObj = (NetStateObject)ar.AsyncState;
                Socket handler = netSatetObj.socket;
                //read data from the remote server
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // Versione string builder
                    //netObj.receivedMex.Append(Encoding.ASCII.GetString(netObj.recBuffer, 0, bytesRead));

                    netSatetObj.receivedMex = Encoding.ASCII.GetString(netSatetObj.recBuffer, 0, bytesRead);

                    // Verifico che il messaggio sia arrivato tutto cercando il trailer
                    string recMex = netSatetObj.receivedMex.ToString();

                    if (recMex.IndexOf(NetMessages.messageTrailer) > -1)
                    {
                        //Se l'intero messaggio è arrivato allora lo rendo disponibile in uscita e sollevo evento
                        this._receivedMex = recMex;
                        OnMessageReceived();
                        /* Versione con string builder
                            * //Svuoto il buffer di ricezione
                            * netObj.receivedMex.Remove(0, netObj.receivedMex.Length);
                            * */

                        //Rimango in ascolto di altri messaggi
                        netSatetObj.socket.BeginReceive(netSatetObj.recBuffer, 0, netSatetObj.bufferSize, 0, new AsyncCallback(ReceiveCallback), netSatetObj);
                    }
                    //else
                    //{
                    //    //Se rimangono dati da ricevere rimane in ascolto
                    //    netObj.socket.BeginReceive(netObj.recBuffer, 0, netObj.bufferSize, 0, new AsyncCallback(ReceiveCallback), netObj);
                    //}
                }
            }
            catch (Exception ex)
            {
                _log = "Error: " + ex.Message;
            }
        }

        /// <summary>
        /// Funzione usata per spedire messaggi di protocollo
        /// </summary>
        /// <param name="mess">messaggio di protocollo da inviare</param>
        public void SendMessage(string mess)
        {
            if (this._remote.Connected)
            {
                string message = NetMessages.messageHeader + NetMessages.token + mess + NetMessages.token + NetMessages.messageTrailer;
                byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._remote.Send(sendingData, sendingData.Length, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to send message {0}", ex.ToString());
                }
            }
        }

        /// <summary>
        /// Funzione usata per spedire i valori dell'accelerazione dell'ambiente
        /// </summary>
        /// <param name="data1">Primo dato</param>
        /// <param name="data2">Secondo dato</param>
        /// <param name="data3">Terzo dato</param>
        public void SendAccelerationData(float data1, float data2, float data3)
        {
            if (this._remote.Connected)
            {
                string message = NetMessages.accelerationHeader + NetMessages.token + data1.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data2.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data3.ToString(CultureInfo.InvariantCulture) + NetMessages.token + NetMessages.messageTrailer;
                Byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._remote.Send(sendingData, sendingData.Length, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to send data {0}", ex.ToString());
                }

            }
        }

        /// <summary>
        /// Funzione usata per spedire parametri della simulazione
        /// </summary>
        /// <param name="startingTime">Tempo dopo il quale inizierà il terremoto</param>
        /// <param name="totalDuration">durata totale del terremoto</param>
        /// <param name="peakTime">Tempo di inizio del picco</param>
        /// <param name="peakDuration">durata del picco</param>
        /// <param name="peakAmplitude">ampiezza del picco</param>
        public void StartSimulationWithParameters(float startingTime, float totalDuration, float peakTime, float peakDuration, float peakAmplitude)
        {
            if (this._remote.Connected)
            {
                string message = NetMessages.startSignal + NetMessages.token +
                    startingTime.ToString(CultureInfo.InvariantCulture) + NetMessages.token +
                    totalDuration.ToString(CultureInfo.InvariantCulture) + NetMessages.token +
                    peakTime.ToString(CultureInfo.InvariantCulture) + NetMessages.token +
                    peakDuration.ToString(CultureInfo.InvariantCulture) + NetMessages.token +
                    peakAmplitude.ToString(CultureInfo.InvariantCulture);

                this.SendMessage(message);
            }
        }

        /// <summary>
        /// Method used to stop the simulation
        /// </summary>
        public void StopSimulation()
        {
            if (this._remote.Connected)
            {
                string message = NetMessages.stopSignal;
                this.SendMessage(message);
            }
        }

        /// <summary>
        /// Funzione usata per spedire posizione utente
        /// </summary>
        /// <param name="data1">Primo dato</param>
        /// <param name="data2">Secondo dato</param>
        /// <param name="data3">Terzo dato</param>
        public void SendKinectData(float data1, float data2, float data3)
        {
            if (this._remote.Connected)
            {
                string message = NetMessages.userPositionHeader + NetMessages.token + data1.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data2.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data3.ToString(CultureInfo.InvariantCulture) + NetMessages.token + NetMessages.messageTrailer;
                Byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._remote.Send(sendingData, sendingData.Length, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to send data {0}", ex.ToString());
                }

            }
        }

        public int SendVarData( string mex )
        {
            int total = 0;
            int size = mex.Length;
            int dataleft = size;
            int sent;
            Byte[] dataSize = System.Text.Encoding.ASCII.GetBytes(size.ToString());
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(mex.ToString());


            sent = _remote.Send(dataSize);

            sent = _remote.Send(data);

            //while (total < size)
            //{
            //    sent = _remote.Send(data, total, dataleft, SocketFlags.None);
            //    total += sent;
            //    dataleft -= sent;
            //}
            return sent;

        }

        /// <summary>
        /// Close the tcp connection
        /// </summary>
        public void closeConnection()
        {
            if (this._remoteIsConnected)
            {
                //Segnalo connessione disconnessione del client
                this._remote.Shutdown(SocketShutdown.Both);
                this._remote.Disconnect(true);
                this._remote.Close();
                this._remote = null;

                this._listener.Close();
                this._listener = null;


                this.OnConnectionStateChanged();
            }
        }
        
        #endregion
    }
}






