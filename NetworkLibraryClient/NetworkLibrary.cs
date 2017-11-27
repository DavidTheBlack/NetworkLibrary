//Unity Network Client Plugin. 
//It provide a low level network communication interface using socket
//@Copyright Davide Galdo 

/* 
 * 1-Open Socket Connection 
 * 2-Open Protocol Connection 
 * 3-Receive Data
 * 4-Close Protocol connectio
 * 5-Close Socket Connection
 */

/*
 * TODO
 * Implementa la funzione di connessiont a livello protocollo.
 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;
using NetworkLibraryMessages;




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
        #endregion

        #region Events
        //Event of completed connection
        public ManualResetEvent connectDone = new ManualResetEvent(false);
        //Event of received data is completed
        public ManualResetEvent receiveDone = new ManualResetEvent(false);
        #endregion

        #region Methods

        //Constructor
        public NetClient(string ip, int port)
        {
            _serverIP = ip;
            _serverPort = port;
        }
        
        //Socket connection method
        private void openTcpConnection()
        {
            try
            {
                //Create new Network object
                NetObject netobj = new NetObject();
                //Create the remote endpoint for the socket
                IPEndPoint remoteEP=new IPEndPoint(IPAddress.Parse(_serverIP),_serverPort);
                //Create TCP/IP socket
                netobj.clientSocket= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //Start connection to remote endpoint
                netobj.clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), netobj);
            }
            catch (Exception ex)
            {
                this._log="Server is not active. \n Please start server and try again. \n" + ex.ToString();
            }
        }

        //Async connection callback method 
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                //Retrive network information
                NetObject netobj = (NetObject)ar.AsyncState;
                //complete the connections
                netobj.clientSocket.EndConnect(ar);
                //Start to listen for data from the server
                netobj.clientSocket.BeginReceive(netobj.recBuffer,0,netobj.bufferSize,0,new AsyncCallback(ReceiveCallback),netobj);

                // Convert the string data to byte data using ASCII encoding.
                byte[] byteData = Encoding.ASCII.GetBytes(NetworkMessages.readyToReceive);
                //Transmit the message
                netobj.clientSocket.Send(byteData);
                this._log = "Connection created";
                connectDone.Set();
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
                NetObject netObj = (NetObject)ar.AsyncState;

                //read data from the remote server
                int bytesRead = netObj.clientSocket.EndReceive(ar);

                if (bytesRead != 0) { 
                    //Encoding received buffer in ascii 
                    this._receivedMex=Encoding.ASCII.GetString(netObj.recBuffer,0,bytesRead);
                    //Signal that all bytes have been received
                    receiveDone.Set();
                }
                //Get other data.
                netObj.clientSocket.BeginReceive(netObj.recBuffer, 0, netObj.bufferSize, 0, new AsyncCallback(ReceiveCallback), netObj);

            }
            catch (Exception ex)
            {
                _log = "Error: " + ex.Message;
            }
        }

        //Application level connection method -> Livello applicazione
        public openConnection(){

        }

        //public void closeConnection()
        //{
        //    SendData("DISCONNECT");
        //}


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


        
        #endregion
    }

}

