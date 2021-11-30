using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled tileset
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TileSet {GetDebuggerString(),nq}")]
    public partial class TiledTileset : TiledProperties
    {
        /// <summary>
        /// The Tiled version used to create this tileset
        /// </summary>
        public string TiledVersion { get; set; }
        /// <summary>
        /// The first gid defines which gid matches the tile with source vector 0,0. Is used to determine which tileset belongs to which gid
        /// </summary>
        public int FirstGlobalId { get; set; }        
        /// <summary>
        /// The tileset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The (maximum) tile width in pixels
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// The (maximum) tile height in pixels
        /// </summary>
        public int TileHeight { get; set; }        
        /// <summary>
        /// The number of tiles in this tileset (since 0.13)
        /// </summary>
        public int TileCount { get; set; }
        /// <summary>
        /// The number of tile columns in the tileset.<br/>
        /// For image collection tilesets it is editable and is used when displaying the tileset.<br/>
        /// (since 0.15)<br/>
        /// </summary>
        public int Columns { get; set; }
        /// <summary>
        /// The image definition used by the tileset
        /// </summary>
        public TiledImage Image { get; set; }
        /// <summary>
        /// The spacing in pixels between the tiles in this tileset (applies to the tileset image, defaults to 0)
        /// </summary>
        public int Spacing { get; set; }
        /// <summary>
        /// The margin around the tiles in this tileset (applies to the tileset image, defaults to 0)
        /// </summary>
        public int Margin { get; set; }
        /// <summary>
        /// This element is used to specify an offset in pixels, to be applied when drawing a tile from the related tileset.<br/>
        /// When not present, no offset is applied.
        /// </summary>
        public TiledTileOffset TileOffset { get; set; }
        /// <summary>
        /// An array of tile definitions
        /// </summary>
        /// <remarks>Not all tiles within a tileset have definitions. Only those with properties, animations, terrains, ...</remarks>
        public TiledTile[] Tiles { get; set; }
        /// <summary>
        /// An array of terrain definitions
        /// </summary>
        public TiledTerrain[] Terrains { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            // if (node.SelectNodes("wangsets").Count > 0) throw new TiledException("wangtiles not supported");

            TiledVersion = node.Attributes.GetStringOrDefault("tiledversion", string.Empty);
            FirstGlobalId = node.Attributes.GetSignedOrDefault("firstgid", 0);
            Name = node.Attributes.GetStringOrDefault("name", null);
            TileWidth = int.Parse(node.Attributes["tilewidth"].Value);
            TileHeight = int.Parse(node.Attributes["tileheight"].Value);

            Margin = node.Attributes.GetSignedOrDefault("margin", 0);
            Spacing = node.Attributes.GetSignedOrDefault("spacing", 0);
            Image = TryCreateFrom<TiledImage>(node.SelectSingleNode("image"));
            Tiles = TryCreateFrom<TiledTile>(node.SelectNodes("tile"));
            Terrains = TryCreateFrom<TiledTerrain>(node.SelectNodes("terraintypes/terrain"));

            TileCount = node.Attributes.GetSignedOrDefault("tilecount", Tiles.Length);
            Columns = node.Attributes.GetSignedOrDefault("columns", 0);

            TileOffset = TryCreateFrom<TiledTileOffset>(node.SelectSingleNode("tileoffset"));
        }

        /// <summary>
        /// Gets debugger information from this object.
        /// </summary>
        /// <returns>A debug information string.</returns>
        protected virtual string GetDebuggerString()
        {
            var txt = string.Empty;
            if (FirstGlobalId != 0) txt += $" Id={FirstGlobalId}";
            if (!string.IsNullOrWhiteSpace(Name)) txt += $" Name=\"{Name}\"";

            txt += $" ({TileWidth}x{TileHeight}) x {TileCount}";

            return txt.TrimStart();
        }
    }

    /// <summary>
    /// Represents a tile drawing offset.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{x},{y}")]
    public partial class TiledTileOffset : TiledSerializable
    {
        /// <summary>
        /// Horizontal offset in pixels. (defaults to 0)
        /// </summary>
        public int x;

        /// <summary>
        /// Vertical offset in pixels (positive is down, defaults to 0)
        /// </summary>
        public int y;

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            x = node.Attributes.GetSignedOrDefault("x", 0);
            y = node.Attributes.GetSignedOrDefault("y", 0);
        }
    }

    /// <summary>
    /// Represents a tile within a tileset
    /// </summary>
    /// <remarks>These are not defined for all tiles within a tileset, only the ones with properties, terrains and animations.</remarks>
    [System.Diagnostics.DebuggerDisplay("Tile {Id} {Type,nq}")]
    public partial class TiledTile : TiledProperties
    {
        /// <summary>
        /// The local tile ID within its tileset.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// The type of the tile.
        /// </summary>
        /// <remarks>
        /// Refers to an object type and is used by tile objects. (optional) (since 1.0)
        /// </remarks>
        public string Type { get; set; }
        /// <summary>
        ///  A percentage indicating the probability that this tile is chosen when it competes with others while editing with the terrain tool. (defaults to 0)
        /// </summary>
        public float Probability { get; set; }
        /// <summary>
        /// The terrain definitions as int array. These are indices indicating what part of a terrain and which terrain this tile represents.
        /// </summary>
        /// <remarks>
        /// In the map file empty space is used to indicate null or no value. However, since it is an int array I needed something so I decided to replace empty values with -1.
        /// </remarks>
        public int[] Terrain { get; set; }
        /// <summary>
        /// An array of tile animations. Is null if none were defined. 
        /// </summary>
        public TiledTileAnimationFrame[] Animation { get; set; }
        /// <summary>
        /// The individual tile image
        /// </summary>
        public TiledImage Image { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            Id = int.Parse(node.Attributes["id"].Value);
            Type = node.Attributes.GetStringOrDefault("type", null);
            Probability = node.Attributes.GetSingleOrDefault("probability", 0);
            Terrain = node.Attributes.GetStringOrDefault("terrain", string.Empty).Split(',').AsIntArray();
            Animation = TryCreateFrom<TiledTileAnimationFrame>(node.SelectNodes("animation/frame"));
            Image = TryCreateFrom<TiledImage>(node.SelectSingleNode("image"));
        }
    }

    /// <summary>
    /// Represents a tile animation. Tile animations are a group of tiles which act as frames for an animation.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Frame {TileId} {Duration}")]
    public partial class TiledTileAnimationFrame : TiledSerializable
    {
        /// <summary>
        /// The tile id within a tileset
        /// </summary>
        public int TileId { get; set; }
        /// <summary>
        /// The duration in miliseconds
        /// </summary>
        public int Duration { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            TileId = int.Parse(node.Attributes["tileid"].Value);
            Duration = int.Parse(node.Attributes["duration"].Value);
        }
    }

    /// <summary>
    /// Represents a terrain definition.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Terrain {Name} {TileId}")]
    public partial class TiledTerrain : TiledSerializable
    {
        /// <summary>
        /// The terrain name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The tile used as icon for the terrain editor
        /// </summary>
        public int TileId { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            Name = node.Attributes["name"].Value;
            TileId = int.Parse(node.Attributes["tile"].Value);
        }
    }
}