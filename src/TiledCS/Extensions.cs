using System;
using System.IO;
using System.Linq;

namespace TiledCS
{
    /// <summary>
    /// Defines all extension methods used within TiledCS
    /// </summary>
    public static class Extensions
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

        internal static MemoryStream OpenReadBase64(this string base64text)
        {
            return new MemoryStream(Convert.FromBase64String(base64text));
        }

        internal static string GetStringOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, string defval)
        {
            var att = attributes[name];
            return att != null
                ? att.Value
                : defval;
        }

        internal static int GetIntegerOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, int defval)
        {
            var att = attributes[name];
            return att != null
                ? int.Parse(att.Value)
                : defval;
        }

        internal static float GetSingleOrDefault(this System.Xml.XmlAttributeCollection attributes, string name, float defval)
        {
            var att = attributes[name];
            return att != null
                ? float.Parse(att.Value, System.Globalization.CultureInfo.InvariantCulture)
                : defval;
        }
    }
}