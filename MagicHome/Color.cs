using System;
using System.Collections.Generic;
using System.Text;

namespace MagicHome
{
    public class Color
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        /// <summary> Creates a new color object with red, green and blue set to 0. </summary>
        public Color()
        {
            Red = 0;
            Blue = 0;
            Green = 0;
        }

        /// <summary> Creates a new color object with red, green and blue values, from 0 to 255 (inclusive). </summary>
        public Color(byte red, byte green, byte blue)
        {
            this.Red = red;
            this.Green = green;
            this.Blue = blue;
        }

        /// <summary> Creates a new color object from hexadecimal values. (ex. #0000ff) </summary>
        public Color(string hexColor)
        {
            byte[] bytes = Utilis.ToByteArray(hexColor);
            Red = bytes[0];
            Green = bytes[1];
            Blue = bytes[2];
        }

        public override string ToString()
        {
            return "R" + Red + " G" + Green + " B" + Blue;
        }
    }

    public class Colors
    {
        public static Color Empty = new Color(0, 0, 0);
        public static Color Red = new Color(255, 0, 0);
        public static Color Blue = new Color(0, 0, 255);
        public static Color Green = new Color(0, 255, 0);
        public static Color Purple = new Color(255, 0, 255);
        public static Color Cyan = new Color(0, 255, 255);
        public static Color Yellow = new Color(255, 255, 0);
    }
}
