# TiledCS
TiledCS is a dotnet library for loading Tiled maps and tilesets. It supports both XML as JSON map formats. The library has no 3rd-party dependencies except for Newtonsoft.Json. This way the library can be used with popular game engines like Unity3D, MonoGame and Godot.

## Installation
```
dotnet add package tiledcs
```

## Usage
```csharp
using System;
using System.IO;
using Newtonsoft.Json;
using TiledCS;

namespace TestApp
{
    public class Program(string[] args)
    {
        // For loading maps in XML format
        var map = new TiledMap("path-to-map.tmx");        
        var tileset = new TiledTileset("path-to-tileset.tsx");
           
        // For loading maps in JSON format
        var json1 = File.ReadAllText("path-to-map.json");
        var json2 = File.ReadAllText("path-to-tileset.json");
        var map = JsonConvert.DeserializeObject<TiledMap>(json1);
        var tileset = JsonConvert.DeserializeObject<TiledTileset>(json2);
    }
}
```

## Contribution
Feel free to open up an issue with your question or request. If you do plan to make modifications to the code please open up an issue first with more details about what you'd like to change and why.

## License
[MIT](LICENSE)
