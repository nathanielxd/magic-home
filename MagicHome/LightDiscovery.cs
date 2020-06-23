using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicHome
{
    /// <summary>Class used to discover lights on the network. </summary>
    /// <example>Here is an example on how to get all Magic Home lights available on the LAN.
    /// <code>
    /// var lights = LightDiscovery.DiscoverAsync();
    /// </code>
    /// </example>
    public class LightDiscovery
    {
        //Properties
        ///<summary>How long will the discovery take in milliseconds.</summary>
        public static int Timeout { get; set; } = 1000;

        private static List<Light> Lights { get; set; } = new List<Light>();
        private static IPEndPoint ep = new IPEndPoint(IPAddress.Any, DISCOVERY_PORT);
        private static UdpClient socket = new UdpClient(DISCOVERY_PORT);

        //Constants
        private const int DISCOVERY_PORT = 48899;
        private const string DISCOVERY_MESSAGE = "HF-A11ASSISTHREAD";

        //This method is fired whenever we receive a response back.
        private static void Receive(IAsyncResult result)
        {
            byte[] data = socket.EndReceive(result, ref ep);
            string message = Encoding.UTF8.GetString(data);

            //Handle discovered address.
            if (message != DISCOVERY_MESSAGE)
            {
                string address = message.Split(',')[0];
                Lights.Add(new Light(address));
            }

            //Start receiving data once again.
            socket.BeginReceive(Receive, new object());
        }

        private static void Send()
        {
            //Start receiving data.
            socket.BeginReceive(Receive, new object());

            //Send discovery message.
            var data = Encoding.UTF8.GetBytes(DISCOVERY_MESSAGE);
            socket.Send(data, data.Length, "255.255.255.255", DISCOVERY_PORT);
        }

        /// <summary>
        /// Send a broadcast message on the LAN to get all available lights.
        /// This operation will take 1 second by default. You can modify the timeout property to change the execution time.
        /// </summary>
        /// <returns>A list of available lights.</returns>
        public static async Task<List<Light>> DiscoverAsync()
        {
            Lights.Clear();
            Send();
            await Task.Delay(Timeout);
            return Lights;
        }
    }
}
