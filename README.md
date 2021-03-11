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

### MonoGame
> **Note! The MonoGame content pipeline only supports loading maps in TMX and TSX format**

If you are using MonoGame and you would like to import Tiled maps and tilesets I recommend taking a look at (DelightedCat's example project)[https://github.com/DelightedCat/TiledCS.MonoGame]. He created a custom pipeline which allows the import of Tiled maps and tilesets.

## Version support
If you want to know what package supports your version of Tiled, please read the package's description. The most recent package always supports the current Tiled version.

## Contribution
Feel free to open up an issue with your question or request. If you do plan to make modifications to the code please open up an issue first with more details about what you'd like to change and why.

## Credits
A huge thanks to DelightedCat who created a MonoGame pipeline. Although the pipeline isn't officially included in this library (as we both agreed to keep it standalone and not engine-based), you can ask help and questions here as DelightedCat is an official maintainer.

## License
[MIT](LICENSE)
