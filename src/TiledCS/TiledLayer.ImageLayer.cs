using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents an image layer within a <see cref="TiledMap"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Image {GetDebuggerString(),nq}")]
    public class TiledImageLayer : TiledLayer
    {
        /// <summary>
        /// The image of this layer.
        /// </summary>
        public TiledImage Image { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            this.Image = TryCreateFrom<TiledImage>(node.SelectSingleNode("image"));
        }
    }
}
