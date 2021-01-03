using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace TiledSharp
{
    public class TiledTileset
    {
        public string Name { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int TileCount { get; set; }
        public int Columns { get; set; }
        public string ImageSource { get; set; }
        public TiledTile[] Tiles { get; set; }
        public TiledTerrain[] Terrains { get; set; }
        public TiledProperty[] Properties { get; set; }
        
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

            if (path.EndsWith(".tsx"))
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
                var document = new XmlDocument();
                document.LoadXml(xml);

                var nodeTileset = document.SelectSingleNode("tileset");
                var nodeImage = nodeTileset.SelectSingleNode("image");
                var nodesTile = nodeTileset.SelectNodes("tile");
                var nodesProperty = nodeTileset.SelectNodes("properties/property");
                var nodesTerrain = nodeTileset.SelectNodes("terraintypes/terrain");

                Name = nodeTileset.Attributes["name"].Value;
                TileWidth = int.Parse(nodeTileset.Attributes["tilewidth"].Value);
                TileHeight = int.Parse(nodeTileset.Attributes["tileheight"].Value);
                TileCount = int.Parse(nodeTileset.Attributes["tilecount"].Value);
                Columns = int.Parse(nodeTileset.Attributes["columns"].Value);
                ImageSource = nodeImage.Attributes["source"].Value;

                Tiles = ParseTiles(nodesTile);
                Properties = ParseProperties(nodesProperty);
                Terrains = ParseTerrains(nodesTerrain);
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled tileset", ex);
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

        private TiledTile[] ParseTiles(XmlNodeList nodeList)
        {
            var result = new List<TiledTile>();

            foreach (XmlNode node in nodeList)
            {
                var nodesProperty = node.SelectNodes("properties/property");

                var tile = new TiledTile();
                tile.id = int.Parse(node.Attributes["id"].Value);
                tile.terrain = node.Attributes["terrain"]?.Value.Split(',').AsIntArray();
                tile.properties = ParseProperties(nodesProperty);

                result.Add(tile);
            }

            return result.ToArray();
        }

        private TiledTerrain[] ParseTerrains(XmlNodeList nodeList)
        {
            var result = new List<TiledTerrain>();

            foreach (XmlNode node in nodeList)
            {
                var terrain = new TiledTerrain();
                terrain.name = node.Attributes["name"].Value;
                terrain.tile = int.Parse(node.Attributes["tile"].Value);

                result.Add(terrain);
            }

            return result.ToArray();
        }
    }
}