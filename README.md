# TiledCS
TiledCS is a dotnet library for loading Tiled maps and tilesets. It supports both TMX as JSON map formats.

## Installation
```
dotnet add package tiledcs
```

## Usage
```csharp
using System;
using TiledCS;

namespace TestApp
{
    public class Program(string[] args)
    {
        var map = new TiledMap();
        map.Load("path-to-map.tmx");
        
        var tileset = new TiledTileset();
        tileset.Load("path-to-tileset.tsx");
    }
}
```

## Contribution
Feel free to open up an issue with your question or request. If you do plan to make modifications to the code please open up an issue first with more details about what you'd like to change and why.

## License
[MIT](LICENSE)
