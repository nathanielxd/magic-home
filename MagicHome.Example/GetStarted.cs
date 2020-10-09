using System;
using MagicHome;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MagicHome.Example
{
    class GetStarted
    {
        static async Task Main()
        {
            var discoveredLights = await Light.DiscoverAsync();

            if (discoveredLights?.Count > 0)
            {
                var light = discoveredLights[0];

                //Connect.
                await light.ConnectAsync();

                //Check if it is ON.
                if (light.Power == false)
                    await light.TurnOnAsync();

                //Change color to green.
                await light.SetColorAsync(0, 255, 0);

                //Print to console light's status.
                Console.WriteLine(light.ToString());
            }            
        }
    }
}
