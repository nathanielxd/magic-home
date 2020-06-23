using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MagicHome;

namespace ConsoleAppForLights
{
    class Program
    {
        static async Task Main()
        {
            var lightBulb = new Light("192.168.1.7");
            await lightBulb.ConnectAsync();
            await lightBulb.TurnOnAsync();
            await lightBulb.SetWarmWhiteAsync(25);

            var lightBanda = new Light("192.168.1.2");
            await lightBanda.ConnectAsync();

            Console.WriteLine("BEC");
            PrintLight(lightBulb);

            Console.WriteLine("BANDA");
            PrintLight(lightBanda);
        }

        static void PrintLight(Light light)
        {
            Console.WriteLine("Power: {0}, Color: {1} {2} {3}, White: {7}, Brightness: {4}, Mode: {5}, Protocol: {6}",
                light.PowerOn, light.Color.Red, light.Color.Green, light.Color.Blue, light.Brightness, light.Mode, light.Protocol, light.WarmWhite);
        }
    }
}
