using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Globalization;
using NetworkLibraryMessages;
using NetworkStateObject;


namespace NetworkLibraryServer 
{
    public class NetServer
    {
        private string _locHostName;
        private IPAddress[] _localAddresses;
        private IPAddress _localAddressV4;
        public IPAddress localAddressV4
        {
            get { return _localAddressV4; }
        }

        //Listening connection port
        private int _localPort;

        //Client Socket Serve avere anche questo handler per la trasmissione sincrona
        private Socket _client;
        
        //Socket asincrono
        private Socket _listener;

        private bool _clientIsConnected = false;
        public bool clientIsConnected
        {
            get { return this._clientIsConnected; }
        }

        //Used to log special event
        private string _log = string.Empty;
        public string log
        {
            get { return _log; }
        }


        //Stringa ricevuta
        private string _receivedMex = string.Empty;
        public string receivedMex
        {
            get { return _receivedMex; }
        }


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
            //Aggiorno lo stato della connessione tcp
            this._clientIsConnected = !this._clientIsConnected;
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
        //Constructor
        public NetServer(int port)
        {
            this._localPort = port;
        }

        /// <summary>
        /// Funzione che mette in ascolto l'oggetto in attesa di connessioni TCP 
        /// </summary>
        public void waitForTcpConnection()
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
                Console.WriteLine("Error trying to get local address {0}", ex.ToString());
            }

            // Verify we got an IP address. Tell the user if we did
            if (_localAddressV4 == null)
            {
                Console.WriteLine("Unable to get local address");
                return;
            }

            //Edit hai reso listener privato a livello di classe
            //Create a new listener socket on localPort and localAddress
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                _listener.Bind(new IPEndPoint(_localAddressV4, _localPort));
                _listener.Listen(1); //Accetta connessioni da un solo socket per volta
                _listener.BeginAccept(new AsyncCallback(this.TcpConnectionCallback), _listener);

            
        }


        /// <summary>
        /// Finalizza la connessione, quando la connessione TCP è stabilita solleva evento OnConnectionStateChanged
        /// </summary>
        /// <param name="ar"></param>
        private void TcpConnectionCallback(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            //Salvo il socket per poterlo usare per inviare dati in maniera sincrona
            this._client = handler;
            

            NetStateObject netStateObject = new NetStateObject();
            netStateObject.socket = handler;
            //Mette in ascolto l'oggetto per comunicazioni 
            handler.BeginReceive(netStateObject.recBuffer, 0, netStateObject.bufferSize, 0,
                new AsyncCallback(ReceiveCallback), netStateObject);

            this.OnConnectionStateChanged();
        }

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
        /// Funzione usata per spedire dati al client
        /// </summary>
        /// <param name="data1">Primo dato</param>
        /// <param name="data2">Secondo dato</param>
        /// <param name="data3">Terzo dato</param>
        public void SendData(float data1, float data2, float data3)
        {
            if (this._client.Connected)
            {
                string message = NetMessages.userPositionHeader + NetMessages.token +data1.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data2.ToString(CultureInfo.InvariantCulture) + NetMessages.token + data3.ToString(CultureInfo.InvariantCulture) + NetMessages.token +NetMessages.messageTrailer;
                Byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._client.Send(sendingData, sendingData.Length, 0);
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
            if (this._client.Connected)
            {
                string message = NetMessages.messageHeader + NetMessages.token + mess + NetMessages.token + NetMessages.messageTrailer;
                byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
                try
                {
                    this._client.Send(sendingData, sendingData.Length, 0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error trying to send message {0}", ex.ToString());
                }

            }
        }

        /// <summary>
        /// Close the tcp connection
        /// </summary>
        public void closeConnection()
        {
            if (this._clientIsConnected)
            {
                //Segnalo connessione disconnessione del client
                

                this._client.Shutdown(SocketShutdown.Both);
                this._client.Disconnect(true);
                this._client.Close();
                this._client = null;

                
                this._listener.Close();
                this._listener = null;


                this.OnConnectionStateChanged();
            }
        }
        #endregion
    }
}

