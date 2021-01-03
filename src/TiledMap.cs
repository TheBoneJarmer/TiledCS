using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Reapenshaw.Core;

namespace Reapenshaw.Tiled
{
    public class TiledMap
    {
        public Dictionary<string, string> Properties { get; set; }
        public Dictionary<int, string> Tilesets { get; set; }
        public List<TiledLayer> Layers { get; private set; }
        public List<TiledObjectGroup> ObjectGroups { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        
        public TiledMap()
        {
            Layers = new List<TiledLayer>();
            ObjectGroups = new List<TiledObjectGroup>();
            Properties = new Dictionary<string, string>();
            Tilesets = new Dictionary<int, string>();
        }

        public void Load(string path)
        {
            // Check the file
            if (!File.Exists(path))
            {
                throw new TiledException("Map not found");
            }
            if (!path.EndsWith(".tmx"))
            {
                throw new TiledException("Invalid Tiled map format");
            }

            Parse(File.ReadAllText(path));
        }

        public void Parse(string xml)
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

        private void ParseProperties(XmlNode node)
        {
            var nodesProperty = node.SelectNodes("property");
                
            for (var i=0; i<nodesProperty.Count; i++)
            {
                var nodeProperty = nodesProperty[i];
                var propertyName = nodeProperty.Attributes["name"].Value;
                var propertyValue = nodeProperty.Attributes["value"].Value;

                Properties.Add(propertyName, propertyValue);
            }
        }

        private void ParseTilesets(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                int firstgid = int.Parse(node.Attributes["firstgid"].Value);
                string source = node.Attributes["source"].Value;

                Tilesets.Add(firstgid, source);
            }
        }

        private void ParseLayers(XmlNodeList nodeList)
        {
            foreach (XmlNode node in nodeList)
            {
                var nodeData = node.SelectSingleNode("data");

                TiledLayer tiledLayer = new TiledLayer();
                tiledLayer.Id = int.Parse(node.Attributes["id"].Value);
                tiledLayer.Name = node.Attributes["name"].Value;
                tiledLayer.Width = int.Parse(node.Attributes["width"].Value);
                tiledLayer.Height = int.Parse(node.Attributes["height"].Value);
                tiledLayer.Data = nodeData.InnerText.Replace("\n", "").Split(',').AsIntArray();

                Layers.Add(tiledLayer);
            }
        }

        private void ParseObjectGroups(XmlNodeList nodeList)
        {
            for (var i=0; i<nodeList.Count; i++) {
                var nodeGroup = nodeList[i];
                var nodesObjects = nodeGroup.SelectNodes("object");

                var group = new TiledObjectGroup();
                group.Id = int.Parse(nodeGroup.Attributes["id"].Value);
                group.Name = nodeGroup.Attributes["name"].Value;

                for (var j=0; j<nodesObjects.Count; j++) {
                    var nodeObject = nodesObjects[j];
                    var nodesProperties = nodeObject.SelectNodes("properties/property");

                    var obj = new TiledObject();
                    obj.Id = int.Parse(nodeObject.Attributes["id"].Value);
                    obj.Name = nodeObject.Attributes["name"].Value;
                    obj.X = int.Parse(nodeObject.Attributes["x"].Value);
                    obj.Y = int.Parse(nodeObject.Attributes["y"].Value);

                    if (nodeObject.Attributes["width"] != null)
                    {
                        obj.Width = int.Parse(nodeObject.Attributes["width"].Value);
                    }
                    if (nodeObject.Attributes["height"] != null)
                    {
                        obj.Height = int.Parse(nodeObject.Attributes["height"].Value);
                    }

                    for (var k=0; k<nodesProperties.Count; k++) {
                        var nodeProperty = nodesProperties[k];
                        var attribName = nodeProperty.Attributes["name"];
                        var attribValue = nodeProperty.Attributes["value"];

                        if (attribValue != null)
                        {
                            obj.Properties.Add(attribName.Value, attribValue.Value);
                        }
                        else
                        {
                            obj.Properties.Add(attribName.Value, nodeProperty.InnerText);
                        }
                    }

                    group.Objects.Add(obj);
                }

                ObjectGroups.Add(group);
            }
        }
    }
}