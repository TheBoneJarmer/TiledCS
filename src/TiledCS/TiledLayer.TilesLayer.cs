using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a square region of tiles.
    /// </summary>
    /// <remarks>
    /// Implemented by:<br/>
    /// - <see cref="TiledTilesLayer"/><br/>
    /// - <see cref="TiledLayerChunk"/>
    /// </remarks>
    public interface ITilesRegion
    {
        /// <summary>
        /// The horizontal offset
        /// </summary>
        int OffsetX { get; }
        /// <summary>
        /// The vertical offset
        /// </summary>
        int OffsetY { get; }
        /// <summary>
        /// Total horizontal tiles
        /// </summary>
        int Width { get; }
        /// <summary>
        /// Total vertical tiles
        /// </summary>
        int Height { get; }
        TiledIndex[] Data { get; }
    }

    /// <summary>
    /// Represents an tiles layer within a <see cref="TiledMap"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Layer {GetDebuggerString(),nq}")]
    public class TiledTilesLayer : TiledLayer , ITilesRegion
    {
        /// <summary>
        /// The width of the layer in tiles.<br/>
        /// Always the same as the map width for fixed-size maps.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The height of the layer in tiles.<br/>
        /// Always the same as the map height for fixed-size maps.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Horizontal parallax factor for this layer. Defaults to 1. (since 1.5)
        /// </summary>
        public float ParallaxX { get; set; }
        /// <summary>
        /// Vertical parallax factor for this layer. Defaults to 1. (since 1.5)
        /// </summary>
        public float ParallaxY { get; set; }
        /// <summary>
        /// An int array of gid numbers which define which tile is being used where.<br/>
        /// The length of the array equals the layer width * the layer height.<br/>
        /// Is null when the layer is not a tilelayer.<br/>
        /// </summary>
        public TiledIndex[] Data { get; set; }
        /// <summary>
        /// chunks for infinite TiledMap.
        /// </summary>
        public TiledLayerChunk[] Chunks { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            this.Width = node.Attributes.GetSignedOrDefault("width", 0);
            this.Height = node.Attributes.GetSignedOrDefault("height", 0);

            this.ParallaxX = node.Attributes.GetSingleOrDefault("parallaxx", 1f);
            this.ParallaxY = node.Attributes.GetSingleOrDefault("parallaxy", 1f);

            var nodeData = node.SelectSingleNode("data");
            var nodeChunks = nodeData.SelectNodes("chunk");

            if (nodeChunks == null || nodeChunks.Count == 0)
            {
                this.Data = TiledIndex.TryCreateFrom(nodeData);

                if (this.Data.Length < this.Width * this.Height) throw new TiledException("data size mismatch");
            }
            else
            {
                this.Chunks = TryCreateFrom<TiledLayerChunk>(nodeChunks);
            }
        }

        /// <inheritdoc />
        protected override string GetDebuggerString()
        {
            var txt = base.GetDebuggerString();
            if (Width != 0 || Height != 0) txt += $" ({Width}x{Height})";
            if (ParallaxX != 1 || ParallaxY != 1) txt += $" Parallax=({ParallaxX},{ParallaxY})";

            return txt.TrimStart();
        }
    }

    /// <summary>
    /// Represents a chunk of tiles within a <see cref="TiledTilesLayer"/>.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Chunk ({offsetX},{offsetY}) {width}x{height}")]
    public partial class TiledLayerChunk : TiledSerializable , ITilesRegion
    {
        /// <summary>
        /// The horizontal offset
        /// </summary>
        public int OffsetX { get; set; }
        /// <summary>
        /// The vertical offset
        /// </summary>
        public int OffsetY { get; set; }
        /// <summary>
        /// Total horizontal tiles
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Total vertical tiles
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// An int array of gid numbers which define which tile is being used where.<br/>
        /// The length of the array equals the width * the height.        
        /// </summary>
        public TiledIndex[] Data { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            this.OffsetX = node.Attributes.GetSignedOrDefault("x", 0);
            this.OffsetY = node.Attributes.GetSignedOrDefault("y", 0);
            this.Width = node.Attributes.GetSignedOrDefault("width", 0);
            this.Height = node.Attributes.GetSignedOrDefault("height", 0);

            this.Data = TiledIndex.TryCreateFrom(node);

            if (this.Data.Length < this.Width * this.Height) throw new TiledException("data size mismatch");
        }
    }
}
