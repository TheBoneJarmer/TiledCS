using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace TiledCS
{
    /// <summary>
    /// Represents a Tiled template
    /// </summary>
    public class TiledTemplate
    {
        const uint FLIPPED_HORIZONTALLY_FLAG = 0b10000000000000000000000000000000;
        const uint FLIPPED_VERTICALLY_FLAG   = 0b01000000000000000000000000000000;
        const uint FLIPPED_DIAGONALLY_FLAG   = 0b00100000000000000000000000000000;
        const int  SHIFT_FLIP_FLAG_TO_BYTE   = 29;

        /// <summary>
        /// The tileset referenced by the template, if any
        /// </summary>
        public TiledMapTileset Tileset { get; set; }

        /// <summary>
        /// The base object defined in the template
        /// </summary>
        public TiledObject Object { get; set; }

        /// <summary>
        /// Returns an empty instance of TiledTemplate
        /// </summary>
        public TiledTemplate()
        {
        }

        /// <summary>
        /// Loads a Tiled template in TX format and parses it
        /// </summary>
        /// <param name="path">The path to the tx file</param>
        /// <exception cref="TiledException">Thrown when the file could not be found or parsed</exception>
        public TiledTemplate(string path)
        {
            if (!File.Exists(path))
            {
                throw new TiledException($"{path} not found");
            }

            var content = File.ReadAllText(path);

            if (path.EndsWith(".tx"))
            {
                ParseXml(content);
            }
            else
            {
                throw new TiledException("Unsupported file format");
            }
        }

        /// <summary>
        /// Loads a Tiled template in TX format and parses it
        /// </summary>
        /// <param name="stream">Stream of opened tx file</param>
        /// <exception cref="TiledException">Thrown when the template could not be loaded</exception>
        public TiledTemplate(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var content = streamReader.ReadToEnd();
            ParseXml(content);
        }

        /// <summary>
        /// Can be used to parse the content of a TX template manually instead of loading it using the constructor
        /// </summary>
        /// <param name="xml">The tx file content as string</param>
        /// <exception cref="TiledException"></exception>
        public void ParseXml(string xml)
        {
            try
            {
                var document = new XmlDocument();
                document.LoadXml(xml);

                var nodeTemplate = document.SelectSingleNode("template");
                var nodeTileset  = nodeTemplate.SelectSingleNode("tileset");
                var nodeObject   = nodeTemplate.SelectSingleNode("object");

                if (nodeTileset != null)
                {
                    var tileset = new TiledMapTileset();
                    tileset.firstgid = int.Parse(nodeTileset.Attributes["firstgid"].Value);
                    tileset.source   = nodeTileset.Attributes["source"]?.Value;
                    Tileset = tileset;
                }

                if (nodeObject != null)
                {
                    Object = ParseObject(nodeObject);
                }
            }
            catch (Exception ex)
            {
                throw new TiledException("An error occurred while trying to parse the Tiled template file", ex);
            }
        }

        private TiledObject ParseObject(XmlNode node)
        {
            var nodesProperty = node.SelectNodes("properties/property");
            var nodePolygon   = node.SelectSingleNode("polygon");
            var nodePoint     = node.SelectSingleNode("point");
            var nodeEllipse   = node.SelectSingleNode("ellipse");
            var attrGid       = node.Attributes["gid"];

            var obj = new TiledObject();
            obj.name   = node.Attributes["name"]?.Value;
            obj.@class = node.Attributes["class"]?.Value;
            obj.type   = node.Attributes["type"]?.Value;

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
                var points   = nodePolygon.Attributes["points"].Value;
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

            return obj;
        }

        private TiledProperty[] ParseProperties(XmlNodeList nodeList)
        {
            var result = new List<TiledProperty>();

            foreach (XmlNode node in nodeList)
            {
                var attrType = node.Attributes["type"];

                var property = new TiledProperty();
                property.name  = node.Attributes["name"].Value;
                property.value = node.Attributes["value"]?.Value;
                property.type  = TiledPropertyType.String;

                if (attrType != null)
                {
                    if (attrType.Value == "bool")   property.type = TiledPropertyType.Bool;
                    if (attrType.Value == "color")  property.type = TiledPropertyType.Color;
                    if (attrType.Value == "file")   property.type = TiledPropertyType.File;
                    if (attrType.Value == "float")  property.type = TiledPropertyType.Float;
                    if (attrType.Value == "int")    property.type = TiledPropertyType.Int;
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

        private void ParseObjectGid(ref TiledObject tiledObject, string gid)
        {
            var rawID = uint.Parse(gid);
            var hor   = rawID & FLIPPED_HORIZONTALLY_FLAG;
            var ver   = rawID & FLIPPED_VERTICALLY_FLAG;
            var dia   = rawID & FLIPPED_DIAGONALLY_FLAG;

            tiledObject.dataRotationFlag = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);
            tiledObject.gid = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
        }
    }
}
