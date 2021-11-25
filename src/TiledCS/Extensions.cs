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
    }
}