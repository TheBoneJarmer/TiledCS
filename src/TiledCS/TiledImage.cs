using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents an image
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Image {Source} {Width}x{Height}")]
    public class TiledImage : TiledSerializable
    {
        /// <summary>
        /// The image width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The image height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The image source path
        /// </summary>
        public string Source { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            Source = node.Attributes["source"].Value;
            Width = node.Attributes.GetSignedOrDefault("width", 0);
            Height = node.Attributes.GetSignedOrDefault("height", 0);
        }
    }
}
