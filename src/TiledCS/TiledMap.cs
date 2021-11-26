using System;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled map
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TiledMap {Infinite} {Width}x{Height} {TileWidth}x{TileHeight}")]
    public partial class TiledMap : TiledProperties
    {
        /// <summary>
        /// Returns the Tiled version used to create this map
        /// </summary>
        public string TiledVersion { get; set; }        
        /// <summary>
        /// Returns an array of tileset definitions in the map
        /// </summary>
        public TiledMapTileset[] Tilesets { get; set; }
        /// <summary>
        /// Returns an array of layers or null if none were defined
        /// </summary>
        public TiledLayer[] Layers { get; set; }
        /// <summary>
        /// Returns an array of groups or null if none were defined
        /// </summary>
        public TiledGroup[] Groups { get; set; }
        /// <summary>
        /// Returns the defined map orientation as a string
        /// </summary>
        public string Orientation { get; set; }
        /// <summary>
        /// Returns the render order as a string
        /// </summary>
        public string RenderOrder { get; set; }
        /// <summary>
        /// The amount of horizontal tiles
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The amount of vertical tiles
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The tile width in pixels
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// The tile height in pixels
        /// </summary>
        public int TileHeight { get; set; }
        /// <summary>
        /// Returns true if the map is configured as infinite
        /// </summary>
        public bool Infinite { get; set; }
        /// <summary>
        /// Returns the defined map background color as a hex string
        /// </summary>
        public string BackgroundColor { get; set; }
        /// <summary>
        /// Returns an empty instance of TiledMap
        /// </summary>
        public TiledMap() {}        
    }
}