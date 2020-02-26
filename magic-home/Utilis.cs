using System;
using System.Collections.Generic;
using System.Text;

namespace MagicHome
{
    /// <summary> Different useful methods used in the package. </summary>
    internal static class Utilis
    {
        /// <summary> Transforms speed (0 to 100) to light specific 'delay' property (0 to 27). </summary>
        internal static byte SpeedToDelay(byte speed)
        {
            if (speed > 100)
                speed = 100;

            int inv_speed = 100 - speed;
            byte delay = Convert.ToByte((inv_speed * (0x1f - 1)) / 100);
            delay += 1;

            return delay;
        }

        /// <summary> Determines brightness by red, green and blue values. </summary>
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
        internal static LightMode DetermineMode(string patternCode)
        {
            LightMode mode = LightMode.Unknown;
            if (patternCode == "61" || patternCode == "62" || patternCode == "41")
                mode = LightMode.Color;

            if (patternCode == "60")
                mode = LightMode.Custom;

            for (int i = 25; i <= 38; i++)
            {
                if (patternCode == i.ToString())
                {
                    Console.WriteLine(i.ToString());
                    mode = LightMode.Preset;
                    break;
                }
            }
            if (patternCode == "2a" || patternCode == "2b" || patternCode == "2c" || patternCode == "2d" || patternCode == "2e" || patternCode == "2f")
                mode = LightMode.Preset;

            return mode;
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
