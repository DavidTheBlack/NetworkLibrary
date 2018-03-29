//@Copyright Davide Galdo 2017

namespace QuakeSimulatorNetworkLibrary
{
    static public class NetMessages
    {
        //Application Protocol Packet Type
        //packetHeader + | + payloadType + | + data1 + [|]...[|] + [dataN] + | + packetTrailer

        //Data packet
        //<| D | userRecognized | userPositionX | userPositionY | userPositionZ | groundAccX | groundAccY | groundAccZ |>

        //Acceleration Packet
        //<| A | xAcc | yAcc | zAcc |>

        //Protocol Information packet
        //<| M | [message] |>



        /// <summary>
        /// Header used for sending package
        /// </summary>
        public static string packetHeader = "<";

        /// <summary>
        /// header used for close a message
        /// </summary>
        public static string packetTrailer = ">";

        /// <summary>
        /// header used by server for sending data to client (user position and table acceleration)
        /// </summary>
        public static string dataPayload = "D";

        /// <summary>
        /// header used by accelerometer client to send data to server
        /// </summary>
        public static string accelerationPayload = "A";

        /// <summary>
        /// Header used for sending message
        /// </summary>
        public static string messagePayload = "M";

        /// <summary>
        /// Header used to communicate the starting of the experiment
        /// </summary>
        public static string startSignal = "Start";

        /// <summary>
        /// Header used to communicate the ending of the experiment
        /// </summary>
        public static string stopSignal = "Stop";

        /// <summary>
        /// Data separator
        /// </summary>
        public static string token = "|";

        /// <summary>
        /// Message sent to accept the connection
        /// </summary>
        public static string connectionAccepted = "ConnectionAccepted";

        /// <summary>
        /// Message sent to comunicate the endpoint disconnection 
        /// </summary>
        public static string disconnectRequest =  "Disconnect";

        /// <summary>
        /// Message sent to communicate that it's ready to receive data
        /// </summary>
        public static string readyToReceive = "ReadyToRec";

        /// <summary>
        /// Message sent to communicate that the user is lost
        /// </summary>
        public static string userLost = "UserLost";

        /// <summary>
        /// Message sent to communicate that the user has been recognized
        /// </summary>
        public static string userRecognized = "UserRecognized";

        /// <summary>
        /// Communicate to load the intro envinronment
        /// </summary>
        public static string loadIntro = "LoadIntro";

        /// <summary>
        /// Communicate to load the outdoor envinronment 
        /// </summary>
        public static string loadOutdoor = "LoadOutdoor";

        /// <summary>
        /// Communicate to load the indoor envinronment
        /// </summary>
        public static string loadIndoor = "LoadIndoor";




    }
}
