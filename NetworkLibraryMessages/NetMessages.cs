

namespace NetworkLibraryMessages
{
    static public class NetMessages
    {
        //Application Protocol Packet Type
        //[packet Header][Payload Type]|[data1]|...|[dataN][packet Trailer]

        //Data packet
        //<D|user1|user2|user3|groundAccX|groundAccY|groundAccZ>

        // Protocol Information packet
        //<P|[message]>

            

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
        /// header used by acceleometer client to send data to server
        /// </summary>
        public static string accelerationPayload = "A";

        /// <summary>
        /// Header used for sending message
        /// </summary>
        public static string messagePayload = "M";

        /// <summary>
        /// Header used to comunicate the starting of the experiment
        /// </summary>
        public static string startSignal = "Start";

        /// <summary>
        /// Header used to comunicate the ending of the experiment
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
        /// Message sent to comunicate that the user is lost
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
