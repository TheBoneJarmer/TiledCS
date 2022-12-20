> **Important!** It is with great sadness that I have to anounce that I no longer can find enough time to care for the project as it is. Not only do I personally no longer work with C#, there are other projects that require my attention.

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
```

## Examples
### Rendering all tile layers
Within a layer whose `type` equals that of `tilelayer`, exists a field of type `int[]` called `data`. This array represents the gids of all tiles. A gid is a tile **index** from a tileset, _not_ the tile id as some think. In order to render a specific tile from a tileset into your spritebatch, you would need to link the gid to a tileset.

If you have multiple tilesets, you need to figure out which gid belongs to which. To help you with this process, I have added some helper methods to make this more easier for you. Let's say for example you have the following list of tilesets:

```
main.tsx
dungeon.tsx
overworld.tsx
```

And each tileset has 10 tiles horizontal and 10 tiles vertical, then the gid's per tileset would look like this:

```
main.tsx 1..100
dungeon.tsx 101..200
overworld.tsx 201..300
```

Why? Because a tileset of 10x10 has 100 tiles in total and since 0 is used to tell that there is no tile, it starts with 1. Then the next included tileset's gid starts with the total amount of tiles from the tileset included before. Therefore you should be careful when extending an existing tileset when your map has already been drawn. The helper method `TiledMap.GetTiledMapTileset` returns a dictionary where the key represent the tileset's first gid and the value represents the tileset which is of type `TiledTileset`.

> **Warning!** _This works with external tilesets only for the moment. Support for embedded is on the way!_

Once you have the tileset and the gid, you can use that data to retrieve the tile's source rect. You can than use this data to render the tile. See the example below. You may need to tweak things a bit to fit for you case but below should do the trick. Be aware the `layer.width` does **not** equal the layer's total width in pixels. The `layer.width` and `layer.height` value represents the layer's horizontal tiles and vertical tiles. So you need to know your tile's size in pixels too, which can be fetched from the `TiledMap.TileWidth` and `TiledMap.TileHeight` property. 

```cs
var map = new TiledMap("path-to-map.tmx");
var tilesets = map.GetTiledTilesets("path-to-map-folder/"); // DO NOT forget the / at the end
var tileLayers = map.Layers.Where(x => x.type == TiledLayerType.TileLayer);

foreach (var layer in tileLayers)
{
    for (var y = 0; y < layer.height; y++)
    {
        for (var x = 0; x < layer.width; x++)
        {
            var index = (y * layer.width) + x; // Assuming the default render order is used which is from right to bottom
            var gid = layer.data[index]; // The tileset tile index
            var tileX = (x * map.TileWidth);
            var tileY = (y * map.TileHeight);

            // Gid 0 is used to tell there is no tile set
            if (gid == 0)
            {
                continue;
            }

            // Helper method to fetch the right TieldMapTileset instance. 
            // This is a connection object Tiled uses for linking the correct tileset to the gid value using the firstgid property.
            var mapTileset = map.GetTiledMapTileset(gid);
            
            // Retrieve the actual tileset based on the firstgid property of the connection object we retrieved just now
            var tileset = tilesets[mapTileset.firstgid];
            
            // Use the connection object as well as the tileset to figure out the source rectangle.
            var rect = map.GetSourceRect(mapTileset, tileset, gid);
            
            // Render sprite at position tileX, tileY using the rect
        }
    }
}
```

### MonoGame
As you already know, TiledCS is a generic library which does not aim at specific frameworks or libraries. That said, I have been receiving lots of requests for an example on MonoGame. However, **I do not use MonoGame**. So it is up to the community to come up with an example on how to use TiledCS within monogame. And they did:

* https://github.com/Temeez/TiledCS-MonoGame-Example
* https://github.com/ironcutter24/TiledCS-example-MonoGame

> **Note!** Temeez has mentioned he is no longer actively working with C# anymore. So while the example may or may not work, it is not actively maintained anymore. Be aware of that.

## Building
You need **.NET 6** to compile TiledCS as it makes use of modern C# features that may or may not be included in earlier versions.

## Version support
If you want to know what package supports your version of Tiled, please read the package's description.

## Contribution
Feel free to open up an issue with your question or request. If you open a pull request, please use the **develop** branch as both **source** and **target** branch. The **main** branch is supposed to be production-ready and stable. I follow the [GitFlow branching model](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow) and I ask you do too.

## Credits
* A very respectful thank you to Goodlyay who introduced support for tile rotations aka flipped tiles and taught me about bitwise operators in C#.
* A very huge thanks towards user [Temeez](https://github.com/Temeez) and [IronCutter24](https://github.com/ironcutter24) who put effort into providing a MonoGame example.

## License
[MIT](LICENSE)
