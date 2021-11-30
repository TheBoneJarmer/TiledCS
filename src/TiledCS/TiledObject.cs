using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents an tiled object defined in object layers
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Object {GetDebuggerString(),nq}")]
    public partial class TiledObject : TiledIdName
    {
        /// <summary>
        /// The object type if defined. Null if none was set.
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// A reference to a tile. (optional)
        /// </summary>
        public int GlobalId { get; set; }
        /// <summary>
        /// The object's x position in pixels
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// The object's y position in pixels
        /// </summary>
        public float Y { get; set; }
        /// <summary>
        /// The object's width in pixels
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        /// The object's height in pixels
        /// </summary>
        public float Height { get; set; }
        /// <summary>
        /// The rotation of the object in degrees clockwise around (x, y).
        /// </summary>
        public int Rotation { get; set; }

        internal static TiledObject TryCreateFrom(XmlNode node)
        {
            var nodePoint = node.SelectSingleNode("point");
            var nodeEllipse = node.SelectSingleNode("ellipse");
            var nodePolygon = node.SelectSingleNode("polygon");
            var nodePolyline = node.SelectSingleNode("polyline");
            var nodeText = node.SelectSingleNode("text");

            if (nodePoint != null) return TryCreateFrom<TiledPoint>(node);
            if (nodeEllipse != null) return TryCreateFrom<TiledEllipse>(node);
            if (nodePolygon != null) return TryCreateFrom<TiledPolygon>(node);
            if (nodePolyline != null) return TryCreateFrom<TiledPolyline>(node);
            if (nodeText != null) return TryCreateFrom<TiledText>(node);

            return TryCreateFrom<TiledObject>(node);
        }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            this.Type = node.Attributes.GetStringOrDefault("type", null);
            this.GlobalId = (int)node.Attributes.GetUnsignedOrDefault("gid", 0);
            this.X = node.Attributes.GetSingleOrDefault("x", 0);
            this.Y = node.Attributes.GetSingleOrDefault("y", 0);
            this.Width = node.Attributes.GetSingleOrDefault("width", 0);
            this.Height = node.Attributes.GetSingleOrDefault("height", 0);
            this.Rotation = node.Attributes.GetSignedOrDefault("rotation", 0);              
        }

        /// <summary>
        /// Parses a collection of consecutive points.
        /// </summary>
        /// <param name="node">The xml source</param>
        /// <returns>an array of floats.</returns>
        protected static float[] ParseXmlPoints(XmlNode node)
        {
            var vertices = node.Attributes["points"].Value.Split(' ');

            var points = new float[vertices.Length * 2];

            for (var i = 0; i < vertices.Length; i++)
            {
                points[(i * 2) + 0] = float.Parse(vertices[i].Split(',')[0], CultureInfo.InvariantCulture);
                points[(i * 2) + 1] = float.Parse(vertices[i].Split(',')[1], CultureInfo.InvariantCulture);
            }

            return points;
        }

        /// <inheritdoc />
        protected override string GetDebuggerString()
        {
            var txt = base.GetDebuggerString();
            if (!string.IsNullOrWhiteSpace(Type)) txt += $" Type={Type}";
            if (GlobalId != 0) txt += $" TileId={GlobalId}";
            if (X != 0 || Y!=0) txt += $" ({X} {Y})";
            if (Width != 0 || Height != 0) txt += $" ({Width}x{Height})";
            if (Rotation != 0) txt += $" {Rotation}°";

            return txt.TrimStart();
        }
    }

    /// <summary>
    /// Represents a point shape
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Point {GetDebuggerString(),nq}")]
    public class TiledPoint : TiledObject { }

    /// <summary>
    /// Represents an ellipse shape
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Ellipse {GetDebuggerString(),nq}")]
    public class TiledEllipse : TiledObject { }

    /// <summary>
    /// Represents a polygon shape
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Polygon {GetDebuggerString(),nq}")]
    public class TiledPolygon : TiledObject
    {
        /// <summary>
        /// The array of vertices where each two elements represent an x and y position. Like 'x,y,x,y,x,y,x,y'.
        /// </summary>
        public float[] Points { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            var nodePolygon = node.SelectSingleNode("polygon");

            if (nodePolygon != null)
            {
                Points = ParseXmlPoints(nodePolygon);
            }
        }
    }

    /// <summary>
    /// Represents a polyline shape
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Polyline {GetDebuggerString(),nq}")]
    public class TiledPolyline : TiledObject
    {
        /// <summary>
        /// The array of vertices where each two elements represent an x and y position. Like 'x,y,x,y,x,y,x,y'.
        /// </summary>
        public float[] Points { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            var nodePolyline = node.SelectSingleNode("polyline");

            if (nodePolyline != null)
            {
                Points = ParseXmlPoints(nodePolyline);
            }
        }
    }

    /// <summary>
    /// Represents a point shape
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Text {GetDebuggerString(),nq}")]
    public class TiledText : TiledObject
    {
        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);
        }
    }    
}
