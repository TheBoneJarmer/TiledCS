using System;
using System.Collections.Generic;

namespace Reapenshaw.Tiled
{
    public class TiledObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public TiledObject()
        {
            Properties = new Dictionary<string, string>();
        }
    }
}