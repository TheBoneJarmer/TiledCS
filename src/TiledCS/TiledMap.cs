using System;
using System.Linq;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled map
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TiledMap {Infinite} {Width}x{Height} {TileWidth}x{TileHeight}")]
    public partial class TiledMap : TiledProperties
    {
        /// <summary>
        /// The TMX format version. Was “1.0” so far, and will be incremented to match minor Tiled releases.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// The Tiled version used to save the file (since Tiled 1.0.1).<br/>
        /// May be a date (for snapshot builds).<br/>
        /// (optional)<br/>
        /// </summary>
        public string TiledVersion { get; set; }
        /// <summary>
        /// Returns an array of tileset definitions in the map
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public TiledTileset[] Tilesets { get; set; }
        /// <summary>
        /// Returns an array of layers or null if none were defined
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public TiledLayer[] Layers { get; set; }
        /// <summary>
        /// Map orientation.
        /// </summary>
        /// <remarks>
        ///  Tiled supports:<br/>
        ///  - orthogonal,<br/>
        ///  - isometric,<br/>
        ///  - staggered,<br/>
        ///  - hexagonal<br/>
        ///  (since 0.11).
        /// </remarks>
        public string Orientation { get; set; }
        /// <summary>
        /// The order in which tiles on tile layers are rendered.
        /// </summary>
        /// <remarks>
        /// Valid values are:<br/>
        /// - right-down (the default),<br/>
        /// - right-up,<br/>
        /// - left-down,<br/>
        /// - left-up.<br/>
        /// In all cases, the map is drawn row-by-row.<br/>
        /// (only supported for orthogonal maps at the moment)
        /// </remarks>
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
        

        /// <inheritdoc />
        protected override void ParseXml(XmlNode nodeMap)
        {
            base.ParseXml(nodeMap);

            this.Version = nodeMap.Attributes.GetStringOrDefault("version", "1.0");
            this.TiledVersion = nodeMap.Attributes.GetStringOrDefault("tiledversion", null);

            this.Orientation = nodeMap.Attributes["orientation"].Value;
            this.RenderOrder = nodeMap.Attributes.GetStringOrDefault("renderorder", "right-down");
            this.BackgroundColor = nodeMap.Attributes.GetStringOrDefault("backgroundcolor", null);
            this.Infinite = nodeMap.Attributes.GetStringOrDefault("infinite", "0") == "1";

            this.Width = int.Parse(nodeMap.Attributes["width"].Value);
            this.Height = int.Parse(nodeMap.Attributes["height"].Value);
            this.TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
            this.TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

            Tilesets = TryCreateFrom<TiledTilesetSource>(nodeMap.SelectNodes("tileset"));
            Layers = TiledLayer.TryCreateFrom(nodeMap).ToArray();
        }
    }

    
}