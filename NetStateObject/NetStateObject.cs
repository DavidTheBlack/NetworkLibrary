using System.Net.Sockets;

namespace NetworkStateObject
{
    // Network Class for receiving data from remote device.
    public class NetStateObject
    {
        //Client Socket
        public Socket socket = null;
        //Size of receive Buffer
        private int _bufferSize = 128;
        public int bufferSize
        {
            get { return _bufferSize; }
        }

        //Receive Buffer
        public byte[] recBuffer;
        //Received data String
        public string receivedMex = string.Empty;
        //public StringBuilder receivedMex = new StringBuilder();

        public NetStateObject()
        {
            this.recBuffer = new byte[this._bufferSize];
        }

        //Overloaded Constructor to be used if a user want to set Receiving Buffer Size
        public NetStateObject(int bufferSize)
        {
            this._bufferSize = bufferSize;
        }
    }
}
