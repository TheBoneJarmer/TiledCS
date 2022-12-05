using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled tileset
    /// </summary>
    public class TiledTileset
    {
        /// <summary>
        /// The Tiled version used to create this tileset
        /// </summary>
        public string TiledVersion { get; set; }
        /// <summary>
        /// The tileset name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The tileset class
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// The tile width in pixels
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// The tile height in pixels
        /// </summary>
        public int TileHeight { get; set; }
        /// <summary>
        /// The total amount of tiles
        /// </summary>
        public int TileCount { get; set; }
        /// <summary>
        /// The amount of horizontal tiles
        /// </summary>
        public int Columns { get; set; }
        /// <summary>
        /// The image definition used by the tileset
        /// </summary>
        public TiledImage Image { get; set; }
        /// <summary>
        /// The amount of spacing between the tiles in pixels
        /// </summary>
        public int Spacing { get; set; }
        /// <summary>
        /// The amount of margin between the tiles in pixels
        /// </summary>
        public int Margin { get; set; }
        /// <summary>
        /// An array of tile definitions
        /// </summary>
        /// <remarks>Not all tiles within a tileset have definitions. Only those with properties, animations, terrains, ...</remarks>
        public TiledTile[] Tiles { get; set; }
        /// <summary>
        /// An array of tileset properties
        /// </summary>
        public TiledProperty[] Properties { get; set; }
        
        /// <summary>
        /// The tile offset in pixels
        /// </summary>
        public TiledOffset Offset { get; set; }

        /// <summary>
        /// Returns an empty instance of TiledTileset
        /// </summary>
        public TiledTileset()
        {

        }

        /// <summary>
        /// Loads a tileset in TSX format and parses it
        /// </summary>
        /// <param name="path">The file path of the TSX file</param>
        /// <exception cref="TiledException">Thrown when the file could not be found or parsed</exception>
        public TiledTileset(string path)
        {
            // Check the file
            if (!File.Exists(path))
            {
                throw new TiledException($"{path} not found");
            }
            
            var content = File.ReadAllText(path);

            if (path.EndsWith(".tsx"))
            {
                ParseXml(content);
            }
            else
            {
                throw new TiledException("Unsupported file format");
            }
        }

        /// <summary>
        /// Loads a tileset in TSX format and parses it
        /// </summary>
        /// <param name="stream">The file stream of the TSX file</param>
        /// <exception cref="TiledException">Thrown when the file could not be parsed</exception>
        public TiledTileset(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();
            ParseXml(content);
        }

        /// <summary>
        /// Can be used to parse the content of a TSX tileset manually instead of loading it using the constructor
        /// </summary>
        /// <param name="xml">The tmx file content as string</param>
        /// <exception cref="TiledException"></exception>
        public void ParseXml(string xml)
        {
            try
            {
                var document = new XmlDocument();
                document.LoadXml(xml);

                var nodeTileset = document.SelectSingleNode("tileset");
                var nodeImage = nodeTileset.SelectSingleNode("image");
                var nodeOffset = nodeTileset.SelectSingleNode("tileoffset");
                var nodesTile = nodeTileset.SelectNodes("tile");
                var nodesProperty = nodeTileset.SelectNodes("properties/property");

                var attrMargin = nodeTileset.Attributes["margin"];
                var attrSpacing = nodeTileset.Attributes["spacing"];
                var attrClass = nodeTileset.Attributes["class"];

                TiledVersion = nodeTileset.Attributes["tiledversion"].Value;
                Name = nodeTileset.Attributes["name"]?.Value;
                TileWidth = int.Parse(nodeTileset.Attributes["tilewidth"].Value);
                TileHeight = int.Parse(nodeTileset.Attributes["tileheight"].Value);
                TileCount = int.Parse(nodeTileset.Attributes["tilecount"].Value);
                Columns = int.Parse(nodeTileset.Attributes["columns"].Value);

                if (attrMargin != null) Margin = int.Parse(nodeTileset.Attributes["margin"].Value);
                if (attrSpacing != null) Spacing = int.Parse(nodeTileset.Attributes["spacing"].Value);
                if (attrClass != null) Class = attrClass.Value;
                if (nodeImage != null) Image = ParseImage(nodeImage);
                if (nodeOffset != null) Offset = ParseOffset(nodeOffset);

                Tiles = ParseTiles(nodesTile);
                Properties = ParseProperties(nodesProperty);
            }
            catch (Exception ex)
            {
                throw new TiledException("An error occurred while trying to parse the Tiled tileset file", ex);
            }
        }

        private TiledOffset ParseOffset(XmlNode node)
        {
            var tiledOffset = new TiledOffset();
            tiledOffset.x = int.Parse(node.Attributes["x"].Value);
            tiledOffset.y = int.Parse(node.Attributes["y"].Value);

            return tiledOffset;
        }

        private TiledImage ParseImage(XmlNode node)
        {
            var tiledImage = new TiledImage();
            tiledImage.source = node.Attributes["source"].Value;
            tiledImage.width = int.Parse(node.Attributes["width"].Value);
            tiledImage.height = int.Parse(node.Attributes["height"].Value);

            return tiledImage;
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
                var attrType = node.Attributes["type"];

                var property = new TiledProperty();
                property.name = node.Attributes["name"].Value;
                property.value = node.Attributes["value"]?.Value;
                property.type = TiledPropertyType.String;

                if (attrType != null)
                {
                    if (attrType.Value == "bool") property.type = TiledPropertyType.Bool;
                    if (attrType.Value == "color") property.type = TiledPropertyType.Color;
                    if (attrType.Value == "file") property.type = TiledPropertyType.File;
                    if (attrType.Value == "float") property.type = TiledPropertyType.Float;
                    if (attrType.Value == "int") property.type = TiledPropertyType.Int;
                    if (attrType.Value == "object") property.type = TiledPropertyType.Object;
                }

                if (property.value == null)
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
                var nodesObject = node.SelectNodes("objectgroup/object");
                var nodesAnimation = node.SelectNodes("animation/frame");
                var nodeImage = node.SelectSingleNode("image");

                var tile = new TiledTile();
                tile.id = int.Parse(node.Attributes["id"].Value);
                tile.@class = node.Attributes["class"]?.Value;
                tile.type = node.Attributes["type"]?.Value;
                tile.terrain = node.Attributes["terrain"]?.Value.Split(',').AsIntArray();
                tile.properties = ParseProperties(nodesProperty);
                tile.animation = ParseAnimations(nodesAnimation);
                tile.objects = ParseObjects(nodesObject);

                if (nodeImage != null)
                {
                    var tileImage = new TiledImage();
                    tileImage.width = int.Parse(nodeImage.Attributes["width"].Value);
                    tileImage.height = int.Parse(nodeImage.Attributes["height"].Value);
                    tileImage.source = nodeImage.Attributes["source"].Value;

                    tile.image = tileImage;
                }

                result.Add(tile);
            }

            return result.ToArray();
        }
        
        private TiledObject[] ParseObjects(XmlNodeList nodeList)
        {
            var result = new List<TiledObject>();

            foreach (XmlNode node in nodeList)
            {
                var nodesProperty = node.SelectNodes("properties/property");
                var nodePolygon = node.SelectSingleNode("polygon");
                var nodePoint = node.SelectSingleNode("point");
                var nodeEllipse = node.SelectSingleNode("ellipse");

                var obj = new TiledObject();
                obj.id = int.Parse(node.Attributes["id"].Value);
                obj.name = node.Attributes["name"]?.Value;
                obj.@class = node.Attributes["class"]?.Value;
                obj.type = node.Attributes["type"]?.Value;
                obj.gid = int.Parse(node.Attributes["gid"]?.Value ?? "0");
                obj.x = float.Parse(node.Attributes["x"].Value, CultureInfo.InvariantCulture);
                obj.y = float.Parse(node.Attributes["y"].Value, CultureInfo.InvariantCulture);

                if (nodesProperty != null)
                {
                    obj.properties = ParseProperties(nodesProperty);
                }

                if (nodePolygon != null)
                {
                    var points = nodePolygon.Attributes["points"].Value;
                    var vertices = points.Split(' ');

                    var polygon = new TiledPolygon();
                    polygon.points = new float[vertices.Length * 2];

                    for (var i = 0; i < vertices.Length; i++)
                    {
                        polygon.points[(i * 2) + 0] = float.Parse(vertices[i].Split(',')[0], CultureInfo.InvariantCulture);
                        polygon.points[(i * 2) + 1] = float.Parse(vertices[i].Split(',')[1], CultureInfo.InvariantCulture);
                    }

                    obj.polygon = polygon;
                }

                if (nodeEllipse != null)
                {
                    obj.ellipse = new TiledEllipse();
                }

                if (nodePoint != null)
                {
                    obj.point = new TiledPoint();
                }

                if (node.Attributes["width"] != null)
                {
                    obj.width = float.Parse(node.Attributes["width"].Value, CultureInfo.InvariantCulture);
                }

                if (node.Attributes["height"] != null)
                {
                    obj.height = float.Parse(node.Attributes["height"].Value, CultureInfo.InvariantCulture);
                }

                if (node.Attributes["rotation"] != null)
                {
                    obj.rotation = float.Parse(node.Attributes["rotation"].Value, CultureInfo.InvariantCulture);
                }

                result.Add(obj);
            }

            return result.ToArray();
        }
    }
}