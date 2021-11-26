using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled tileset
    /// </summary>
    public partial class TiledTileset : TiledProperties
    {
        /// <summary>
        /// The Tiled version used to create this tileset
        /// </summary>
        public string TiledVersion { get; set; }
        /// <summary>
        /// The tileset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The tile width in pixels
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// The tile height in pixels
        /// </summary>
        public int TileHeight { get; set; }
        /// <summary>
        /// The total amount of tiles
        /// </summary>
        public int TileCount { get; set; }
        /// <summary>
        /// The amount of horizontal tiles
        /// </summary>
        public int Columns { get; set; }
        /// <summary>
        /// The image definition used by the tileset
        /// </summary>
        public TiledImage Image { get; set; }
        /// <summary>
        /// The amount of spacing between the tiles in pixels
        /// </summary>
        public int Spacing { get; set; }
        /// <summary>
        /// The amount of margin between the tiles in pixels
        /// </summary>
        public int Margin { get; set; }
        /// <summary>
        /// An array of tile definitions
        /// </summary>
        /// <remarks>Not all tiles within a tileset have definitions. Only those with properties, animations, terrains, ...</remarks>
        public TiledTile[] Tiles { get; set; }
        /// <summary>
        /// An array of terrain definitions
        /// </summary>
        public TiledTerrain[] Terrains { get; set; }
        /// <summary>
        /// Returns an empty instance of TiledTileset
        /// </summary>
        public TiledTileset() {}        
    }
}