

namespace NetworkLibraryMessages
{
    static public class NetMessages
    {
        /*Protocol Messages*/

        /// <summary>
        /// Header used for sending protocol message
        /// </summary>
        public static string messageHeader = "<M>";
        /// <summary>
        /// header used for sending User position
        /// </summary>
        public static string userPositionHeader = "<U>";
        /// <summary>
        /// Header used for sending ground acceleration
        /// </summary>
        public static string accelerationHeader = "<A>";
        //header used for close a message
        public static string messageTrailer = "<E>";
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
        //Message sent for comunicate that the endpoint is ready to receive messages
        public static string readyToReceive = "ReadyToRec";
        //Message sent to comunicate the endpoint disconnection
        public static string disconnectRequest =  "Disconnect";
        //Message sent to comunicate that the player is lost
        public static string playerIsLost = "PlayerLost";
        //Communicate to load the indoor envinronment
        public static string loadIndoor = "LoadIndoor";
        //Communicate to load the outdoor envinronment
        public static string loadOutdoor = "LoadOutdoor";
        //Communicate to load the intro envinronment
        public static string loadIntro = "LoadIntro";


    }
}
