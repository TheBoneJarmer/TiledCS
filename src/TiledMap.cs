using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace TiledCS
{
    public class TiledMap
    {
        public TiledVersion TiledVersion { get; set; }
        public TiledVersion Version { get; set; }
        public TiledProperty[] Properties { get; set; }
        public TiledMapTileset[] Tilesets { get; set; }
        public TiledLayer[] Layers { get; set; }
        public Orientation Orientation { get; set; }
        public RenderOrder RenderOrder { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public void Load(string path)
        {
            var content = "";

            // Check the file
            if (!File.Exists(path))
            {
                throw new TiledException($"{path} not found");
            }
            else
            {
                content = File.ReadAllText(path);
            }

            if (path.EndsWith(".tmx"))
            {
                Parse(content);
            }
            else
            {
                throw new TiledException("Unsupported file format");
            }
        }

        private void Parse(string xml)
        {
            try
            {
                // Load the xml document
                var document = new XmlDocument();
                document.LoadXml(xml);

                var nodeMap = document.SelectSingleNode("map");
                var nodesProperty = nodeMap.SelectNodes("properties/property");
                var nodesLayer = nodeMap.SelectNodes("layer");
                var nodesObjectGroup = nodeMap.SelectNodes("objectgroup");
                var nodesTileset = nodeMap.SelectNodes("tileset");

                this.Version = ParseVersion(nodeMap.Attributes["version"].Value);
                this.TiledVersion = ParseVersion(nodeMap.Attributes["tiledversion"].Value);
                this.Orientation = ParseOrientation(nodeMap.Attributes["orientation"].Value);
                this.RenderOrder = ParseRenderOrder(nodeMap.Attributes["renderorder"].Value);

                this.Width = int.Parse(nodeMap.Attributes["width"].Value);
                this.Height = int.Parse(nodeMap.Attributes["height"].Value);
                this.TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
                this.TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

                if (nodesProperty != null) Properties = ParseProperties(nodesProperty);
                if (nodesTileset != null) Tilesets = ParseTilesets(nodesTileset);
                if (nodesLayer != null) Layers = ParseLayers(nodesLayer, nodesObjectGroup);
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled map", ex);
            }
        }

        private Orientation ParseOrientation(string value)
        {
            if (value == "orthogonal") return Orientation.Orthogonal;
            if (value == "hexagonal") return Orientation.Hexagonal;
            if (value == "isometric") return Orientation.Isometric;
            if (value == "staggered") return Orientation.Staggered;

            return Orientation.Unknown;
        }
        private RenderOrder ParseRenderOrder(string value)
        {
            if (value == "right-down") return RenderOrder.RightDown;
            if (value == "right-up") return RenderOrder.RightUp;
            if (value == "left-down") return RenderOrder.LeftDown;
            if (value == "left-up") return RenderOrder.LeftUp;

            return RenderOrder.Unknown;
        }

        private TiledVersion ParseVersion(string value)
        {
            TiledVersion version = new TiledVersion();

            if (Regex.IsMatch(value, @"[0-9]+\.[0-9]+\.\[0-9]"))
            {
                version.major = int.Parse(value.Split('.')[0]);
                version.minor = int.Parse(value.Split('.')[1]);
                version.patch = int.Parse(value.Split('.')[2]);
            }
            else if (Regex.IsMatch(value, @"[0-9]+\.[0-9]+"))
            {
                version.major = int.Parse(value.Split('.')[0]);
                version.minor = int.Parse(value.Split('.')[1]);
                version.patch = 0;
            }
            else
            {
                throw new TiledException($"Version string {value} is not a Major.Minor.Patch format");
            }

            return version;
        }

        private TiledProperty[] ParseProperties(XmlNodeList nodeList)
        {
            var result = new List<TiledProperty>();

            foreach (XmlNode node in nodeList)
            {
                var property = new TiledProperty();
                property.name = node.Attributes["name"].Value;
                property.type = node.Attributes["type"]?.Value;
                property.value = node.Attributes["value"]?.Value;

                if (property.value == null && node.InnerText != null)
                {
                    property.value = node.InnerText;
                }

                result.Add(property);
            }

            return result.ToArray();
        }

        private TiledMapTileset[] ParseTilesets(XmlNodeList nodeList)
        {
            var result = new List<TiledMapTileset>();

            foreach (XmlNode node in nodeList)
            {
                TiledMapTileset tileset = new TiledMapTileset();
                tileset.firstgid = int.Parse(node.Attributes["firstgid"].Value);
                tileset.source = node.Attributes["source"].Value;

                result.Add(tileset);
            }

            return result.ToArray();
        }

        private TiledLayer[] ParseLayers(XmlNodeList nodeListLayers, XmlNodeList nodeListObjGroups)
        {
            var result = new List<TiledLayer>();

            foreach (XmlNode node in nodeListLayers)
            {
                var nodeData = node.SelectSingleNode("data");
                var encoding = nodeData.Attributes["encoding"].Value;

                if (encoding != "csv")
                {
                    throw new TiledException($"Only CSV encoding is currently supported");
                }

                TiledLayer tiledLayer = new TiledLayer();
                tiledLayer.id = int.Parse(node.Attributes["id"].Value);
                tiledLayer.name = node.Attributes["name"].Value;
                tiledLayer.height = int.Parse(node.Attributes["height"].Value);
                tiledLayer.width = int.Parse(node.Attributes["width"].Value);
                tiledLayer.data = nodeData.InnerText.Replace("\n", "").Split(',').AsIntArray();
                tiledLayer.type = "tilelayer";
                tiledLayer.visible = true;

                result.Add(tiledLayer);
            }

            foreach (XmlNode node in nodeListObjGroups)
            {
                var nodesObject = node.SelectNodes("object");

                TiledLayer tiledLayer = new TiledLayer();
                tiledLayer.id = int.Parse(node.Attributes["id"].Value);
                tiledLayer.name = node.Attributes["name"].Value;
                tiledLayer.objects = ParseObjects(nodesObject);
                tiledLayer.type = "objectgroup";
                tiledLayer.visible = true;

                result.Add(tiledLayer);
            }

            return result.ToArray();
        }

        private TiledObject[] ParseObjects(XmlNodeList nodeList)
        {
            var result = new List<TiledObject>();

            foreach (XmlNode node in nodeList)
            {
                var nodesProperty = node.SelectNodes("properties/property");
                    
                TiledObject obj = new TiledObject();
                obj.id = int.Parse(node.Attributes["id"].Value);
                obj.name = node.Attributes["name"]?.Value;
                obj.type = node.Attributes["type"]?.Value;
                obj.x = int.Parse(node.Attributes["x"].Value);
                obj.y = int.Parse(node.Attributes["y"].Value);
                obj.properties = ParseProperties(nodesProperty);

                if (node.Attributes["width"] != null)
                {
                    obj.width = int.Parse(node.Attributes["width"].Value);
                }
                if (node.Attributes["height"] != null)
                {
                    obj.height = int.Parse(node.Attributes["height"].Value);
                }               

                result.Add(obj);
            }

            return result.ToArray();
        }
    }
}