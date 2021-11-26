using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled map
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("TiledMap {Infinite} {Width}x{Height} {TileWidth}x{TileHeight}")]
    public partial class TiledMap
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;

        /// <summary>
        /// How many times we shift the FLIPPED flags to the right in order to store it in a byte.
        /// For example: 0b10100000000000000000000000000000 >> SHIFT_FLIP_FLAG_TO_BYTE = 0b00000101
        /// </summary>
        const int SHIFT_FLIP_FLAG_TO_BYTE = 29;

        /// <summary>
        /// Returns the Tiled version used to create this map
        /// </summary>
        public string TiledVersion { get; set; }
        /// <summary>
        /// Returns an array of properties defined in the map
        /// </summary>
        public TiledProperty[] Properties { get; set; }
        /// <summary>
        /// Returns an array of tileset definitions in the map
        /// </summary>
        public TiledMapTileset[] Tilesets { get; set; }
        /// <summary>
        /// Returns an array of layers or null if none were defined
        /// </summary>
        public TiledLayer[] Layers { get; set; }
        /// <summary>
        /// Returns an array of groups or null if none were defined
        /// </summary>
        public TiledGroup[] Groups { get; set; }
        /// <summary>
        /// Returns the defined map orientation as a string
        /// </summary>
        public string Orientation { get; set; }
        /// <summary>
        /// Returns the render order as a string
        /// </summary>
        public string RenderOrder { get; set; }
        /// <summary>
        /// The amount of horizontal tiles
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// The amount of vertical tiles
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// The tile width in pixels
        /// </summary>
        public int TileWidth { get; set; }
        /// <summary>
        /// The tile height in pixels
        /// </summary>
        public int TileHeight { get; set; }
        /// <summary>
        /// Returns true if the map is configured as infinite
        /// </summary>
        public bool Infinite { get; set; }
        /// <summary>
        /// Returns the defined map background color as a hex string
        /// </summary>
        public string BackgroundColor { get; set; }
        /// <summary>
        /// Returns an empty instance of TiledMap
        /// </summary>
        public TiledMap()
        {

        }

        /// <summary>
        /// Loads a Tiled map in TMX format and parses it
        /// </summary>
        /// <param name="path">The path to the tmx file</param>
        /// <exception cref="TiledException">Thrown when the map could not be loaded or is not in a correct format</exception>
        public TiledMap(string path)
        {
            string content;

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
                ParseXml(content);
            }
            else
            {
                throw new TiledException("Unsupported file format");
            }
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

                var nodeMap = document.SelectSingleNode("map");
                var nodesProperty = nodeMap.SelectNodes("properties/property");
                var nodesLayer = nodeMap.SelectNodes("layer");
                var nodesImageLayer = nodeMap.SelectNodes("imagelayer");
                var nodesObjectGroup = nodeMap.SelectNodes("objectgroup");
                var nodesTileset = nodeMap.SelectNodes("tileset");
                var nodesGroup = nodeMap.SelectNodes("group");

                var version = nodeMap.Attributes.GetStringOrDefault("version", "Unknown");
                this.TiledVersion = nodeMap.Attributes.GetStringOrDefault("tiledversion", version);

                this.Orientation = nodeMap.Attributes["orientation"].Value;
                this.RenderOrder = nodeMap.Attributes.GetStringOrDefault("renderorder", string.Empty);
                this.BackgroundColor = nodeMap.Attributes.GetStringOrDefault("backgroundcolor", null);
                this.Infinite = nodeMap.Attributes.GetStringOrDefault("infinite","0") == "1";

                this.Width = int.Parse(nodeMap.Attributes["width"].Value);
                this.Height = int.Parse(nodeMap.Attributes["height"].Value);
                this.TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
                this.TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

                if (nodesProperty != null) Properties = ParseProperties(nodesProperty);
                if (nodesTileset != null) Tilesets = ParseTilesets(nodesTileset);
                if (nodesLayer != null) Layers = ParseLayers(nodesLayer, nodesObjectGroup, nodesImageLayer);
                if (nodesGroup != null) Groups = ParseGroups(nodesGroup);            

            #if !DEBUG
            }
            catch (Exception ex)
            {
                throw new TiledException("Unable to parse xml data, make sure the xml data represents a valid Tiled map", ex);
            }
            #endif
        }

        private static TiledProperty[] ParseProperties(XmlNodeList nodeList)
        {
            var result = new List<TiledProperty>();

            foreach (XmlNode node in nodeList)
            {
                var property = new TiledProperty();
                property.name = node.Attributes["name"].Value;
                property.type = node.Attributes.GetStringOrDefault("type", null);
                property.value = node.Attributes.GetStringOrDefault("value", null);

                if (property.value == null && node.InnerText != null)
                {
                    property.value = node.InnerText;
                }

                result.Add(property);
            }

            return result.ToArray();
        }

        private static TiledMapTileset[] ParseTilesets(XmlNodeList nodeList)
        {
            var result = new List<TiledMapTileset>();

            foreach (XmlNode node in nodeList)
            {
                var tileset = new TiledMapTileset();
                tileset.firstgid = int.Parse(node.Attributes["firstgid"].Value);
                tileset.source = node.Attributes["source"].Value;

                result.Add(tileset);
            }

            return result.ToArray();
        }

        private static TiledGroup[] ParseGroups(XmlNodeList nodeListGroups)
        {
            var result = new List<TiledGroup>();

            foreach (XmlNode node in nodeListGroups)
            {
                var nodesProperty = node.SelectNodes("properties/property");
                var nodesGroup = node.SelectNodes("group");
                var nodesLayer = node.SelectNodes("layer");
                var nodesObjectGroup = node.SelectNodes("objectgroup");
                var nodesImageLayer = node.SelectNodes("imagelayer");
                var attrVisible = node.Attributes["visible"];
                var attrLocked = node.Attributes["locked"];

                var tiledGroup = new TiledGroup();
                tiledGroup.id = int.Parse(node.Attributes["id"].Value);
                tiledGroup.name = node.Attributes["name"].Value;
                
                if (attrVisible != null) tiledGroup.visible = attrVisible.Value == "1";
                if (attrLocked != null) tiledGroup.locked = attrLocked.Value == "1";
                if (nodesProperty != null) tiledGroup.properties = ParseProperties(nodesProperty);
                if (nodesGroup != null) tiledGroup.groups = ParseGroups(nodesGroup);
                if (nodesLayer != null) tiledGroup.layers = ParseLayers(nodesLayer, nodesObjectGroup, nodesImageLayer);
                
                result.Add(tiledGroup);
            }
            
            return result.ToArray();
        }
        private static TiledLayer[] ParseLayers(XmlNodeList nodesLayer, XmlNodeList nodesObjectGroup, XmlNodeList nodesImageLayer)
        {
            var result = new List<TiledLayer>();

            foreach (XmlNode node in nodesLayer)
            {
                var tiledLayer = new TiledLayer();
                tiledLayer.type = "tilelayer";
                ParseLayerDefault(tiledLayer, node);

                var nodeData = node.SelectSingleNode("data");
                string encoding = nodeData.Attributes.GetStringOrDefault("encoding", null);

                if (encoding == "csv")
                {
                    var csvs = nodeData.InnerText.Split(',');

                    tiledLayer.data = new int[csvs.Length];
                    tiledLayer.dataRotationFlags = new byte[csvs.Length];

                    // Parse the comma separated csv string and update the inner data as well as the data rotation flags
                    for (var i = 0; i < csvs.Length; i++)
                    {
                        var rawID = uint.Parse(csvs[i]);
                        var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                        var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                        var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
                        tiledLayer.dataRotationFlags[i] = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);

                        // assign data to rawID with the rotation flags cleared
                        tiledLayer.data[i] = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
                    }
                }
                else if (encoding == "base64")
                {
                    var compression = nodeData.Attributes.GetStringOrDefault("compression", null);

                    if (compression == null)
                    {
                        using (var base64DataStream = nodeData.InnerText.OpenReadBase64())
                        {
                            // Parse the decoded bytes and update the inner data as well as the data rotation flags
                            var rawBytes = new byte[4];
                            tiledLayer.data = new int[base64DataStream.Length];
                            tiledLayer.dataRotationFlags = new byte[base64DataStream.Length];

                            for (var i = 0; i < base64DataStream.Length; i++)
                            {
                                base64DataStream.Read(rawBytes, 0, rawBytes.Length);
                                var rawID = BitConverter.ToUInt32(rawBytes, 0);
                                var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                                var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                                var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
                                tiledLayer.dataRotationFlags[i] = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);

                                // assign data to rawID with the rotation flags cleared
                                tiledLayer.data[i] = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
                            }
                        }
                    }
                    else if (compression == "zlib")
                    {
                        using (var base64DataStream = nodeData.InnerText.OpenReadBase64())
                        {
                            // .NET doesn't play well with the headered zlib data that Tiled produces,
                            // so we have to manually skip the 2-byte header to get what DeflateStream's looking for
                            // Should an external library be used instead of this hack?
                            base64DataStream.ReadByte();
                            base64DataStream.ReadByte();

                            using (var decompressionStream = new DeflateStream(base64DataStream, CompressionMode.Decompress))
                            {
                                // Parse the raw decompressed bytes and update the inner data as well as the data rotation flags
                                var decompressedDataBuffer = new byte[4]; // size of each tile
                                var dataRotationFlagsList = new List<byte>();
                                var layerDataList = new List<int>();

                                while (decompressionStream.Read(decompressedDataBuffer, 0, decompressedDataBuffer.Length) == decompressedDataBuffer.Length)
                                {
                                    var rawID = BitConverter.ToUInt32(decompressedDataBuffer, 0);
                                    var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                                    var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                                    var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
                                    dataRotationFlagsList.Add((byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE));

                                    // assign data to rawID with the rotation flags cleared
                                    layerDataList.Add((int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG)));
                                }

                                tiledLayer.data = layerDataList.ToArray();
                                tiledLayer.dataRotationFlags = dataRotationFlagsList.ToArray();
                            }
                        }
                    }
                    else if (compression == "gzip")
                    {
                        using (var decompressionStream = new GZipStream(nodeData.InnerText.OpenReadBase64(), CompressionMode.Decompress))
                        {
                            // Parse the raw decompressed bytes and update the inner data as well as the data rotation flags
                            var decompressedDataBuffer = new byte[4]; // size of each tile
                            var dataRotationFlagsList = new List<byte>();
                            var layerDataList = new List<int>();

                            while (decompressionStream.Read(decompressedDataBuffer, 0, decompressedDataBuffer.Length) == decompressedDataBuffer.Length)
                            {
                                var rawID = BitConverter.ToUInt32(decompressedDataBuffer, 0);
                                var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                                var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                                var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));

                                dataRotationFlagsList.Add((byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE));

                                // assign data to rawID with the rotation flags cleared
                                layerDataList.Add((int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG)));
                            }

                            tiledLayer.data = layerDataList.ToArray();
                            tiledLayer.dataRotationFlags = dataRotationFlagsList.ToArray();
                        }
                    }
                    else
                    {
                        throw new TiledException($"{compression} compression is currently not supported");
                    }

                }
                else
                {
                    throw new TiledException("Only CSV and Base64 encodings are currently supported");
                }

                result.Add(tiledLayer);
            }

            foreach (XmlNode node in nodesObjectGroup)
            {
                var tiledLayer = new TiledLayer();
                tiledLayer.type = "objectgroup";
                ParseLayerDefault(tiledLayer, node);
                
                var nodesObject = node.SelectNodes("object");
                tiledLayer.objects = ParseObjects(nodesObject);

                result.Add(tiledLayer);
            }

            foreach (XmlNode node in nodesImageLayer)
            {
                var tiledLayer = new TiledLayer();
                tiledLayer.type = "imagelayer";
                ParseLayerDefault(tiledLayer, node);
                
                var nodeImage = node.SelectSingleNode("image");                
                if (nodeImage != null) tiledLayer.image = ParseImage(nodeImage);
                
                result.Add(tiledLayer);
            }

            return result.ToArray();
        }

        private static void ParseLayerDefault(TiledLayer layer, XmlNode node)
        {
            layer.id = node.Attributes.GetIntegerOrDefault("id", 0);
            layer.name = node.Attributes.GetStringOrDefault("name", string.Empty);
            layer.visible = node.Attributes.GetIntegerOrDefault("visible", 1) == 1;
            layer.locked = node.Attributes.GetIntegerOrDefault("locked", 0) == 1;
            layer.offsetX = node.Attributes.GetIntegerOrDefault("offsetx", 0);
            layer.offsetY = node.Attributes.GetIntegerOrDefault("offsety", 0);
            layer.tintcolor = node.Attributes.GetStringOrDefault("tintcolor", null);

            var nodesProperty = node.SelectNodes("properties/property");
            if (nodesProperty != null) layer.properties = ParseProperties(nodesProperty);
        }
        
        private static TiledImage ParseImage(XmlNode node)
        {
            var tiledImage = new TiledImage();
            tiledImage.source = node.Attributes["source"].Value;
            tiledImage.width = int.Parse(node.Attributes["width"].Value);
            tiledImage.height = int.Parse(node.Attributes["height"].Value);

            return tiledImage;
        }

        private static TiledObject[] ParseObjects(XmlNodeList nodeList)
        {
            var result = new List<TiledObject>();

            foreach (XmlNode node in nodeList)
            {
                var nodesProperty = node.SelectNodes("properties/property");
                var nodePolygon = node.SelectSingleNode("polygon");
                var nodePoint = node.SelectSingleNode("point");
                var nodeEllipse = node.SelectSingleNode("ellipse");

                var obj = new TiledObject();
                obj.id = node.Attributes.GetIntegerOrDefault("id", 0);
                obj.name = node.Attributes.GetStringOrDefault("name", null);
                obj.type = node.Attributes.GetStringOrDefault("type", null);
                obj.gid = node.Attributes.GetIntegerOrDefault("gid", 0);
                obj.x = node.Attributes.GetSingleOrDefault("x", 0);
                obj.y = node.Attributes.GetSingleOrDefault("y", 0);
                obj.width = node.Attributes.GetSingleOrDefault("width", 0);
                obj.height = node.Attributes.GetSingleOrDefault("height", 0);
                obj.rotation = node.Attributes.GetIntegerOrDefault("rotation", 0);

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

                result.Add(obj);
            }

            return result.ToArray();
        }        
    }
}