using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace TiledCS
{
    public class TiledTileset
    {
        public string TiledVersion { get; set; }
        public string Name { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int TileCount { get; set; }
        public int Columns { get; set; }
        public string Image { get; set; }
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public int Spacing { get; set; }
        public int Margin { get; set; }
        public TiledTile[] Tiles { get; set; }
        public TiledTerrain[] Terrains { get; set; }
        public TiledProperty[] Properties { get; set; }

        public TiledTileset()
        {

        }

        public TiledTileset(string path)
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

                TiledVersion = nodeTileset.Attributes["tiledversion"].Value;
                Name = nodeTileset.Attributes["name"]?.Value;
                TileWidth = int.Parse(nodeTileset.Attributes["tilewidth"].Value);
                TileHeight = int.Parse(nodeTileset.Attributes["tileheight"].Value);
                TileCount = int.Parse(nodeTileset.Attributes["tilecount"].Value);
                Columns = int.Parse(nodeTileset.Attributes["columns"].Value);

                if (nodeTileset.Attributes["margin"] != null)
                {
                    Margin = int.Parse(nodeTileset.Attributes["margin"].Value);
                }
                if (nodeTileset.Attributes["spacing"] != null)
                {
                    Spacing = int.Parse(nodeTileset.Attributes["spacing"].Value);
                }

                Image = nodeImage.Attributes["source"].Value;
                ImageWidth = int.Parse(nodeImage.Attributes["width"].Value);
                ImageHeight = int.Parse(nodeImage.Attributes["height"].Value);

                Tiles = ParseTiles(nodesTile);
                Properties = ParseProperties(nodesProperty);
                Terrains = ParseTerrains(nodesTerrain);
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled tileset", ex);
            }
        }

        private TiledTileAnimation[] ParseAnimations(XmlNodeList nodeList)
        {
            var result = new List<TiledTileAnimation>();

            foreach (XmlNode node in nodeList)
            {
                var animation = new TiledTileAnimation();
                animation.tileid = int.Parse(node.Attributes["tileid"].Value);
                animation.duration = int.Parse(node.Attributes["duration"].Value);
                
                result.Add(animation);
            }
            
            return result.ToArray();
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
                var nodesAnimation = node.SelectNodes("animation/frame");

                var tile = new TiledTile();
                tile.id = int.Parse(node.Attributes["id"].Value);
                tile.terrain = node.Attributes["terrain"]?.Value.Split(',').AsIntArray();
                tile.properties = ParseProperties(nodesProperty);
                tile.animation = ParseAnimations(nodesAnimation);

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