using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

namespace TiledSharp
{
    public class TiledMap
    {
        public TiledVersion TiledVersion { get; set; }
        public TiledVersion Version { get; set; }
        public TiledProperty[] Properties { get; set; }
        public TiledMapTileset[] Tilesets { get; set; }
        public TiledLayer[] Layers { get; set; }
        public TiledObjectGroup[] ObjectGroups { get; set; }
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

                this.Width = int.Parse(nodeMap.Attributes["width"].Value);
                this.Height = int.Parse(nodeMap.Attributes["height"].Value);
                this.TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
                this.TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

                if (nodesProperty != null) Properties = ParseProperties(nodesProperty);
                if (nodesTileset != null) Tilesets = ParseTilesets(nodesTileset);
                if (nodesLayer != null) Layers = ParseLayers(nodesLayer);
                if (nodesObjectGroup != null) ObjectGroups = ParseObjectGroups(nodesObjectGroup);
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled map", ex);
            }
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

        private TiledLayer[] ParseLayers(XmlNodeList nodeList)
        {
            var result = new List<TiledLayer>();

            foreach (XmlNode node in nodeList)
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

                result.Add(tiledLayer);
            }

            return result.ToArray();
        }

        private TiledObjectGroup[] ParseObjectGroups(XmlNodeList nodeList)
        {
            var result = new List<TiledObjectGroup>();

            foreach (XmlNode node in nodeList)
            {
                var nodesObject = node.SelectNodes("object");
                
                TiledObjectGroup group = new TiledObjectGroup();
                group.id = int.Parse(node.Attributes["id"].Value);
                group.name = node.Attributes["name"].Value;
                group.objects = ParseObjects(nodesObject);

                result.Add(group);
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
                obj.width = int.Parse(node.Attributes["width"]?.Value);
                obj.height = int.Parse(node.Attributes["height"]?.Value);
                obj.properties = ParseProperties(nodesProperty);

                result.Add(obj);
            }

            return result.ToArray();
        }
    }
}