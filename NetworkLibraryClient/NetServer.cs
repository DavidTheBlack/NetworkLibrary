//Unity Network Server Plugin. 
//It provide a low level network communication interface using socket
//@Copyright Davide Galdo 



using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace NetworkLibrary
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
        private Socket _client;
        //Listening connection port
        private int _localPort; 

        public void startListen()
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
                        System.Windows.MessageBox.Show(_localAddressV4.ToString());
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
            /*
             * Debug
            Console.WriteLine("Listening on : [{0}] {1}", locHostName, _localAddressV4.ToString());
             */
            //Create a new listener socket on localPort and localAddress
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(_localAddressV4, _localPort));
            listener.Listen(1);

            listener.BeginAccept(new AsyncCallback(this.OnConnectRequest), listener);
        }

        private void OnConnectRequest(IAsyncResult ar)
        {

            Socket listener = (Socket)ar.AsyncState;
            _client = listener.EndAccept(ar);
            Console.WriteLine("Client {0} , joined", _client.RemoteEndPoint);
        }

        public void SendData(float x, float y, float z)
        {
            string message = "Data|" + x.ToString(CultureInfo.InvariantCulture) + "|" + y.ToString(CultureInfo.InvariantCulture) + "|" + z.ToString(CultureInfo.InvariantCulture) + "|<EOM>";
            Byte[] sendingData = System.Text.Encoding.ASCII.GetBytes(message);
            _client.Send(sendingData, sendingData.Length, 0);
            Console.WriteLine(message + "\n");

        }

    }
}