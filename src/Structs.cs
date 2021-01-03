using System;
using System.Collections.Generic;

namespace TiledSharp
{
    public struct TiledVersion
    {
        public int major;
        public int minor;
        public int patch;
    }

    public struct TiledMapTileset
    {
        public int firstgid;
        public string source;
    }

    public struct TiledProperty
    {
        public string name;
        public string value;
    }

    public struct TiledLayer
    {
        public int id;
        public string name;
        public int width;
        public int height;
        public int[] data;
    }

    public struct TiledObject
    {
        public int id;
        public string name;
        public string type;
        public int x;
        public int y;
        public int width;
        public int height;
        public List<TiledProperty> properties;
    }

    public struct TiledObjectGroup
    {
        public int id;
        public string name;
        public List<TiledObject> objects;
    }
}