using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents an objects layer within a <see cref="TiledMap"/>
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Objects {GetDebuggerString(),nq}")]
    public class TiledObjectsLayer : TiledLayer
    {
        /// <summary>
        /// The color used to display the objects in this group. (defaults to gray (“#a0a0a4”))
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// Whether the objects are drawn according to the order of appearance (“index”) or sorted by their y-coordinate (“topdown”). (defaults to “topdown”)
        /// </summary>
        public string DrawOrder { get; set; }
        /// <summary>
        /// The list of objects in case of an objectgroup layer.<br/>
        /// Is null when the layer has no objects.<br/>
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public TiledObject[] Objects { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);
            this.Color = node.Attributes.GetStringOrDefault("color", "#a0a0a4");
            this.DrawOrder = node.Attributes.GetStringOrDefault("draworder", "topdown");
            this.Objects = TryCreateFrom(node.SelectNodes("object"), TiledObject.TryCreateFrom);
        }
    }
}
