using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MagicHome
{
    public enum TransitionType { Gradual = 0x3a, Strobe = 0x3c, Jump = 0x3b }
    public enum PresetPattern
    {
        SevenColorsCrossFade = 0x25,
        RedGradualChange = 0x26,
        GreenGradualChange = 0x27,
        BlueGradualChange = 0x28,
        YellowGradualChange = 0x29,
        CyanGradualChange = 0x2a,
        PurpleGradualChange = 0x2b,
        WhiteGradualChange = 0x2c,
        RedGreenCrossFade = 0x2d,
        RedBlueCrossFade = 0x2e,
        GreenBlueCrossFade = 0x2f,
        SevenColorStrobeFlash = 0x30,
        RedStrobeFlash = 0x31,
        GreenStrobeFlash = 0x32,
        BlueStrobeFlash = 0x33,
        YellowStrobeFlash = 0x34,
        CyanStrobeFlash = 0x35,
        PurpleStrobeFlash = 0x36,
        WhiteStrobeFlash = 0x37,
        SevenColorsJumping = 0x38
    }
    public enum LedProtocol { LEDENET, LEDENET_ORIGINAL }
    public enum LightMode { Color, Preset, White, Custom, Unknown }

    public class Light
    {
        /// <summary> The socket of the light </summary>
        public Socket Socket { get; private set; }
        /// <summary> The endpoint of the light </summary>
        public IPEndPoint Ep { get; private set; }
        /// <summary> The logging functionality of the light </summary>
        public LightLogger Logger { get; set; }

        /// <summary> Specifies whether or not to append checksum to outgoing requests. </summary>
        public bool UseCsum { get; set; }

        public static List<string> DiscoveredAdresses { get; set; }

        ///<summary> The time, status, protocol, color, brightness, mode in a single string. </summary>
        public string Stats { get; private set; }

        /// <summary> The date time of the light. </summary>
        public DateTime Time { get; private set; }
        /// <summary> The protocol of the light. </summary>
        public LedProtocol Protocol { get; private set; }
        /// <summary> The color of the light. </summary>
        public RGB Color { get; private set; }
        /// <summary> The brightness of the light, in percentage. </summary>
        public byte Brightness { get; private set; }
        /// <summary> Specifies whether the light is on or off. </summary>
        public bool IsOn { get; private set; }
        /// <summary> Specifies the mode of the light (Color, Preset, White, Custom) </summary>
        public LightMode Mode { get; private set; }

        private const int PORT = 5577;
        private const int DISCOVERY_PORT = 48899;
        private const string DISCOVERY_MSG = "HF-A11ASSISTHREAD";

        /// <summary>
        /// Creates a new endpoint and connects to the bulb on the specified local ip address.
        /// </summary>
        public Light(string ip)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress Ip = IPAddress.Parse(ip);
            Ep = new IPEndPoint(Ip, PORT);

            Directory.CreateDirectory(Assembly.GetExecutingAssembly().Location + "\\.." + "\\Logs");
            Logger = new LightLogger(this);

            UseCsum = true;
            Connect();
            GetStatus();
        }

        ///<summary>
        ///Discovers all lights on the network and puts them in a List.
        ///Timeout is 5 seconds by default.
        ///Refrain from using this too often since it's slow.
        ///</summary>
        public static List<Light> Discover(int timeout = 5)
        {
            DiscoveredAdresses = new List<string>();
            List<Light> lightList = new List<Light>();
            timeout += 1000;

            UdpClient udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DISCOVERY_PORT));

            var recvEp = new IPEndPoint(0, 0);
            Task.Run(() =>
            {
                DateTime quitTime = DateTime.Now.AddMilliseconds(timeout);

                while (true)
                {
                    if (DateTime.Now > quitTime)
                        break;

                    var recvBuffer = udpClient.Receive(ref recvEp);
                    var recvData = Encoding.UTF8.GetString(recvBuffer);
                    if (recvData != DISCOVERY_MSG)
                    {
                        string recvIp = recvData.Split(',')[0];
                        DiscoveredAdresses.Add(recvIp);
                        lightList.Add(new Light(recvIp));
                    }
                }
            });

            var data = Encoding.UTF8.GetBytes(DISCOVERY_MSG);
            udpClient.Send(data, data.Length, "255.255.255.255", DISCOVERY_PORT);

            Thread.Sleep(timeout);
            return lightList;
        }

        /// <summary> Turns the light on </summary>
        public void TurnOn()
        {
            if (Protocol == LedProtocol.LEDENET)
                SendData(new byte[] { 0x71, 0x23, 0x0f });
            else
                SendData(new byte[] { 0xcc, 0x23, 0x33 });
            IsOn = true;

            Log("turned on");
        }

        /// <summary> Turns the light off (without disconnecting it). </summary>
        public void TurnOff()
        {
            if (Protocol == LedProtocol.LEDENET)
                SendData(new byte[] { 0x71, 0x24, 0x0f });
            else
                SendData(new byte[] { 0xcc, 0x24, 0x33 });
            IsOn = false;

            Log("turned off");
        }

        /// <summary> Sets the color of the light in a RGB manner. </summary>
        public void SetColor(byte R, byte G, byte B)
        {
            if (Protocol == LedProtocol.LEDENET)
                SendData(new byte[] { 0x41, R, G, B, 0x00, 0x00, 0x0f });
            else
                SendData(new byte[] { 0x56, R, G, B, 0xaa });

            RGB placeholder = new RGB(R, G, B);
            Color = placeholder;
            UpdateBrightness();
            Mode = LightMode.Color;

            Log("changed colors to " + R + " " + G + " " + B);
        }

        // Deprecated
        public void SetColor(RGB color)
        {
            if (Protocol == LedProtocol.LEDENET)
                SendData(new byte[] { 0x41, color.red, color.green, color.blue, 0x00, 0x00, 0x0f });
            else
                SendData(new byte[] { 0x56, color.red, color.green, color.blue, 0xaa });

            Color = color;
            Mode = LightMode.Color;
            UpdateBrightness();

            Log("changed colors to " + color.red + " " + color.green + " " + color.blue);
        }

        // Check if works
        public void SetWarmWhite(byte white)
        {
            if (Protocol == LedProtocol.LEDENET)
                SendData(new byte[] { 0x31, 0, 0, 0, white, 0x0f, 0x0f });
            else
            {
                Log("Couldn't apply warm white on non-RGBWW lights. Switched to cold white.");
                SetWhite(white);
            }

            Color = new RGB(white, white, white);
            Mode = LightMode.White;
            UpdateBrightness();


            Log("changed to warm white color: " + white);

        }

        // Bad stuff
        public void SetWhite(byte white)
        {
            SetColor(white, white, white);
            Log("changed to white color: " + white);
        }

        //Bad stuff
        public void SetBrightness(byte brightness)
        {
            RGB newColor = new RGB();
            if (Brightness == 0)
                TurnOff();
            else
            {
                if (brightness > 99) brightness = 99; // Prevent an exception
                newColor.red = Convert.ToByte((Color.red * brightness) / Brightness);
                newColor.green = Convert.ToByte((Color.green * brightness) / Brightness);
                newColor.blue = Convert.ToByte((Color.blue * brightness) / Brightness);

                SetColor(newColor);
                Brightness = brightness;

                Log("changed brightness to: " + brightness);
            }
        }

        /// <summary> Sets the light a preset pattern. </summary>
        public void SetPresetPattern(PresetPattern pattern, byte speed)
        {
            byte delay = Utilis.SpeedToDelay(speed);
            SendData(new byte[] { 0x61, Convert.ToByte(pattern), delay, 0x0f });

            Mode = LightMode.Preset;

            Log("changed to preset pattern: " + pattern.ToString());
        }

        /// <summary> 
        /// Sets the light a custom pattern.
        /// Use an array of RGB objects to assign a list of colors the light will cycle through.
        /// Specify the transition type (Gradual, Strobe, Jump) and a speed.
        /// <param name="speed"> How quick the light will cycle through the pattern, in percentage. </param>
        /// </summary>
        public void SetCustomPattern(RGB[] list, TransitionType transition, byte speed)
        {
            List<byte> byData = new List<byte>();
            bool firstbyte = true;

            for (int i = 0; i < list.Length; i++)
            {
                if (firstbyte == true)
                {
                    byData.Add(0x51);
                    firstbyte = false;
                }
                else
                    byData.Add(0);

                byData.AddRange(new byte[] { list[i].red, list[i].green, list[i].blue });
            }

            for (int i = 0; i < 16 - list.Length; i++)
                byData.AddRange(new byte[] { 0, 1, 2, 3 });

            byData.AddRange(new byte[] { 0x00, Utilis.SpeedToDelay(speed), Convert.ToByte(transition), 0xff, 0x0f });

            byte[] byDataReady = byData.ToArray();
            SendData(byDataReady);

            Mode = LightMode.Custom;

            Log(" changed to custom pattern.");
        }

        /// <summary> Sets the date and time of the light. Leave null for the current system date (DateTime.Now). </summary>
        public void SetTime(DateTime? dateTime = null)
        {
            if (dateTime == null)
                dateTime = DateTime.Now;

            SendData(new byte[] { 0x10, 0x14,
                Convert.ToByte(dateTime.Value.Year - 2000),
                Convert.ToByte(dateTime.Value.Month),
                Convert.ToByte(dateTime.Value.Day),
                Convert.ToByte(dateTime.Value.Hour),
                Convert.ToByte(dateTime.Value.Minute),
                Convert.ToByte(dateTime.Value.Second),
                Convert.ToByte(dateTime.Value.DayOfWeek),
                0x00, 0x0f }
            );
        }

        /// <summary> Updates this object with current light's mode, time, status, protocol, color, brightness. </summary>
        public void Refresh()
        {
            GetStatus();
        }

        private void GetStatus()
        {
            Protocol = GetProtocol();

            if (Protocol == LedProtocol.LEDENET)
                SendData(new byte[] { 0x81, 0x8a, 0x8b });
            else
                SendData(new byte[] { 0xef, 0x01, 0x77 });

            var dataRaw = ReadData();
            string[] dataHex = new string[14];
            for (int i = 0; i < dataHex.Length; i++)
                dataHex[i] = dataRaw[i].ToString("X");

            if (Protocol == LedProtocol.LEDENET_ORIGINAL)
                if (dataHex[1] == "01")
                    UseCsum = false;

            if (dataHex[2] == "23")
                IsOn = true;
            else if (dataHex[2] == "24")
                IsOn = false;

            Mode = Utilis.DetermineMode(dataHex[3]);
            if (Mode == LightMode.Color)
            {
                Color = new RGB(dataRaw[6], dataRaw[7], dataRaw[8]);
            }
            if(Mode == LightMode.White)
            {
                Color = new RGB(0, 0, 0);
            }
            if(Mode == LightMode.Preset || Mode == LightMode.Custom)
            {
                Color = new RGB(0, 0, 0);
            }

            Time = GetTime();

            UpdateBrightness();

            Stats = Ep.Address + " " + IsOn.ToString() + " " + Protocol.ToString() + " " + Mode.ToString() + " " +
                Color.red + " " + Color.green + " " + Color.blue + " " + Brightness + " " + Time.ToLongTimeString();
        }

        /// <summary> Gets the time of the light. </summary>
        private DateTime GetTime(int retries = 2)
        {
            if (retries > 0)
            {
                SendData(new byte[] { 0x11, 0x1a, 0x1b, 0x0f });
                byte[] data = ReadData();

                try
                {
                    Time = new DateTime(
                        Convert.ToInt32(data[3]) + 2000,
                        Convert.ToInt32(data[4]),
                        Convert.ToInt32(data[5]),
                        Convert.ToInt32(data[6]),
                        Convert.ToInt32(data[7]),
                        Convert.ToInt32(data[8])
                    );
                }
                catch
                {
                    GetTime(retries - 1);
                    Log("Problem retrieving time data");
                }
            }
            return Time;
        }

        /// <summary> Gets the protocol of the light. </summary>
        private LedProtocol GetProtocol()
        {
            SendData(new byte[] { 0x81, 0x8a, 0x8b });
            try
            {
                var recv_default = ReadData();
                return LedProtocol.LEDENET;
            }
            catch (SocketException)
            {
                SendData(new byte[] { 0xef, 0x01, 0x77 });
                try
                {
                    var recv_originalLEDNET = ReadData();
                    return LedProtocol.LEDENET_ORIGINAL;
                }
                catch (SocketException)
                {
                    Log("No protocol detected :(");
                    throw new Exception("ProtocolException: Could not detect protocol");
                }
            }
        }

        /// <summary> Updates the brightness property of the object based on it's colors. </summary>
        private void UpdateBrightness()
        {
            Brightness = Utilis.DetermineBrightness(Color.red, Color.green, Color.blue);
        }

        /// <summary> Connects the socket to the endpoint. </summary>
        private void Connect(int retries = 2)
        {
            if (retries > 0)
            {
                Socket.Connect(Ep);

                if (Socket.Connected)
                    Log("connected successfully");
                else
                    Log("failed connecting. Retrying...");
            }
            else
            {
                Log("failed connecting. Please ensure the light is on the same network.");
            }
        }

        private void Log(string info)
        {
            Logger.Log(info);
        }

        /// <summary> Sends data to the light. </summary>
        private void SendData(byte[] _byData)
        {
            List<byte> byData = new List<byte>();
            byData.AddRange(_byData);

            if (UseCsum == true)
            {
                byte csum = 0;
                for (int i = 0; i < byData.Count; i++)
                    csum += byData[i];
                csum = Convert.ToByte(csum & 0xFF);
                byData.Add(csum);
            }
            byte[] byDataReady = byData.ToArray();

            Socket.Send(byDataReady);
        }

        /// <summary> Reads data from the light. </summary>
        private byte[] ReadData(int retries = 2, int timeout = 1000)
        {
            byte[] buffer = new byte[14];
            Socket.ReceiveTimeout = timeout;

            if (retries > 0)
            {
                try {
                    int receiver = Socket.Receive(buffer);
                }
                catch {
                    ReadData(retries - 1);
                }
            }
            else
            {
                Log("Data could not be read. ");
            }

            return buffer;
        }
    }
}
