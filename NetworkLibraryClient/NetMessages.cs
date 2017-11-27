using System;


namespace NetworkLibrary
{
    static class NetMessages
    {
        /*Protocol Messages*/

        //Header used for sending protocol message
        public static string messageHeader = "MES";
        //header used for sending data
        public static string dataHeader = "DATA";
        //header used for close a message
        public static string messageTrailer = "<EOM>";
        //data separator
        public static string token = "|";
        //Message sent to request a Conection
        public static string connectionRequest = messageHeader + token + "ConnectionReq" + token + messageTrailer;
        //Message sent for comunicate that the endpoint is ready to receive messages
        public static string readyToReceive = messageHeader + token + "ReadyToReceive" + token + messageTrailer;
        //Message sent to comunicate the endpoint disconnection
        public static string disconnectRequest = messageHeader + token + "Disconnect" + token + messageTrailer;
    }
}
