using System;

namespace MagicHome
{
    /// <summary> Different useful methods used in the library. </summary>
    internal static class Utilis
    {
        /// <summary> Transforms speed (0 to 100) to light specific 'delay' property (0 to 27). </summary>
        internal static byte SpeedToDelay(byte speed)
        {
            if (speed > 100)
                throw new MagicHomeException("Speed cannot have a value more than 100.");

            int inv_speed = 100 - speed;
            byte delay = Convert.ToByte((inv_speed * (0x1f - 1)) / 100);
            delay += 1;

            return delay;
        }

        /// <summary> Determines brightness by red, green and blue color values. </summary>
        /// <returns> Brightness level from 0 to 100. </returns>
        internal static byte DetermineBrightness(byte Red, byte Green, byte Blue)
        {
            int maxx = 0;

            if (Red > maxx) maxx = Red;
            if (Green > maxx) maxx = Green;
            if (Blue > maxx) maxx = Blue;

            maxx = maxx * 100 / 255;

            var brightness = Convert.ToByte(maxx);

            return brightness;
        }

        /// <summary> Determines the mode of the light according to a code given by the light. </summary>
        /// <returns> Mode of the light. </returns>
        internal static LightMode DetermineMode(string patternCode, string whiteCode)
        {
            switch (patternCode)
            {
                case "60":
                    return LightMode.Custom;
                case "41":
                case "61":
                case "62":
                    if (whiteCode == "0")
                        return LightMode.Color;
                    else return LightMode.WarmWhite;
                case "2a":
                case "2b":
                case "2c":
                case "2d":
                case "2e":
                case "2f":
                    return LightMode.Preset;
            }

            if(int.TryParse(patternCode, out int result))
            {
                if(result >= 25 && result <= 38)
                {
                    return LightMode.Preset;
                }
            }

            return LightMode.Unknown;
        }

        /// <summary> Converts a string containing hexadecimals to a byte array. </summary>
        internal static byte[] ToByteArray(string hexString)
        {
            byte[] bytes = new byte[hexString.Length / 2];
            int indexer;
            if (hexString[0] == '#')
                indexer = 1;
            else
                indexer = 0;

            for (int i = indexer; i < hexString.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            return bytes;
        }
    }
}
