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
    public class TiledMap
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0b10000000000000000000000000000000;
        const uint FLIPPED_VERTICALLY_FLAG = 0b01000000000000000000000000000000;
        const uint FLIPPED_DIAGONALLY_FLAG = 0b00100000000000000000000000000000;

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
        /// The parallax origin x
        /// </summary>
        public float ParallaxOriginX { get; set; }

        /// <summary>
        /// The parallax origin y
        /// </summary>
        public float ParallaxOriginY { get; set; }

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
            // Check the file
            if (!File.Exists(path))
            {
                throw new TiledException($"{path} not found");
            }

            var content = File.ReadAllText(path);

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
        /// Loads a Tiled map in TMX format and parses it
        /// </summary>
        /// <param name="stream">Stream of opened tmx file</param>
        /// <exception cref="TiledException">Thrown when the map could not be loaded</exception>
        public TiledMap(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();
            ParseXml(content);
        }

        /// <summary>
        /// Can be used to parse the content of a TMX map manually instead of loading it using the constructor
        /// </summary>
        /// <param name="xml">The tmx file content as string</param>
        /// <exception cref="TiledException"></exception>
        public void ParseXml(string xml)
        {
            try
            {
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
                var attrParallaxOriginX = nodeMap.Attributes["parallaxoriginx"];
                var attrParallaxOriginY = nodeMap.Attributes["parallaxoriginy"];

                TiledVersion = nodeMap.Attributes["tiledversion"].Value;
                Orientation = nodeMap.Attributes["orientation"].Value;
                RenderOrder = nodeMap.Attributes["renderorder"].Value;
                BackgroundColor = nodeMap.Attributes["backgroundcolor"]?.Value;
                Infinite = nodeMap.Attributes["infinite"].Value == "1";

                Width = int.Parse(nodeMap.Attributes["width"].Value);
                Height = int.Parse(nodeMap.Attributes["height"].Value);
                TileWidth = int.Parse(nodeMap.Attributes["tilewidth"].Value);
                TileHeight = int.Parse(nodeMap.Attributes["tileheight"].Value);

                if (nodesProperty != null) Properties = ParseProperties(nodesProperty);
                if (nodesTileset != null) Tilesets = ParseTilesets(nodesTileset);
                if (nodesLayer != null) Layers = ParseLayers(nodesLayer, nodesObjectGroup, nodesImageLayer);
                if (nodesGroup != null) Groups = ParseGroups(nodesGroup);
                if (attrParallaxOriginX != null) ParallaxOriginX = float.Parse(attrParallaxOriginX.Value, CultureInfo.InvariantCulture);
                if (attrParallaxOriginY != null) ParallaxOriginY = float.Parse(attrParallaxOriginY.Value, CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                throw new TiledException("An error occurred while trying to parse the Tiled map file", ex);
            }
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

        private TiledMapTileset[] ParseTilesets(XmlNodeList nodeList)
        {
            var result = new List<TiledMapTileset>();

            foreach (XmlNode node in nodeList)
            {
                var tileset = new TiledMapTileset();
                tileset.firstgid = int.Parse(node.Attributes["firstgid"].Value);
                tileset.source = node.Attributes["source"]?.Value;

                result.Add(tileset);
            }

            return result.ToArray();
        }

        private TiledGroup[] ParseGroups(XmlNodeList nodeListGroups)
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

        private TiledLayer[] ParseLayers(XmlNodeList nodesLayer, XmlNodeList nodesObjectGroup, XmlNodeList nodesImageLayer)
        {
            var result = new List<TiledLayer>();

            foreach (XmlNode node in nodesLayer)
            {
                result.Add(ParseLayer(node, TiledLayerType.TileLayer));
            }

            foreach (XmlNode node in nodesObjectGroup)
            {
                result.Add(ParseLayer(node, TiledLayerType.ObjectLayer));
            }

            foreach (XmlNode node in nodesImageLayer)
            {
                result.Add(ParseLayer(node, TiledLayerType.ImageLayer));
            }

            return result.ToArray();
        }

        private TiledLayer ParseLayer(XmlNode node, TiledLayerType type)
        {
            var nodesProperty = node.SelectNodes("properties/property");
            var attrVisible = node.Attributes["visible"];
            var attrLocked = node.Attributes["locked"];
            var attrTint = node.Attributes["tintcolor"];
            var attrOffsetX = node.Attributes["offsetx"];
            var attrOffsetY = node.Attributes["offsety"];
            var attrParallaxX = node.Attributes["parallaxx"];
            var attrParallaxY = node.Attributes["parallaxy"];
            var attrOpacity = node.Attributes["opacity"];
            var attrClass = node.Attributes["class"];
            var attrWidth = node.Attributes["width"];
            var attrHeight = node.Attributes["height"];

            var tiledLayer = new TiledLayer();
            tiledLayer.id = int.Parse(node.Attributes["id"].Value);
            tiledLayer.type = type;
            tiledLayer.name = node.Attributes["name"].Value;
            tiledLayer.visible = true;
            tiledLayer.opacity = 1.0f;
            tiledLayer.parallaxX = 1.0f;
            tiledLayer.parallaxY = 1.0f;

            if (attrWidth != null) tiledLayer.width = int.Parse(attrWidth.Value);
            if (attrHeight != null) tiledLayer.height = int.Parse(attrHeight.Value);
            if (attrVisible != null) tiledLayer.visible = attrVisible.Value == "1";
            if (attrLocked != null) tiledLayer.locked = attrLocked.Value == "1";
            if (attrTint != null) tiledLayer.tintcolor = attrTint.Value;
            if (attrClass != null) tiledLayer.@class = attrClass.Value;
            if (attrOpacity != null) tiledLayer.opacity = float.Parse(attrOpacity.Value, CultureInfo.InvariantCulture);
            if (attrOffsetX != null) tiledLayer.offsetX = float.Parse(attrOffsetX.Value, CultureInfo.InvariantCulture);
            if (attrOffsetY != null) tiledLayer.offsetY = float.Parse(attrOffsetY.Value, CultureInfo.InvariantCulture);
            if (attrParallaxX != null) tiledLayer.parallaxX = float.Parse(attrParallaxX.Value, CultureInfo.InvariantCulture);
            if (attrParallaxY != null) tiledLayer.parallaxY = float.Parse(attrParallaxY.Value, CultureInfo.InvariantCulture);
            if (nodesProperty != null) tiledLayer.properties = ParseProperties(nodesProperty);

            if (type == TiledLayerType.TileLayer)
            {
                var nodeData = node.SelectSingleNode("data");
                
                ParseTileLayerData(nodeData, ref tiledLayer);
            }

            if (type == TiledLayerType.ObjectLayer)
            {
                var nodesObject = node.SelectNodes("object");
                
                tiledLayer.objects = ParseObjects(nodesObject);
            }

            if (type == TiledLayerType.ImageLayer)
            {
                var nodeImage = node.SelectSingleNode("image");
                
                if (nodeImage != null) tiledLayer.image = ParseImage(nodeImage);
            }

            return tiledLayer;
        }

        private void ParseTileLayerData(XmlNode nodeData, ref TiledLayer tiledLayer)
        {
            var encoding = nodeData.Attributes["encoding"].Value;
            var compression = nodeData.Attributes["compression"]?.Value;

            if (encoding != "csv" && encoding != "base64")
            {
                throw new TiledException("Only CSV and Base64 encodings are currently supported");
            }

            if (Infinite)
            {
                var nodesChunk = nodeData.SelectNodes("chunk");
                var chunks = new List<TiledChunk>();

                foreach (XmlNode nodeChunk in nodesChunk)
                {
                    var chunk = new TiledChunk();
                    chunk.x = int.Parse(nodeChunk.Attributes["x"].Value);
                    chunk.y = int.Parse(nodeChunk.Attributes["y"].Value);
                    chunk.width = int.Parse(nodeChunk.Attributes["width"].Value);
                    chunk.height = int.Parse(nodeChunk.Attributes["height"].Value);

                    if (encoding == "csv") ParseTileLayerDataAsCSV(nodeChunk.InnerText, ref chunk.data, ref chunk.dataRotationFlags);
                    if (encoding == "base64") ParseTileLayerDataAsBase64(nodeChunk.InnerText, compression, ref chunk.data, ref chunk.dataRotationFlags);

                    chunks.Add(chunk);
                }

                tiledLayer.chunks = chunks.ToArray();
            }
            else
            {
                if (encoding == "csv") ParseTileLayerDataAsCSV(nodeData.InnerText, ref tiledLayer.data, ref tiledLayer.dataRotationFlags);
                if (encoding == "base64") ParseTileLayerDataAsBase64(nodeData.InnerText, compression, ref tiledLayer.data, ref tiledLayer.dataRotationFlags);
            }
        }

        private void ParseTileLayerDataAsBase64(string input, string compression, ref int[] data, ref byte[] dataRotationFlags)
        {
            using (var base64DataStream = new MemoryStream(Convert.FromBase64String(input)))
            {
                if (compression == null)
                {
                    // Parse the decoded bytes and update the inner data as well as the data rotation flags
                    var rawBytes = new byte[4];
                    data = new int[base64DataStream.Length];
                    dataRotationFlags = new byte[base64DataStream.Length];

                    for (var i = 0; i < base64DataStream.Length; i++)
                    {
                        base64DataStream.Read(rawBytes, 0, rawBytes.Length);
                        var rawID = BitConverter.ToUInt32(rawBytes, 0);
                        var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                        var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                        var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
                        dataRotationFlags[i] = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);

                        // assign data to rawID with the rotation flags cleared
                        data[i] = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
                    }
                }
                else if (compression == "zlib")
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

                        data = layerDataList.ToArray();
                        dataRotationFlags = dataRotationFlagsList.ToArray();
                    }
                }
                else if (compression == "gzip")
                {
                    using (var decompressionStream = new GZipStream(base64DataStream, CompressionMode.Decompress))
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

                        data = layerDataList.ToArray();
                        dataRotationFlags = dataRotationFlagsList.ToArray();
                    }
                }
                else
                {
                    throw new TiledException("Zstandard compression is currently not supported");
                }
            }
        }

        private void ParseTileLayerDataAsCSV(string input, ref int[] data, ref byte[] dataRotationFlags)
        {
            var csvs = input.Split(',');

            data = new int[csvs.Length];
            dataRotationFlags = new byte[csvs.Length];

            // Parse the comma separated csv string and update the inner data as well as the data rotation flags
            for (var i = 0; i < csvs.Length; i++)
            {
                var rawID = uint.Parse(csvs[i]);
                var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
                dataRotationFlags[i] = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);

                // assign data to rawID with the rotation flags cleared
                data[i] = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
            }
        }

        private TiledImage ParseImage(XmlNode node)
        {
            var tiledImage = new TiledImage();
            tiledImage.source = node.Attributes["source"].Value;
            tiledImage.width = int.Parse(node.Attributes["width"].Value);
            tiledImage.height = int.Parse(node.Attributes["height"].Value);

            return tiledImage;
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
                var attrGid = node.Attributes["gid"];

                var obj = new TiledObject();
                obj.id = int.Parse(node.Attributes["id"].Value);
                obj.name = node.Attributes["name"]?.Value;
                obj.@class = node.Attributes["class"]?.Value;
                obj.type = node.Attributes["type"]?.Value;
                obj.x = float.Parse(node.Attributes["x"].Value, CultureInfo.InvariantCulture);
                obj.y = float.Parse(node.Attributes["y"].Value, CultureInfo.InvariantCulture);

                if (attrGid != null)
                {
                    ParseObjectGid(ref obj, attrGid.Value);
                }

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

        private void ParseObjectGid(ref TiledObject tiledObject, String gid)
        {
            var rawID = uint.Parse(gid);
            var hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
            var ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
            var dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
            
            tiledObject.dataRotationFlag = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);
            tiledObject.gid = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
        }

        /* HELPER METHODS */
        /// <summary>
        /// Locates the right TiledMapTileset object for you within the Tilesets array
        /// </summary>
        /// <param name="gid">A value from the TiledLayer.data array</param>
        /// <returns>An element within the Tilesets array or null if no match was found</returns>
        public TiledMapTileset GetTiledMapTileset(int gid)
        {
            if (Tilesets == null)
            {
                return null;
            }

            for (var i = 0; i < Tilesets.Length; i++)
            {
                if (i < Tilesets.Length - 1)
                {
                    int gid1 = Tilesets[i + 0].firstgid;
                    int gid2 = Tilesets[i + 1].firstgid;

                    if (gid >= gid1 && gid < gid2)
                    {
                        return Tilesets[i];
                    }
                }
                else
                {
                    return Tilesets[i];
                }
            }

            return new TiledMapTileset();
        }

        /// <summary>
        /// Loads external tilesets and matches them to firstGids from elements within the Tilesets array
        /// </summary>
        /// <param name="src">The folder where the TiledMap file is located</param>
        /// <returns>A dictionary where the key represents the firstGid of the associated TiledMapTileset and the value the TiledTileset object</returns>
        public Dictionary<int, TiledTileset> GetTiledTilesets(string src)
        {
            var tilesets = new Dictionary<int, TiledTileset>();
            var info = new FileInfo(src);
            var srcFolder = info.Directory;

            if (Tilesets == null)
            {
                return tilesets;
            }

            foreach (var mapTileset in Tilesets)
            {
                var path = $"{srcFolder}/{mapTileset.source}";

                if (mapTileset.source == null)
                {
                    continue;
                }

                if (File.Exists(path))
                {
                    tilesets.Add(mapTileset.firstgid, new TiledTileset(path));
                }
                else
                {
                    throw new TiledException("Cannot locate tileset '" + path + "'. Please make sure the source folder is correct and it ends with a slash.");
                }
            }

            return tilesets;
        }

        /// <summary>
        /// Locates a specific TiledTile object
        /// </summary>
        /// <param name="mapTileset">An element within the Tilesets array</param>
        /// <param name="tileset">An instance of the TiledTileset class</param>
        /// <param name="gid">An element from within a TiledLayer.data array</param>
        /// <returns>An entry of the TiledTileset.tiles array or null if none of the tile id's matches the gid</returns>
        /// <remarks>Tip: Use the GetTiledMapTileset and GetTiledTilesets methods for retrieving the correct TiledMapTileset and TiledTileset objects</remarks>
        public TiledTile GetTiledTile(TiledMapTileset mapTileset, TiledTileset tileset, int gid)
        {
            foreach (var tile in tileset.Tiles)
            {
                if (tile.id == gid - mapTileset.firstgid)
                {
                    return tile;
                }
            }

            return null;
        }

        /// <summary>
        /// This method can be used to figure out the source rect on a Tileset image for rendering tiles.
        /// </summary>
        /// <param name="mapTileset"></param>
        /// <param name="tileset"></param>
        /// <param name="gid"></param>
        /// <returns>An instance of the class TiledSourceRect that represents a rectangle. Returns null if the provided gid was not found within the tileset.</returns>
        public TiledSourceRect GetSourceRect(TiledMapTileset mapTileset, TiledTileset tileset, int gid)
        {
            var tileHor = 0;
            var tileVert = 0;

            for (var i = 0; i < tileset.TileCount; i++)
            {
                if (i == gid - mapTileset.firstgid)
                {
                    var result = new TiledSourceRect();
                    result.x = tileHor * tileset.TileWidth;
                    result.y = tileVert * tileset.TileHeight;
                    result.width = tileset.TileWidth;
                    result.height = tileset.TileHeight;

                    return result;
                }

                // Update x and y position
                tileHor++;

                if (tileHor == tileset.Image.width / tileset.TileWidth)
                {
                    tileHor = 0;
                    tileVert++;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedHorizontal(TiledLayer layer, int tileHor, int tileVert)
        {
            if (layer.type != TiledLayerType.TileLayer)
            {
                throw new TiledException("Retrieving tile flipped state for a tile does not work for non-tile layers");
            }
            
            return IsTileFlippedHorizontal(layer, tileHor + (tileVert * layer.width));
        }

        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedHorizontal(TiledLayer layer, int dataIndex)
        {
            return (layer.dataRotationFlags[dataIndex] & (FLIPPED_HORIZONTALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }
        
        /// <summary>
        /// Checks is a tile linked to an object is flipped horizontally
        /// </summary>
        /// <param name="tiledObject">The tiled object</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedHorizontal(TiledObject tiledObject)
        {
            if (tiledObject.gid == 0)
            {
                throw new TiledException("Tiled object not linked to a tile");
            }
            
            return (tiledObject.dataRotationFlag & (FLIPPED_HORIZONTALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }

        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        public bool IsTileFlippedVertical(TiledLayer layer, int tileHor, int tileVert)
        {
            if (layer.type != TiledLayerType.TileLayer)
            {
                throw new TiledException("Retrieving tile flipped state for a tile does not work for non-tile layers");
            }
            
            return IsTileFlippedVertical(layer, tileHor + (tileVert * layer.width));
        }

        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        public bool IsTileFlippedVertical(TiledLayer layer, int dataIndex)
        {
            return (layer.dataRotationFlags[dataIndex] & (FLIPPED_VERTICALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }
        
        /// <summary>
        /// Checks is a tile linked to an object is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tiledObject">The tiled object</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedVertical(TiledObject tiledObject)
        {
            if (tiledObject.gid == 0)
            {
                throw new TiledException("Tiled object not linked to a tile");
            }
            
            return (tiledObject.dataRotationFlag & (FLIPPED_VERTICALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }

        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public bool IsTileFlippedDiagonal(TiledLayer layer, int tileHor, int tileVert)
        {
            if (layer.type != TiledLayerType.TileLayer)
            {
                throw new TiledException("Retrieving tile flipped state for a tile does not work for non-tile layers");
            }
            
            return IsTileFlippedDiagonal(layer, tileHor + (tileVert * layer.width));
        }

        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public bool IsTileFlippedDiagonal(TiledLayer layer, int dataIndex)
        {
            return (layer.dataRotationFlags[dataIndex] & (FLIPPED_DIAGONALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }
        
        /// <summary>
        /// Checks is a tile linked to an object is flipped diagonally
        /// </summary>
        /// <param name="tiledObject">The tiled object</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedDiagonal(TiledObject tiledObject)
        {
            if (tiledObject.gid == 0)
            {
                throw new TiledException("Tiled object not linked to a tile");
            }
            
            return (tiledObject.dataRotationFlag & (FLIPPED_DIAGONALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }
    }
}
