using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MagicHome
{
    public class LightGroup
    {
        public List<Light> Group { get; set; }

        public LightGroup(params Light[] lights)
        {
            Group = new List<Light>();
            Group.AddRange(lights);
        }

        public LightGroup(List<Light> lights)
        {
            Group = new List<Light>();
            Group.AddRange(lights);
        }

        public void AutoFill()
        {
            Group = Light.Discover();
        }

        public void AddLights(params Light[] lights) //Unecessary but I'll keep it here
        {
            Group.AddRange(lights);
        }

        public void TurnOn()
        {
            foreach (Light light in Group)
            {
                light.TurnOn();
            }
        }

        public void TurnOff()
        {
            foreach(Light light in Group)
            {
                light.TurnOff();
            }
        }

        public void SetColor(byte r, byte g, byte b)
        {
            foreach(Light light in Group)
            {
                light.SetColor(r, g, b);
            }
        }

        public void SetColor(RGB rgb)
        {
            foreach(Light light in Group)
            {
                light.SetColor(rgb);
            }
        }

        public void SetWarmWhite(byte white)
        {
            foreach(Light light in Group)
            {
                light.SetWarmWhite(white);
            }
        }

        public void SetWhite(byte white)
        {
            foreach(Light light in Group)
            {
                light.SetWhite(white);
            }
        }

        public void SetBrightness(byte brightness)
        {
            foreach(Light light in Group)
            {
                light.SetBrightness(brightness);
            }
        }

        public void SetCustomPattern(RGB[] list, TransitionType transition, byte speed)
        {
            foreach(Light light in Group)
            {
                light.SetCustomPattern(list, transition, speed);
            }
        }

        public void SetPresetPatterm(PresetPattern pattern, byte speed)
        {
            foreach(Light light in Group)
            {
                light.SetPresetPattern(pattern, speed);
            }
        }
    }
}
