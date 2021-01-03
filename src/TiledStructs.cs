using System;
using System.Collections.Generic;

namespace TiledCS
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
        public string type;
        public string value;
    }

    public struct TiledLayer
    {
        public int id;
        public string name;
        public int width;
        public int height;
        public string type;
        public bool visible;
        public int[] data;
        public TiledObject[] objects;
    }

    public struct TiledObject
    {
        public int id;
        public string name;
        public string type;
        public int x;
        public int y;
        public int rotation;
        public int width;
        public int height;
        public TiledProperty[] properties;
    }

    public struct TiledTile
    {
        public int id;
        public int[] terrain;
        public TiledProperty[] properties;
    }

    public struct TiledTerrain
    {
        public string name;
        public int tile;
    }
}