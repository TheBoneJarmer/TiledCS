using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace TiledCS
{
    partial class TiledTileset
    {
        /// <summary>
        /// Loads a tileset in TSX format and parses it
        /// </summary>
        /// <param name="path">The file path of the TSX file</param>
        /// <exception cref="TiledException">Thrown when the file could not be found or parsed</exception>
        public TiledTileset(string path)
        {
            // Check the file
            if (!File.Exists(path)) throw new TiledException($"{path} not found");
            if (!path.EndsWith(".tsx")) throw new TiledException("Unsupported file format");
            
            var content = File.ReadAllText(path);
            
            ParseXml(content);            
        }

        /// <summary>
        /// Can be used to parse the content of a TSX tileset manually instead of loading it using the constructor
        /// </summary>
        /// <param name="xml">The tmx file content as string</param>
        /// <exception cref="TiledException"></exception>
        public void ParseXml(string xml)
        {
#if !DEBUG
            try {
#endif
            var document = new XmlDocument();
            document.LoadXml(xml);

            var nodeTileset = document.SelectSingleNode("tileset");

            ParseXml(nodeTileset);            

#if !DEBUG
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled tileset", ex);
            }
#endif
        }

        protected override void ParseXml(XmlNode node)
        {
            base.ParseXml(node);            

            TiledVersion = node.Attributes["tiledversion"].Value;
            Name = node.Attributes.GetStringOrDefault("name", null);
            TileWidth = int.Parse(node.Attributes["tilewidth"].Value);
            TileHeight = int.Parse(node.Attributes["tileheight"].Value);
            TileCount = int.Parse(node.Attributes["tilecount"].Value);
            Columns = int.Parse(node.Attributes["columns"].Value);
            Margin = node.Attributes.GetIntegerOrDefault("margin", 0);
            Spacing = node.Attributes.GetIntegerOrDefault("spacing", 0);
            Image = TryCreateFrom<TiledImage>(node.SelectSingleNode("image"));
            Tiles = TryCreateFrom<TiledTile>(node.SelectNodes("tile"));            
            Terrains = TryCreateFrom<TiledTerrain>(node.SelectNodes("terraintypes/terrain"));
        }        
    }
}
