using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents the base class of a layer within a <see cref="TiledMap"/>.
    /// </summary>
    /// <remarks>
    /// Derived types:<br/>
    /// <see cref="TiledGroupLayer"/><br/>
    /// <see cref="TiledObjectsLayer"/><br/>
    /// <see cref="TiledImageLayer"/><br/>
    /// <see cref="TiledTilesLayer"/><br/>
    /// </remarks>    
    public abstract class TiledLayer : TiledIdName
    {        
        /// <summary>
        /// Horizontal offset for this layer in pixels. Defaults to 0. (since 0.14)
        /// </summary>
        public int OffsetX { get; set; }
        /// <summary>
        /// Vertical offset for this layer in pixels. Defaults to 0. (since 0.14)
        /// </summary>
        public int OffsetY { get; set; }
        /// <summary>
        /// The opacity of the layer as a value from 0 to 1. (defaults to 1).
        /// </summary>
        public float Opacity { get; set; }
        /// <summary>
        /// A tint color that is multiplied with any tiles drawn by this layer in #AARRGGBB or #RRGGBB format (optional).
        /// </summary>
        public string TintColor { get; set; }
        /// <summary>
        /// Defines if the layer is visible in the editor
        /// </summary>
        public bool Visible { get; set; }
        /// <summary>
        /// Is true when the layer is locked
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Initializes an array of type <see cref="TiledLayer"/> from XML.
        /// </summary>        
        /// <param name="rootNode">The xml source.</param>
        /// <returns>The initialized array.</returns>
        internal static IEnumerable<TiledLayer> TryCreateFrom(XmlNode rootNode)
        {
            var nodesGroup = rootNode.SelectNodes("group");
            var nodesLayer = rootNode.SelectNodes("layer");
            var nodesObjects = rootNode.SelectNodes("objectgroup");
            var nodesImage = rootNode.SelectNodes("imagelayer");

            foreach (XmlNode node in nodesGroup)
            {
                yield return TryCreateFrom<TiledGroupLayer>(node);
            }

            foreach (XmlNode node in nodesLayer)
            {
                yield return TryCreateFrom<TiledTilesLayer>(node);
            }

            foreach (XmlNode node in nodesObjects)
            {
                yield return TryCreateFrom<TiledObjectsLayer>(node);
            }

            foreach (XmlNode node in nodesImage)
            {
                yield return TryCreateFrom<TiledImageLayer>(node);
            }
        }        

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            // deprecated since 0.15 in favour of offsetx, offsety
            var x = node.Attributes.GetSignedOrDefault("x", 0);
            var y = node.Attributes.GetSignedOrDefault("y", 0);

            this.OffsetX = node.Attributes.GetSignedOrDefault("offsetx", x);
            this.OffsetY = node.Attributes.GetSignedOrDefault("offsety", y);            
            this.Opacity = node.Attributes.GetSingleOrDefault("opacity", 1f);
            this.TintColor = node.Attributes.GetStringOrDefault("tintcolor", null);
            this.Visible = node.Attributes.GetSignedOrDefault("visible", 1) == 1;
            this.Locked = node.Attributes.GetSignedOrDefault("locked", 0) == 1;
        }

        /// <inheritdoc />
        protected override string GetDebuggerString()
        {
            var txt = base.GetDebuggerString();
            if (Visible && Opacity != 1) txt += $" Opacity={Opacity}";
            if (!Visible) txt += $" hidden";
            if (Locked) txt += $" locked";
            if (OffsetX != 0 || OffsetX != 0) txt += $" ({OffsetX},{OffsetX})";

            return txt.TrimStart();
        }
    }
}
