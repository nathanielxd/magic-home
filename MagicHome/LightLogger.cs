using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace MagicHome
{
    public class LightLogger
    {
        public bool Enabled { get; set; }
        public string LogPath { get; set; }
        public DateTime StartTime { get; private set; }
        public Light MyLight { get; private set; }

        public LightLogger(Light light, string path = null)
        {
            if (path != null)
                LogPath = path;
            else
                LogPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Logs";

            Enabled = true;
            MyLight = light;
            StartTime = DateTime.Now;
        }

        internal void Log(string message)
        {
            if (Enabled)
            {
                using (StreamWriter writer = File.AppendText(LogPath + "\\" + "lightlog" + StartTime.Day + StartTime.Month + StartTime.Year + ".log"))
                {
                    writer.Write("\n[" + DateTime.Now.ToLongTimeString() + "] (" + MyLight.Ep.Address + ") " + message);
                }
            }
        }
    }
}
