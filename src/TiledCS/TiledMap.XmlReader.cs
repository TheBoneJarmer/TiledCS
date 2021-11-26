using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace TiledCS
{    
    partial class TiledMap
    {
        /// <summary>
        /// Loads a Tiled map in TMX format and parses it
        /// </summary>
        /// <param name="path">The path to the tmx file</param>
        /// <exception cref="TiledException">Thrown when the map could not be loaded or is not in a correct format</exception>
        public TiledMap(string path)
        {
            // Check the file
            if (!File.Exists(path)) throw new TiledException($"{path} not found");
            if (!path.ToLowerInvariant().EndsWith(".tmx")) throw new TiledException("Unsupported file format");

            var content = File.ReadAllText(path);
            
            ParseXml(content);
        }

        /// <summary>
        /// Can be used to parse the content of a TMX map manually instead of loading it using the constructor
        /// </summary>
        /// <param name="xml">The tmx file content as string</param>
        /// <exception cref="TiledException"></exception>
        public void ParseXml(string xml)
        {
#if !DEBUG
            try {
#endif

            // Load the xml document
            var document = new XmlDocument();
            document.LoadXml(xml);
            ParseXml(document.SelectSingleNode("map"));            

#if !DEBUG
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled map", ex);
            }
#endif
        }

        protected override void ParseXml(XmlNode nodeMap)
        {
            base.ParseXml(nodeMap);

            var version = nodeMap.Attributes.GetStringOrDefault("version", "Unknown");
            this.TiledVersion = nodeMap.Attributes.GetStringOrDefault("tiledversion", version);

            this.Orientation = nodeMap.Attributes["orientation"].Value;
            this.RenderOrder = nodeMap.Attributes.GetStringOrDefault("renderorder", string.Empty);
            this.BackgroundColor = nodeMap.Attributes.GetStringOrDefault("backgroundcolor", null);
            this.Infinite = nodeMap.Attributes.GetStringOrDefault("infinite", "0") == "1";

            this.Width = int.Parse(nodeMap.Attributes["width"].Value);
            this.Height = int.Parse(nodeMap.Attributes["height"].Value);
            this.TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
            this.TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

            Tilesets = TryCreateFrom<TiledMapTileset>(nodeMap.SelectNodes("tileset"));
            Groups = TryCreateFrom<TiledGroup>(nodeMap.SelectNodes("group"));

            var nodesLayer = nodeMap.SelectNodes("layer");
            var nodesImageLayer = nodeMap.SelectNodes("imagelayer");
            var nodesObjectGroup = nodeMap.SelectNodes("objectgroup");
            Layers = TiledLayer.TryCreateFrom(nodesLayer, nodesObjectGroup, nodesImageLayer).ToArray();            
        }
    }
}
