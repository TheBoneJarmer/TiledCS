using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TiledCS
{
    /// <summary>
    /// Defines all extension methods used within TiledCS
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Converts a comma separated string to an int array
        /// </summary>
        /// <param name="src">The comma separated string source</param>
        /// <returns>The parsed int array</returns>
        public static int[] AsIntArray(this string src)
        {
            return src.Select(x => int.Parse(x.ToString().Length == 0 ? "-1" : x.ToString())).ToArray();
        }
        /// <summary>
        /// Converts a string array whose values are actually all numbers to an int array
        /// </summary>
        /// <param name="src">The string array</param>
        /// <returns>The parsed int array</returns>
        public static int[] AsIntArray(this string[] src)
        {
            return src.Select(x => int.Parse(x.Length == 0 ? "-1" : x)).ToArray();
        }

        

        public static string GetStringOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, string defval)
        {
            var att = attributes[name];
            return att != null
                ? att.Value
                : defval;
        }

        public static int GetIntegerOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, int defval)
        {
            var att = attributes[name];
            return att != null
                ? int.Parse(att.Value)
                : defval;
        }

        public static float GetSingleOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, float defval)
        {
            var att = attributes[name];
            return att != null
                ? float.Parse(att.Value, System.Globalization.CultureInfo.InvariantCulture)
                : defval;
        }

        public static double GetDoubleOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, double defval)
        {
            var att = attributes[name];
            return att != null
                ? double.Parse(att.Value, System.Globalization.CultureInfo.InvariantCulture)
                : defval;
        }
    }
}