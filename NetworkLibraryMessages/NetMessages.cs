

namespace NetworkLibraryMessages
{
    static public class NetMessages
    {
        /*Application Protocol Messages*/
        //[packet Header][Payload Type]|[data1]|...|[dataN]|[packet Trailer]
        //
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
        /// header used for sending data (user position and table acceleration
        /// </summary>
        public static string dataPayload = "D";

        /// <summary>
        /// Header used for sending protocol message
        /// </summary>
        public static string protocolHeader = "P";



        
        /// <summary>
        /// Header used to comunicate the starting of the experiment
        /// </summary>
        public static string startSignal = "Start";

        /// <summary>
        /// Header used to comunicate the ending of the experiment
        /// </summary>
        public static string stopSignal = "Stop";

        //data separator
        public static string token = "|";

        //Message sent to accept a Conection
        public static string connectionAccepted = "ConnectionAccepted";

        //Message sent to comunicate the endpoint disconnection
        public static string disconnectRequest =  "Disconnect";

        //Message sent to comunicate that the player is lost
        public static string playerIsLost = "PlayerLost";

        //Communicate to load the intro envinronment
        public static string loadIntro = "LoadIntro";

        //Communicate to load the outdoor envinronment
        public static string loadOutdoor = "LoadOutdoor";

        //Communicate to load the indoor envinronment
        public static string loadIndoor = "LoadIndoor";




    }
}
