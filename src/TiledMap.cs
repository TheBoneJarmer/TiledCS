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
        public List<TiledProperty> Properties { get; set; }
        public List<TiledMapTileset> Tilesets { get; set; }
        public List<TiledLayer> Layers { get; set; }
        public List<TiledObjectGroup> ObjectGroups { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        
        public TiledMap()
        {
            Layers = new List<TiledLayer>();
            ObjectGroups = new List<TiledObjectGroup>();
            Properties = new List<TiledProperty>();
            Tilesets = new List<TiledMapTileset>();
        }

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
                var nodeProperties = nodeMap.SelectSingleNode("properties");
                var nodesLayers = nodeMap.SelectNodes("layer");
                var nodesObjectGroups = nodeMap.SelectNodes("objectgroup");
                var nodesTilesets = nodeMap.SelectNodes("tileset");

                this.Version = ParseVersion(nodeMap.Attributes["version"].Value);
                this.TiledVersion = ParseVersion(nodeMap.Attributes["tiledversion"].Value);

                this.Width = int.Parse(nodeMap.Attributes["width"].Value);
                this.Height = int.Parse(nodeMap.Attributes["height"].Value);
                this.TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
                this.TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

                // Import the properties
                if (nodeProperties != null)
                {
                    ParseProperties(nodeProperties);
                }

                // Import everything else
                ParseTilesets(nodesTilesets);
                ParseLayers(nodesLayers);
                ParseObjectGroups(nodesObjectGroups);
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

        private void ParseProperties(XmlNode node)
        {
            var nodesProperty = node.SelectNodes("property");
                
            for (var i=0; i<nodesProperty.Count; i++)
            {
                var nodeProperty = nodesProperty[i];

                TiledProperty property = new TiledProperty();
                property.name = nodeProperty.Attributes["name"].Value;
                property.type = nodeProperty.Attributes["type"]?.Value;
                property.value = nodeProperty.Attributes["value"]?.Value;

                if (property.value == null && nodeProperty.InnerText != null)
                {
                    property.value = nodeProperty.InnerText;
                }

                Properties.Add(property);
            }
        }

        private void ParseTilesets(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                TiledMapTileset tileset = new TiledMapTileset();
                tileset.firstgid = int.Parse(node.Attributes["firstgid"].Value);
                tileset.source = node.Attributes["source"].Value;

                Tilesets.Add(tileset);
            }
        }

        private void ParseLayers(XmlNodeList nodeList)
        {
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

                Layers.Add(tiledLayer);
            }
        }

        private void ParseObjectGroups(XmlNodeList nodeList)
        {
            for (var i=0; i<nodeList.Count; i++) {
                var nodeGroup = nodeList[i];
                var nodesObjects = nodeGroup.SelectNodes("object");
                
                TiledObjectGroup group = new TiledObjectGroup();
                group.id = int.Parse(nodeGroup.Attributes["id"].Value);
                group.name = nodeGroup.Attributes["name"].Value;
                group.objects = new List<TiledObject>();

                for (var j=0; j<nodesObjects.Count; j++) {
                    var nodeObject = nodesObjects[j];
                    var nodesProperties = nodeObject.SelectNodes("properties/property");
                    
                    TiledObject obj = new TiledObject();
                    obj.id = int.Parse(nodeObject.Attributes["id"].Value);
                    obj.name = nodeObject.Attributes["name"]?.Value;
                    obj.type = nodeObject.Attributes["type"]?.Value;
                    obj.x = int.Parse(nodeObject.Attributes["x"].Value);
                    obj.y = int.Parse(nodeObject.Attributes["y"].Value);
                    obj.width = int.Parse(nodeObject.Attributes["width"]?.Value);
                    obj.height = int.Parse(nodeObject.Attributes["height"]?.Value);
                    obj.properties = new List<TiledProperty>();

                    for (var k=0; k<nodesProperties.Count; k++) {
                        var nodeProperty = nodesProperties[k];
                        
                        TiledProperty property = new TiledProperty();
                        property.name = nodeProperty.Attributes["name"].Value;
                        property.type = nodeProperty.Attributes["type"]?.Value;
                        property.value = nodeProperty.Attributes["value"]?.Value;

                        if (property.value == null && nodeProperty.InnerText != null)
                        {
                            property.value = nodeProperty.InnerText;
                        }

                        obj.properties.Add(property);
                    }

                    group.objects.Add(obj);
                }

                ObjectGroups.Add(group);
            }
        }
    }
}