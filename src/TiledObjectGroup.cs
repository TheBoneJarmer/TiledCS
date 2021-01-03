using System;
using System.Collections.Generic;

namespace Reapenshaw.Tiled
{
    public class TiledObjectGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TiledObject> Objects { get; set; }

        public TiledObjectGroup()
        {
            Objects = new List<TiledObject>();
        }
    }
}