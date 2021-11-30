using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a group of <see cref="TiledLayer"/>.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Group {GetDebuggerString(),nq}")]
    public class TiledGroupLayer : TiledLayer
    {
        /// <summary>
        /// Returns an array of layers or null if none were defined
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public TiledLayer[] Layers { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            Layers = TiledLayer.TryCreateFrom(node).ToArray();
        }
    }
}
