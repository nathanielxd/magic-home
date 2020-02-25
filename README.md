# Magic Home [![NuGet](https://img.shields.io/badge/NuGet-1.2.0-brightgreen.svg)](https://www.nuget.org/packages/MagicHomeAPI/1.2.0) [![DDL](https://img.shields.io/badge/DDL-1.2.0-orange.svg)](https://drive.google.com/file/d/1N8gVF5mD3ZfD7VJSwgafOgqnd-NJ1ncU/view?usp=sharing) 
.NET Library that allows you control Magic Home enabled lights connected to the same LAN.
With this, you can control your bulbs that work with the [Magic Home App](https://play.google.com/store/apps/details?id=com.Zengge.LEDWifiMagicHome).

Remember to to be connected to the same network as the bulb and be able to send packets from the device.

## Features
- Discover bulbs on LAN
- Turn On/Off
- Turn Color and White
- Turn Patterns
- Use time
- Groups for easy access to multiple bulbs
- Logger

Missing Features:

- Music / Microphone
- Using in-built-timers
- administration to set WiFi SSiD key

## Installation
[NuGet](https://www.nuget.org/packages/MagicHomeAPI/1.2.0) / [DDL](https://drive.google.com/file/d/1N8gVF5mD3ZfD7VJSwgafOgqnd-NJ1ncU/view?usp=sharing)

## Documentation
### Initialization
``` Light myLight = new Light("LANAdress"); ``` 

This will create the light, connect it and extract all the data from it (color, protocol, etc).

### Discovery
``` Light myLight = Light.Discover()[0]; ``` 

This will discover all lights on the LAN, and assign the first one to myLight.

### Usage
``` myLight.TurnOn();
myLight.TurnOff();
```
Sets the light on and off.

```myLight.SetWhite(255);
myLight.SetColor(255, 0, 0); // color in bytes as R G B
myLight.SetColor("#ff0000"); // color in hex
myLight.SetColor(new RGB(255, 0, 0)); // color as RGB class
```
Different ways to change the color

```
myLight.SetPresetPattern(PresetPattern.PatternHere, 100)
```
Sets a preset pattern from the enum.
Speed (second param) goes from 1 to 100.
```
myLight.SetCustomPattern(RGBArray, TransitionType.Gradual, 100)
```
Sets a custom pattern from a RGB Array, a transition and speed.

```
myLight.SetBrightness(50);
```
Sets the brightness (UNSTABLE!)

### Using RGB class
```
RGB myColor = new RGB(color bytes or hex);
RGB[] myColors = new RGB[3]; // create an array of colors so we can use SetCustomPattern();
myColors[0].Set(255, 0, 0);
myColors[1].Set(0, 255, 0);
myColors[2].Set(0, 0, 255);

SetCustomPattern(myColors, TransitionType.Strobe, 100);
```

### Groups
``` LightGroup myGroup = new LightGroup(listOfLightsHere);
LightGroup myGroup = new LightGroup(Light.Discovery());
```
Create a group of lights and assign all bulbs discovered on your LAN. You can control all your lights simultaneously by simply calling myGroup.MethodHere();

### Set Time and Get Time
``` 
SetTime(DateTime dt); // sets the time of the device to dt
GetTime(); // gets the time of the device
```

### Properties
Protocol, Color, Brightness, Time, IsOn, Mode, Logger, Socket. Use property Stats as a string containing all data. Use property DiscoveredAdresses to get all discovered IPs if you used Light.Discover();

### LightLogger
All logs are automatically saved in the assembly folder. To change that, or disable the logger:
```
myLight.Logger.LogPath = "X:\\pathhere\\";
//OR
myLight.Logger = new LightLogger(myLight, "path");

myLight.Logger.Enabled = false; //or true to enable/disable
```
