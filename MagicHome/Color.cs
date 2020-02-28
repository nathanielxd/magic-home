using System;
using System.Collections.Generic;
using System.Text;

namespace MagicHome
{
    public class Color
    {
        public byte red { get; set; }
        public byte green { get; set; }
        public byte blue { get; set; }

        public static Color Red = new Color(255, 0, 0);
        public static Color Blue = new Color(0, 0, 255);
        public static Color Green = new Color(0, 255, 0);
        public static Color Purple = new Color(255, 0, 255);
        public static Color Cyan = new Color(0, 255, 255);
        public static Color Yellow = new Color(255, 255, 0);

        /// <summary> Creates a new color object with red, green and blue set to 0. </summary>
        public Color()
        {
            red = 0;
            blue = 0;
            green = 0;
        }

        /// <summary> Creates a new color object with red, green and blue values, from 0 to 255 (inclusive). </summary>
        public Color(byte red, byte green, byte blue)
        {
            this.red = red;
            this.green = green;
            this.blue = blue;
        }

        /// <summary> Creates a new color object from hexadecimal values. (ex. #0000ff) </summary>
        public Color(string hexColor)
        {
            byte[] bytes = Utilis.ToByteArray(hexColor);
            red = bytes[0];
            green = bytes[1];
            blue = bytes[2];
        }
    }
}
