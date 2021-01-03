using System;

namespace Reapenshaw.Tiled
{
    public class TiledLayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] Data { get; set; }

        public int this[int x, int y]
        {
            get { return Data[(y * Width) + x]; }
        }

        public TiledLayer()
        {

        }
    }
}