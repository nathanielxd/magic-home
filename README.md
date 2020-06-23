# Magic Home [![NuGet](https://img.shields.io/badge/NuGet-1.2.0-brightgreen.svg)](https://www.nuget.org/packages/MagicHomeAPI/1.2.0)
.NET Library that allows you control Magic Home enabled lights connected to the same LAN.
With this, you can control your bulbs that work with the [Magic Home App](https://play.google.com/store/apps/details?id=com.Zengge.LEDWifiMagicHome).

## Requirements
- A bulb or led strip that works with the Magic Home app;
- Your device connected to the same network as the light.

## Quick Example
```c#
var lights = await Light.Discover();

var light = lights[0];
await light.ConnectAsync();

if(light.Power == false)
  await light.TurnOnAsync();

await light.SetColorAsync(0, 255, 0);
```

### Features
- Discover bulbs on LAN;
- Turn On/Off;
- Use Color, White and Warm white;
- Turn Patterns;
- Use time.

### Missing Features:

- Music / Microphone;
- Using in-built-timers;
- administration to set WiFi SSiD key.

## Installation
[NuGet](https://www.nuget.org/packages/MagicHomeAPI/1.4.0)

### .NET CLI
`dotnet add package MagicHomeAPI --version 1.4.0`

## Documentation
Soon.
