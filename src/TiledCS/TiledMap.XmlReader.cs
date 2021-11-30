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
        /// Returns an empty instance of <see cref="TiledMap"/>
        /// </summary>
        public TiledMap() { }
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

            this.Tilesets = TiledTilesetSource.ReplaceTileSetFiles(this.Tilesets, new System.IO.FileInfo(path));
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
    }

    [System.Diagnostics.DebuggerDisplay("TileSet {GetDebuggerString(),nq}")]
    public class TiledTilesetSource : TiledTileset
    {
        /// <summary>
        /// The tsx file path as defined in the map file itself
        /// </summary>
        public string Source { get; set; }

        /// <inheritdoc />
        protected override void ParseXml(XmlNode node)
        {
            this.FirstGlobalId = node.Attributes.GetSignedOrDefault("firstgid", 0);
            this.Source = node.Attributes.GetStringOrDefault("source", null);

            // if source is not null, all other properties and fields should not be loaded now.
            if (!string.IsNullOrEmpty(this.Source)) return;

            base.ParseXml(node);
        }

        /// <summary>
        /// Checks if there's external TSX files remaining to be loaded.
        /// </summary>
        /// <param name="srcSets">The <see cref="TiledTileset"/> to check.</param>
        /// <param name="sourceFile">The path for the source TMX file.</param>
        /// <returns>A new <see cref="TiledTileset"/> array where all TSX have been loaded.</returns>
        public static TiledTileset[] ReplaceTileSetFiles(TiledTileset[] srcSets, System.IO.FileInfo sourceFile)
        {
            var directory = sourceFile.Directory;
            return srcSets.Select(item => ReplaceTileSetFile(item, directory)).ToArray();
        }

        private static TiledTileset ReplaceTileSetFile(TiledTileset src, System.IO.DirectoryInfo directory)
        {
            if (src is TiledTilesetSource tss && !string.IsNullOrEmpty(tss.Source))
            {
                var fpath = System.IO.Path.Combine(directory.FullName, tss.Source);

                var extSet = new TiledTileset(fpath);
                extSet.FirstGlobalId = tss.FirstGlobalId;

                return extSet;
            }

            return src;
        }

        /// <inheritdoc />
        protected override string GetDebuggerString()
        {
            return string.IsNullOrEmpty(Source)
                ? base.GetDebuggerString()
                : Source;
        }
    }
}
