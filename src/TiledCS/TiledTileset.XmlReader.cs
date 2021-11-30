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
        /// Returns an empty instance of <see cref="TiledTileset"/>
        /// </summary>
        public TiledTileset() { }

        /// <summary>
        /// Loads a tileset in TSX format and parses it
        /// </summary>
        /// <param name="path">The file path of the TSX file</param>
        /// <exception cref="TiledException">Thrown when the file could not be found or parsed</exception>
        public TiledTileset(string path)
        {
            // Check the file
            if (!File.Exists(path)) throw new TiledException($"{path} not found");
            if (!path.ToLowerInvariant().EndsWith(".tsx")) throw new TiledException("Unsupported file format");
            
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
    }
}
