using System;
using System.Collections.Generic;

namespace Reapenshaw.Tiled
{
    public class TiledTile
    {
        public int Id { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public TiledTile()
        {
            Properties = new Dictionary<string, string>();
        }
    }
}