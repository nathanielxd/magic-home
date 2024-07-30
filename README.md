# Magic Home [![NuGet](https://img.shields.io/badge/NuGet-1.4.0-brightgreen.svg)](https://www.nuget.org/packages/MagicHomeAPI/1.4.0)
.NET Library that allows you to control Magic Home enabled lights connected to the same LAN.
With this, you can control your bulbs and led strips that work with the [Magic Home App](https://play.google.com/store/apps/details?id=com.zengge.wifi).

This library is also available in [Dart.](https://github.com/nathanielxd/magic-home-dart)

## Requirements
- A bulb or led strip that works with the Magic Home app;
- Your device connected to the same network as the light.

## Quick Example
```c#
var discoveredLights = await Light.DiscoverAsync();

if (discoveredLights?.Count > 0)
{
  var light = discoveredLights[0];

  // Connect.
  await light.ConnectAsync();

  // Check if it is ON.
  if (light.Power == false)
      await light.TurnOnAsync();

  // Change color to green.
  await light.SetColorAsync(0, 255, 0);

  // Print to console light's status.
  Console.WriteLine(light.ToString());
}
```

## [Documentation](https://github.com/nathanielxd/magic-home/blob/master/DOCS.md)
Available in the DOCS.md file.

## Installation
[NuGet](https://www.nuget.org/packages/MagicHomeAPI/1.4.0)

.NET CLI `dotnet add package MagicHomeAPI --version 1.4.0`

### Features
- Discover lights on LAN;
- Turn on/off;
- Use color and warm white;
- Turn preset and custom patterns;
- Use time.

### Missing features:
- Music and microphone;
- Use built-timers;
- Other fancy stuff;
- Administration to set WiFi SSiD key.

### Contribute and support
If you need any help or request, open an issue or leave an email. I will answer immediately. If you randomly get errors, it might be because of the light so I can't help it.

I'm also open to collaboration.
