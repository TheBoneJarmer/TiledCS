> **Heads up!** I am looking for a second maintainer or someone to take over the project entirely as you may have noticed I am not that active anymore. The reason for this being the lack of time to work on this project.

# TiledCS
TiledCS is a dotnet library for loading Tiled maps and tilesets. It supports both XML as JSON map formats. The library requires no 3rd-party dependencies except for Newtonsoft.Json or another JSON parsing library. This way the library can be used with popular game engines like Unity3D, MonoGame and Godot.

## Installation
```
dotnet add package tiledcs
```

## Usage
> **JSON support may not be fully functioning for now due to the differences between how nodes differ between the TMX format and the JSON format. This will be fixed in a later version**

TiledCS has been created with as mindset being as comfortable and easy to use as possible. The structure of the classes and properties are similar to the JSON map format and can therefore be deserialized like that.

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
            
            // For loading maps in JSON format
            var json1 = File.ReadAllText("path-to-map.json");
            var json2 = File.ReadAllText("path-to-tileset.json");
            var mapJson = JsonConvert.DeserializeObject<TiledMap>(json1);
            var tilesetJson = JsonConvert.DeserializeObject<TiledTileset>(json2);
            
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

### MonoGame
> **Note! The MonoGame content pipeline only supports loading maps in TMX and TSX format**

If you are using MonoGame and you would like to import Tiled maps and tilesets I recommend taking a look at [DelightedCat's extension project](https://github.com/DelightedCat/TiledCS.Extensions.MonoGame). He created a custom pipeline which allows the import of Tiled maps and tilesets.

## Building
You need the latest stable dotnet sdk to compile TiledCS as it makes use of modern C# features that may or may not be included in earlier versions.

## Version support
If you want to know what package supports your version of Tiled, please read the package's description. The most recent package always supports the current Tiled version.

## Contribution
Feel free to open up an issue with your question or request. If you do plan to make modifications to the code please open up an issue first with more details about what you'd like to change and why. If you open a pull request, please target the **develop** branch. I follow the [GitFlow branching model](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow).

## Credits
* A huge thanks to DelightedCat who created a MonoGame pipeline. Although the pipeline isn't officially included in this library (as we both agreed to keep it standalone and not engine-based), you can ask help and questions here as DelightedCat is an official maintainer.
* A very respectful thank you to Goodlyay who introduced support for tile rotations aka flipped tiles and taught me about bitwise operators in C#.

## License
[MIT](LICENSE)
