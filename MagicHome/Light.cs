using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MagicHome
{
    public enum TransitionType { Gradual = 0x3a, Strobe = 0x3c, Jump = 0x3b }
    public enum LedProtocol { LEDENET, LEDENET_ORIGINAL, Unknown }
    public enum LightMode { Color, WarmWhite, Preset, Custom, Unknown }
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

    public class Light
    {
        //Networking

        /// <summary> The network socket of the light. </summary>
        private readonly Socket Socket;
        /// <summary> The network endpoint of the light. </summary>
        private readonly IPEndPoint Ep;

        //Properties

        /// <summary> Whether the socket is connected or not. </summary>
        public bool Connected { get; private set; } = false;
        /// <summary> The date and time of the light. </summary>
        public DateTime Time { get; private set; }
        /// <summary> The protocol of the light. </summary>
        public LedProtocol Protocol { get; private set; } = LedProtocol.Unknown;
        /// <summary> The color of the light. </summary>
        public Color Color { get; private set; }
        /// <summary> The warm white value of the light. </summary>
        public byte WarmWhite { get; private set; }
        /// <summary> The brightness of the light, from 0 to 100. </summary>
        public byte Brightness { get; private set; }
        /// <summary> Specifies whether the light is on or off. </summary>
        public bool Power { get; private set; }
        /// <summary> Specifies the mode of the light (Color, Preset, White, Custom). </summary>
        public LightMode Mode { get; private set; }

        /// <summary>
        /// Specifies whether or not to append checksum to outgoing requests.
        /// True by default.
        /// </summary>
        public bool UseCsum { get; set; } = true;

        /// <summary>The receive timeout of the socket in milliseconds.</summary>
        public int Timeout { get; set; } = 1000;

        //Constants
        private const int PORT = 5577;

        /// <summary> Creates a new endpoint and connects to the bulb on the specified local ip address. </summary>
        /// <param name="ip"> The IP address string of the light bulb. </param>
        public Light(string ip)
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.ReceiveTimeout = Timeout;

            IPAddress Ip = IPAddress.Parse(ip);
            Ep = new IPEndPoint(Ip, PORT);
        }

        /// <summary> Connects the socket to the endpoint and sends a request to get light's status.
        /// This function will call RefreshAsync();
        /// </summary>
        public async Task ConnectAsync()
        {
            await Socket.ConnectAsync(Ep);
            Protocol = await GetProtocolAsync();
            Connected = true;
            await RefreshAsync();
        }

        /// <summary> Discover Magic Home lights on the network. Uses the LightDiscovery class.</summary>
        /// <example> Here is an example on how to get all Magic Home lights available on the LAN.
        /// <code> var lights = Light.DiscoverAsync(); </code>
        /// </example>
        public static Task<List<Light>> DiscoverAsync() => LightDiscovery.DiscoverAsync();

        /// <summary> Sends a request to get the light's status.
        /// Updates this instance with current bulbs's mode, time, status, protocol, color, brightness.
        /// 
        /// This operation usually takes between 80 and 500 milliseconds.
        /// </summary>
        public async Task RefreshAsync()
        {
            //Send request for status.
            if (Protocol == LedProtocol.LEDENET)
                await SendDataAsync(0x81, 0x8a, 0x8b);
            else if (Protocol == LedProtocol.LEDENET_ORIGINAL)
                await SendDataAsync(0xef, 0x01, 0x77);

            //Read and process the response.
            var dataRaw = await ReadDataAsync();
            string[] dataHex = new string[14];
            for (int i = 0; i < dataHex.Length; i++)
                dataHex[i] = dataRaw[i].ToString("X");

            //Check if it uses checksum.
            if (Protocol == LedProtocol.LEDENET_ORIGINAL)
                if (dataHex[1] == "01")
                    UseCsum = false;

            //Check power state.
            if (dataHex[2] == "23")
                Power = true;
            else if (dataHex[2] == "24")
                Power = false;

            //Check light mode.
            Mode = Utilis.DetermineMode(dataHex[3], dataHex[9]);

            //Handle color property.
            switch (Mode)
            {
                case LightMode.Color:
                    Color = new Color(dataRaw[6], dataRaw[7], dataRaw[8]);
                    WarmWhite = 0;
                    break;
                case LightMode.WarmWhite:
                    Color = Colors.Empty;
                    WarmWhite = dataRaw[9];
                    break;
                case LightMode.Preset:
                case LightMode.Unknown:
                case LightMode.Custom:
                    Color = Colors.Empty;
                    WarmWhite = 0;
                    break;
            }

            UpdateBrightness();

            //Send request to get the time of the light.
            Time = await GetTimeAsync();
        }

        /// <summary>
        /// Sets the light ON or OFF.
        /// </summary>
        /// <param name="power">true to set the light ON, false to set the light OFF.</param>
        public async Task SetPowerAsync(bool power)
        {
            if (power)
            {
                await TurnOnAsync();
            }
            else await TurnOffAsync();
        }

        /// <summary> Turns the light ON. </summary>
        public async Task TurnOnAsync()
        {
            if (Protocol == LedProtocol.LEDENET)
                await SendDataAsync(0x71, 0x23, 0x0f);
            else
                await SendDataAsync(0xcc, 0x23, 0x33);

            Power = true;
        }

        /// <summary> Turns the light OFF. </summary>
        public async Task TurnOffAsync()
        {
            if (Protocol == LedProtocol.LEDENET)
                await SendDataAsync(0x71, 0x24, 0x0f);
            else
                await SendDataAsync(0xcc, 0x24, 0x33);

            Power = false;
        }

        /// <summary> Sets the color of the light. </summary>
        public async Task SetColorAsync(byte red, byte green, byte blue) 
            => await SetColorAsync(new Color(red, green, blue));

        /// <summary> Sets the color of the light. </summary>
        public async Task SetColorAsync(Color color)
        {
            if (Protocol == LedProtocol.LEDENET)
                await SendDataAsync(0x41, color.Red, color.Green, color.Blue, 0x00, 0x00, 0x0f);
            else
                await SendDataAsync(0x56, color.Red, color.Green, color.Blue, 0xaa);

            //Populate fields
            Color = color;
            WarmWhite = 0;
            UpdateBrightness();
        }

        /// <summary> Sets warm white. If the light does not support warm white, will set cold white instead. </summary>
        public async Task SetWarmWhiteAsync(byte white)
        {
            if (Protocol == LedProtocol.LEDENET)
                await SendDataAsync(0x31, 0, 0, 0, white, 0x0f, 0x0f);
            else
                await SetColdWhiteAsync(white);

            //Populate field
            WarmWhite = white;
            Color = Colors.Empty;
            UpdateBrightness();
        }

        /// <summary> Sets all the red, green and blue color to the same value to create cold white light. </summary>
        public async Task SetColdWhiteAsync(byte white) 
            => await SetColorAsync(white, white, white);

        
        /* TODO.
        /// <summary> Sets the brightness of the light. </summary>
        /// <param name="brightness"> The level of brightness, from 0 to 100. </param>
        public async Task SetBrightnessAsync(byte brightness)
        {
            if (brightness > 100) throw new MagicHomeException("Brightness value cannot be more than 100");
            if (Mode == LightMode.Color)
                await SetColorAsync(
                    Convert.ToByte(Color.Red * (brightness / Brightness)),
                    Convert.ToByte(Color.Green * (brightness / Brightness)),
                    Convert.ToByte(Color.Blue * (brightness / Brightness))
                    );
            else if (Mode == LightMode.WarmWhite)
                await SetWarmWhiteAsync(Convert.ToByte(WarmWhite * brightness / Brightness));

            UpdateBrightness();
        }*/

        /// <summary> Sets a preset pattern. </summary>
        /// <param name="speed"> The speed of the pattern from 0 to 100. </param>
        public async Task SetPresetPatternAsync(PresetPattern pattern, byte speed)
        {
            byte delay = Utilis.SpeedToDelay(speed);
            await SendDataAsync(new byte[] { 0x61, Convert.ToByte(pattern), delay, 0x0f });

            //Populate field.
            Mode = LightMode.Preset;
        }

        /// <summary> 
        /// Sets the light a custom pattern.
        /// Use a List of Color objects to assign a list of colors the light will cycle through.
        /// </summary>
        /// <param name="colors"> The list of colors that the light will cycle through. </param>
        /// <param name="transition"> The transition type (Gradual, Strobe, Jump). </param>
        /// <param name="speed"> How quick the light will cycle through the pattern, from 0 to 100. </param>
        public async Task SetCustomPatternAsync(List<Color> colors, TransitionType transition, byte speed) 
            => await SetCustomPatternAsync(colors.ToArray(), transition, speed);

        /// <summary> 
        /// Sets the light a custom pattern.
        /// Use an array of Color objects to assign a list of colors the light will cycle through.
        /// </summary>
        /// <param name="colors"> The array of colors that the light will cycle through. </param>
        /// <param name="transition"> The transition type (Gradual, Strobe, Jump). </param>
        /// <param name="speed"> How quick the light will cycle through the pattern, from 0 to 100. </param>
        public async Task SetCustomPatternAsync(Color[] colors, TransitionType transition, byte speed)
        {
            List<byte> data = new List<byte>();
            bool firstbyte = true;

            for (int i = 0; i < colors.Length; i++)
            {
                if (firstbyte == true)
                {
                    data.Add(0x51);
                    firstbyte = false;
                }
                else
                    data.Add(0);

                data.AddRange(new byte[] { colors[i].Red, colors[i].Green, colors[i].Blue });
            }

            for (int i = 0; i < 16 - colors.Length; i++)
                data.AddRange(new byte[] { 0, 1, 2, 3 });

            data.AddRange(new byte[] { 0x00, Utilis.SpeedToDelay(speed), Convert.ToByte(transition), 0xff, 0x0f });

            byte[] dataReady = data.ToArray();
            await SendDataAsync(dataReady);

            //Populate field.
            Mode = LightMode.Custom;
        }

        /// <summary> Sets the date and time of the light. Leave null for the current system date (DateTime.Now). </summary>
        public async Task SetTimeAsync(DateTime? dateTime = null)
        {
            if (dateTime == null)
                dateTime = DateTime.Now;

            await SendDataAsync(0x10, 0x14,
                Convert.ToByte(dateTime.Value.Year - 2000),
                Convert.ToByte(dateTime.Value.Month),
                Convert.ToByte(dateTime.Value.Day),
                Convert.ToByte(dateTime.Value.Hour),
                Convert.ToByte(dateTime.Value.Minute),
                Convert.ToByte(dateTime.Value.Second),
                Convert.ToByte(dateTime.Value.DayOfWeek),
                0x00, 0x0f);
        }
        
        /// <summary> Gets the time of the light. </summary>
        public async Task<DateTime> GetTimeAsync()
        {
            await SendDataAsync(0x11, 0x1a, 0x1b, 0x0f);
            byte[] data = await ReadDataAsync();
            Time = new DateTime(
                Convert.ToInt32(data[3]) + 2000,
                Convert.ToInt32(data[4]),
                Convert.ToInt32(data[5]),
                Convert.ToInt32(data[6]),
                Convert.ToInt32(data[7]),
                Convert.ToInt32(data[8])
            );

            return Time;
        }

        /// <summary> Gets the protocol of the light and returns it. </summary>
        private async Task<LedProtocol> GetProtocolAsync()
        {
            await SendDataAsync(0x81, 0x8a, 0x8b);
            try
            {
                byte[] buffer_ledenet = new byte[14];
                Socket.Receive(buffer_ledenet);
                return LedProtocol.LEDENET;
            }
            catch (SocketException)
            {
                await SendDataAsync(0xef, 0x01, 0x77);
                try
                {
                    byte[] buffer_original = new byte[14];
                    Socket.Receive(buffer_original);
                    return LedProtocol.LEDENET_ORIGINAL;
                }
                catch (SocketException)
                {
                    return LedProtocol.Unknown;
                }
            }
        }

        /// <summary> Updates the brightness property of this instance based on it's colors or warm white value. </summary>
        private void UpdateBrightness()
        {
            if (Mode == LightMode.Color)
                Brightness = Utilis.DetermineBrightness(Color.Red, Color.Green, Color.Blue);
            else if (Mode == LightMode.WarmWhite)
                Brightness = Utilis.DetermineBrightness(WarmWhite, WarmWhite, WarmWhite);
        }

        /// <summary> Sends data to the light. </summary>
        private async Task SendDataAsync(params byte[] _data)
        {
            List<byte> data = new List<byte>();
            data.AddRange(_data);

            if (UseCsum == true)
            {
                byte csum = 0;
                for (int i = 0; i < data.Count; i++)
                    csum += data[i];
                csum = Convert.ToByte(csum & 0xFF);
                data.Add(csum);
            }

            await Socket.SendAsync(new ArraySegment<byte>(data.ToArray()), SocketFlags.None);
        }

        /// <summary> Reads data from the light socket and returns it. </summary>
        private async Task<byte[]> ReadDataAsync()
        {
            byte[] buffer = new byte[14];
            await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

            return buffer;
        }

        public override string ToString() 
            => "[" + Ep.Address + "]: " +
                "Power " + Power +
                ", Mode " + Mode +
                ", Color " + Color.ToString() +
                ", Warm White" + WarmWhite +
                ", Brightness " + Brightness +
                ", Protocol " + Protocol +
                ", Time " + Time;
    }
}
