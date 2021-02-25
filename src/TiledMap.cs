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
        public string TiledVersion { get; set; }
        public TiledProperty[] Properties { get; set; }
        public TiledMapTileset[] Tilesets { get; set; }
        public TiledLayer[] Layers { get; set; }
        public string Orientation { get; set; }
        public string RenderOrder { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public TiledMap()
        {

        }

        public TiledMap(string path)
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

                this.TiledVersion = nodeMap.Attributes["tiledversion"].Value;
                this.Orientation = nodeMap.Attributes["orientation"].Value;
                this.RenderOrder = nodeMap.Attributes["renderorder"].Value;

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
                var attrVisible = node.Attributes["visible"];

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

                if (attrVisible != null)
                {
                    tiledLayer.visible = attrVisible.Value == "1" ? true : false;
                }

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
                obj.x = float.Parse(node.Attributes["x"].Value);
                obj.y = float.Parse(node.Attributes["y"].Value);
                obj.properties = ParseProperties(nodesProperty);

                if (node.Attributes["width"] != null)
                {
                    obj.width = float.Parse(node.Attributes["width"].Value);
                }
                if (node.Attributes["height"] != null)
                {
                    obj.height = float.Parse(node.Attributes["height"].Value);
                }               

                result.Add(obj);
            }

            return result.ToArray();
        }
    }
}