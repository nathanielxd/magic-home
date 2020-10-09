# Documentation

## Discovery
Use this static method to discover all available magic home lights on your network:
```c#
var lights = await Light.DiscoverAsync();
```
This will take approximately 1 second.

If you have many leds you might need more time to discover all of them, therefore you'll have to use the LightDiscovery class and set a custom timeout.
```c#
LightDiscovery.Timeout = 2000;
var lights = await LightDiscovery.Discover();
```

`Light.DiscoverAsync()` and `LightDiscovery.DiscoverAsync()` are doing the same thing.

## Light
The light entity contains almost all logic of the library.

### Constructor
1. Initialize your light with its local ip address:
```c#
var light = new Light("192.168.1.1");
```

2. Connect to it:
```c#
await light.ConnectAsync();
```
3. You're now ready to use it.

### Methods
These are the methods used to interact with the light.

#### Set power
Use this to turn it on or off:
```c#
await light.TurnOnAsync(); //or light.SetPowerAsync(true)
await light.TurnOffAsync(); //or light.SetPowerAsync(false)
```

#### Set color
```c#
await light.SetColorAsync(0, 127, 243);
```
Or use the Color class:
```c#
await light.SetColorAsync(Colors.Purple);
```

#### Set white
Set cold white:
```c#
await light.SetColdWhiteAsync(255); //or light.SetColorAsync(255, 255, 255)
```

If you have a bulb or strip that supports warm white, use:
```c#
await light.SetWarmWhiteAsync(255);
```

#### Set preset patterns
The light has some factory patterns:
```c#
await light.SetPresetPatternAsync(PresetPattern.PurpleGradualChange, 50);
```

#### Set custom patterns
Make your own patterns by using this:
```c#
var colors = new List<Color>()
{
  new Color(255, 0, 0), //Red
  new Color(0, 255, 0), //Green
  Colors.Green //Or just use Colors class
};

await light.SetCustomPatternAsync(colors, TransitionType.Gradual, 50);
```

#### Refresh
The light's properties are populated whenever you use any method, but sometimes the light might fail so it's recommended to use this from time to time to send a request to the light and get it's status (Light mode, Color, Brightness, etc.):
```c#
await light.RefreshAsync();
```

#### Print
Print your light's status:
```c#
Console.WriteLine(light.ToString());
```

## Color
Color class for library.

Make a new color:
```c#
var color = new Color(25, 12, 164);
```

Or use a pre-set:
```c#
var color = Colors.Purple;
```

Empty / Transparent / Black:
```c#
var color = Colors.Empty; // new Color();
```
