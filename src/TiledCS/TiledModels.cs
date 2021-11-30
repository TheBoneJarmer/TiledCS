using System;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a property object in both tilesets, maps, layers and objects. Values are all in string but you can use the 'type' property for conversions
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Property {type,nq} {name} {value}")]
    public partial class TiledProperty : TiledSerializable
    {
        /// <summary>
        /// The property name or key in string format
        /// </summary>
        public string name;
        /// <summary>
        /// The property type as used in Tiled. Can be bool, number, string, ...
        /// </summary>
        public string type;
        /// <summary>
        /// The value in string format
        /// </summary>
        public string value;

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            this.name = node.Attributes["name"].Value;
            this.type = node.Attributes.GetStringOrDefault("type", null);
            this.value = node.Attributes.GetStringOrDefault("value", null);

            if (this.value == null && node.InnerText != null)
            {
                this.value = node.InnerText;
            }
        }
    }

    /// <summary>
    /// Represents a base class containing a collection of <see cref="TiledProperty"/>.
    /// </summary>
    public abstract partial class TiledProperties : TiledSerializable
    {
        /// <summary>
        /// The layer properties if set
        /// </summary>
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public TiledProperty[] properties { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            properties = TryCreateFrom<TiledProperty>(node.SelectNodes("properties/property"))
                ?? Array.Empty<TiledProperty>();
        }
    }

    /// <summary>
    /// Represents a base class containing an Id and a Name.
    /// </summary>
    public abstract partial class TiledIdName : TiledProperties
    {
        /// <summary>
        /// Unique ID of the object. Each object that is placed on a map gets a unique id.<br/>
        /// Even if an object was deleted, no object gets the same ID.<br/>
        /// </summary>
        /// <remarks>
        /// Can not be changed in Tiled.<br/>
        /// (since Tiled 0.11)
        /// </remarks>
        public int Id { get; set; }
        /// <summary>
        /// The name of the object. An arbitrary string. (defaults to “”)
        /// </summary>
        public string Name { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            this.Id = node.Attributes.GetSignedOrDefault("id", 0);
            this.Name = node.Attributes.GetStringOrDefault("name", string.Empty);
        }

        /// <summary>
        /// Gets debugger information from this object.
        /// </summary>
        /// <returns>A debug information string.</returns>
        protected virtual string GetDebuggerString()
        {
            var txt = string.Empty;
            if (Id != 0) txt += $" Id={Id}";
            if (!string.IsNullOrWhiteSpace(Name)) txt += $" Name=\"{Name}\"";
            return txt.TrimStart();
        }
    }    

    

    /// <summary>
    /// Used as data type for the GetSourceRect method. Represents basically a rectangle.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Rect ({X},{Y}) {Width}x{Height}")]
    public class TiledSourceRect
    {
        /// <summary>
        /// The x position in pixels from the tile location in the source image
        /// </summary>
        public int X;
        /// <summary>
        /// The y position in pixels from the tile location in the source image
        /// </summary>
        public int Y;
        /// <summary>
        /// The width in pixels from the tile in the source image
        /// </summary>
        public int Width;
        /// <summary>
        /// The height in pixels from the tile in the source image
        /// </summary>
        public int Height;
    }    
}