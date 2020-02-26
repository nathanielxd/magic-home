using System;
using System.Collections.Generic;
using System.Text;

namespace MagicHome
{
    public class RGB
    {
        public byte red { get; set; }
        public byte green { get; set; }
        public byte blue { get; set; }

        public static RGB Red = new RGB(255, 0, 0);
        public static RGB Blue = new RGB(0, 0, 255);
        public static RGB Green = new RGB(0, 255, 0);
        public static RGB Purple = new RGB(255, 0, 255);
        public static RGB Cyan = new RGB(0, 255, 255);
        public static RGB Yellow = new RGB(255, 255, 0);

        public RGB()
        {

        }

        public RGB(byte red, byte green, byte blue)
        {
            this.red = red; this.green = green; this.blue = blue;
        }

        public RGB(string hexColor)
        {
            byte[] bytes = Utilis.ToByteArray(hexColor);
            red = bytes[0]; green = bytes[1]; blue = bytes[2];
        }

        public void Set(byte red, byte green, byte blue)
        {
            this.red = red; this.green = green; this.blue = blue;
        }

        public void Set(string hexColor)
        {
            byte[] bytes = Utilis.ToByteArray(hexColor);
            red = bytes[0]; green = bytes[1]; blue = bytes[2];
        }
    }
}
