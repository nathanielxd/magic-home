using System;
using System.Collections.Generic;
using System.Text;

namespace MagicHome
{
    internal static class Utilis
    {
        internal static byte SpeedToDelay(byte speed)
        {
            if (speed > 100)
                speed = 100;

            int inv_speed = 100 - speed;
            byte delay = Convert.ToByte((inv_speed * (0x1f - 1)) / 100);
            delay += 1;

            return delay;
        }

        internal static byte DetermineBrightness(byte Red, byte Green, byte Blue)
        {
            int maxx = 0;
            if (Red > maxx) { maxx = Red; }
            if (Green > maxx) { maxx = Green; }
            if (Blue > maxx) { maxx = Blue; }
            maxx = maxx * 100 / 255;

            var brightness = Convert.ToByte(maxx);

            return brightness;
        }

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

        public static byte[] ToByteArray(string hexString)
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
