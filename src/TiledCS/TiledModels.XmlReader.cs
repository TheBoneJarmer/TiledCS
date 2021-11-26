using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml;
using System.IO.Compression;
using System.Globalization;

namespace TiledCS
{
    public abstract class TiledSerializable
    {
        public static T[] TryCreateFrom<T>(XmlNodeList nodeList)
            where T : TiledSerializable, new()
        {
            if (nodeList == null) return null; // it could be replaced by Array.Empty<T>();

            return nodeList
                .Cast<XmlNode>()
                .Select(TryCreateFrom<T>)
                .ToArray();
        }

        public static T TryCreateFrom<T>(XmlNode node)
            where T:TiledSerializable, new()
        {
            if (node == null) return null;
            var item = new T();
            item.ParseXml(node);
            return item;
        }

        protected abstract void ParseXml(XmlNode node);
    }

    partial class TiledProperty : TiledSerializable
    {
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

    partial class TiledProperties : TiledSerializable
    {
        protected override void ParseXml(XmlNode node)
        {
            properties = TryCreateFrom<TiledProperty>(node.SelectNodes("properties/property"))
                ?? Array.Empty<TiledProperty>();
        }        
    }

    partial class TiledIdName
    {
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            this.id = node.Attributes.GetIntegerOrDefault("id", 0);
            this.name = node.Attributes.GetStringOrDefault("name", null);
        }
    }

    partial class TiledVisibleLocked
    {
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            visible = node.Attributes.GetIntegerOrDefault("visible", 1) == 1;
            locked = node.Attributes.GetIntegerOrDefault("locked", 0) == 1;
        }
    }

    partial class TiledTile
    {
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);            

            id = int.Parse(node.Attributes["id"].Value);
            type = node.Attributes.GetStringOrDefault("type", null);
            terrain = node.Attributes.GetStringOrDefault("terrain", string.Empty).Split(',').AsIntArray();            
            animation = TryCreateFrom<TiledTileAnimation>(node.SelectNodes("animation/frame"));
            image = TryCreateFrom<TiledImage>(node.SelectSingleNode("image"));
        }
    }

    partial class TiledTileAnimation : TiledSerializable
    {
        protected override void ParseXml(XmlNode node)
        {
            tileid = int.Parse(node.Attributes["tileid"].Value);
            duration = int.Parse(node.Attributes["duration"].Value);
        }
    }

    partial class TiledTerrain : TiledSerializable
    {
        protected override void ParseXml(XmlNode node)
        {
            name = node.Attributes["name"].Value;
            tile = int.Parse(node.Attributes["tile"].Value);
        }
    }

    partial class TiledImage : TiledSerializable
    {
        protected override void ParseXml(XmlNode node)
        {
            source = node.Attributes["source"].Value;
            width = node.Attributes.GetIntegerOrDefault("width", 0);
            height = node.Attributes.GetIntegerOrDefault("height", 0);            
        }
    }

    partial class TiledObject
    {
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);            
            
            this.type = node.Attributes.GetStringOrDefault("type", null);
            this.gid = node.Attributes.GetIntegerOrDefault("gid", 0);
            this.x = node.Attributes.GetSingleOrDefault("x", 0);
            this.y = node.Attributes.GetSingleOrDefault("y", 0);
            this.width = node.Attributes.GetSingleOrDefault("width", 0);
            this.height = node.Attributes.GetSingleOrDefault("height", 0);
            this.rotation = node.Attributes.GetIntegerOrDefault("rotation", 0);

            var nodePolygon = node.SelectSingleNode("polygon");
            var nodePoint = node.SelectSingleNode("point");
            var nodeEllipse = node.SelectSingleNode("ellipse");

            if (nodeEllipse != null) this.ellipse = new TiledEllipse();
            if (nodePoint != null) this.point = new TiledPoint();            

            if (nodePolygon != null)
            {
                var points = nodePolygon.Attributes["points"].Value;
                var vertices = points.Split(' ');

                var polygon = new TiledPolygon();
                polygon.points = new float[vertices.Length * 2];

                for (var i = 0; i < vertices.Length; i++)
                {
                    polygon.points[(i * 2) + 0] = float.Parse(vertices[i].Split(',')[0], CultureInfo.InvariantCulture);
                    polygon.points[(i * 2) + 1] = float.Parse(vertices[i].Split(',')[1], CultureInfo.InvariantCulture);
                }

                this.polygon = polygon;
            }            
        }
    }    

    partial class TiledMapTileset : TiledSerializable
    {
        protected override void ParseXml(XmlNode node)
        {
            this.firstgid = int.Parse(node.Attributes["firstgid"].Value);
            this.source = node.Attributes["source"].Value;
        }
    }

    partial class TiledGroup
    {
        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);            

            var nodesLayer = node.SelectNodes("layer");
            var nodesObjectGroup = node.SelectNodes("objectgroup");
            var nodesImageLayer = node.SelectNodes("imagelayer");

            this.groups = TryCreateFrom<TiledGroup>(node.SelectNodes("group"));
            this.layers = TiledLayer.TryCreateFrom(nodesLayer, nodesObjectGroup, nodesImageLayer).ToArray();
        }
    }

    partial class TiledLayer
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        /// <summary>
        /// How many times we shift the FLIPPED flags to the right in order to store it in a byte.
        /// For example: 0b10100000000000000000000000000000 >> SHIFT_FLIP_FLAG_TO_BYTE = 0b00000101
        /// </summary>
        const int SHIFT_FLIP_FLAG_TO_BYTE = 29;

        public static IEnumerable<TiledLayer> TryCreateFrom(XmlNodeList nodesLayer, XmlNodeList nodesObjectGroup, XmlNodeList nodesImageLayer)
        {
            foreach (XmlNode node in nodesLayer)
            {
                yield return TryCreateFrom(node, "tilelayer");
            }

            foreach (XmlNode node in nodesObjectGroup)
            {
                yield return TryCreateFrom(node, "objectgroup");
            }

            foreach (XmlNode node in nodesImageLayer)
            {
                yield return TryCreateFrom(node, "imagelayer");
            }
        }

        public static TiledLayer TryCreateFrom(XmlNode node, string type)
        {
            if (node == null) return null;
            var tile = new TiledLayer();
            tile.type = type;
            tile.ParseXml(node);
            return tile;
        }

        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);

            this.offsetX = node.Attributes.GetIntegerOrDefault("offsetx", 0);
            this.offsetY = node.Attributes.GetIntegerOrDefault("offsety", 0);
            this.width = node.Attributes.GetIntegerOrDefault("width", 0);
            this.height = node.Attributes.GetIntegerOrDefault("height", 0);
            this.tintcolor = node.Attributes.GetStringOrDefault("tintcolor", null);
            
            if (this.type == "objectgroup")
            {
                this.objects = TryCreateFrom<TiledObject>(node.SelectNodes("object"));
            }
            if (this.type == "imagelayer")
            {
                this.image = TryCreateFrom<TiledImage>(node.SelectSingleNode("image"));
            }
            if (this.type == "tilelayer")
            {
                var nodeData = node.SelectSingleNode("data");

                var nodeChunks = nodeData.SelectNodes("chunk");

                if (nodeChunks == null)
                {
                    var (data, flags) = TiledXmlTileDecoder.ParseTiles(nodeData);
                    this.data = data;
                    this.dataRotationFlags = flags;
                }
                else
                {
                    this.chunks = TryCreateFrom<TiledLayerChunk>(nodeChunks);
                }
            }
        }        
    }

    partial class TiledLayerChunk : TiledSerializable
    {
        protected override void ParseXml(XmlNode node)
        {
            this.offsetX = node.Attributes.GetIntegerOrDefault("x", 0);
            this.offsetY = node.Attributes.GetIntegerOrDefault("y", 0);
            this.width = node.Attributes.GetIntegerOrDefault("width", 0);
            this.height = node.Attributes.GetIntegerOrDefault("height", 0);

            var (data, flags) = TiledXmlTileDecoder.ParseTiles(node);

            this.data = data;
            this.dataRotationFlags = flags;
        }
    }

}
