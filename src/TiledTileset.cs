using System;
using System.Collections.Generic;
using System.Xml;
using Reapenshaw.Core;

namespace Reapenshaw.Tiled
{
    public class TiledTileset
    {
        public string Name { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int TileCount { get; set; }
        public int Columns { get; set; }
        public string ImageSource { get; set; }
        public List<TiledTile> Tiles { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public TiledTileset()
        {
            Tiles = new List<TiledTile>();
            Properties = new Dictionary<string, string>();
        }
        public TiledTileset(string xml) : this()
        {
            Parse(xml);
        }

        public void Parse(string xml)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(xml);

                XmlNode nodeTileset = document.SelectSingleNode("tileset");
                XmlNode nodeImage = nodeTileset.SelectSingleNode("image");
                XmlNodeList nodesTile = nodeTileset.SelectNodes("tile");
                XmlNodeList nodesProperty = nodeTileset.SelectNodes("properties/property");

                Name = nodeTileset.Attributes["name"].Value;
                TileWidth = int.Parse(nodeTileset.Attributes["tilewidth"].Value);
                TileHeight = int.Parse(nodeTileset.Attributes["tileheight"].Value);
                TileCount = int.Parse(nodeTileset.Attributes["tilecount"].Value);
                Columns = int.Parse(nodeTileset.Attributes["columns"].Value);
                ImageSource = nodeImage.Attributes["source"].Value;

                foreach (XmlNode nodeTile in nodesTile)
                {
                    TiledTile tiledTile = new TiledTile();
                    XmlNodeList nodeProperties = nodeTile.SelectNodes("properties/property");

                    tiledTile.Id = int.Parse(nodeTile.Attributes["id"].Value);

                    foreach (XmlNode nodeProperty in nodeProperties)
                    {
                        string propertyName = nodeProperty.Attributes["name"].Value;
                        string propertyValue = nodeProperty.Attributes["value"].Value;

                        tiledTile.Properties.Add(propertyName, propertyValue);
                    }

                    Tiles.Add(tiledTile);
                }

                foreach (XmlNode nodeProperty in nodesProperty)
                {
                    string propertyName = nodeProperty.Attributes["name"].Value;
                    string propertyValue = nodeProperty.Attributes["value"].Value;

                    Properties.Add(propertyName, propertyValue);
                }
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled tileset", ex);
            }
        }
    }
}