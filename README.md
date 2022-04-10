# TiledCS
TiledCS is a .NET library for loading Tiled maps and tilesets. It supports only the _TMX_ and _TSX_ file formats. The library requires no 3rd-party dependencies. This way the library can be used with popular game engines like Unity3D, MonoGame and Godot.

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
    public class Program
    {
        private static void Main(string[] args)
        {
            // For loading maps in XML format
            var map = new TiledMap("path-to-map.tmx");
            var tileset = new TiledTileset("path-to-tileset.tsx");

            // Retrieving objects or layers can be done using Linq or a for loop
            var myLayer = map.layers.First(l => l.name == "monsters");
            var myObj = myLayer.objects.First(o => o.name == "monster");

            // Since they are classes and not structs, you can do null checks to figure out if an object exists or not
            if (myObj != null)
            {
                var xx = myObj.x * 16;
                var yy = myObj.y * 16;
            }

            // You can use the helper methods to get useful information to generate maps
            if (map.IsTileFlippedHorizontal(myLayer, 3, 5))
            {
                // Do something
            }

            var mapTileset = map.Tilesets.First();

            // Used for retrieving the tile horizontal and vertical position of the source rect of the tileset image which you can use to draw tiles with. This way you won't have to manually figure that one out yourself.
            var srcVector = map.GetSourceVector(mapTileset, tileset, 478);
        }
    }
}
```

## Building
You need **.NET 6** to compile TiledCS as it makes use of modern C# features that may or may not be included in earlier versions.

## Version support
If you want to know what package supports your version of Tiled, please read the package's description.

## Contribution
Feel free to open up an issue with your question or request. If you do plan to make modifications to the code please open up an issue first with more details about what you'd like to change and why. If you open a pull request, please use the **develop** branch as both **source** and **target** branch. I follow the [GitFlow branching model](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow).

## Credits
* A very respectful thank you to Goodlyay who introduced support for tile rotations aka flipped tiles and taught me about bitwise operators in C#.

## License
[MIT](LICENSE)
