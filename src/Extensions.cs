using System;
using System.Linq;

namespace TiledSharp
{
    public static class Extensions
    {
        public static int[] AsIntArray(this string src)
        {
            return src.Select(x => int.Parse(x.ToString().Length == 0 ? "-1" : x.ToString())).ToArray();
        }
        public static int[] AsIntArray(this string[] src)
        {
            return src.Select(x => int.Parse(x.Length == 0 ? "-1" : x)).ToArray();
        }
    }
}