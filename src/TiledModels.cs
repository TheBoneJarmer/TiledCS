using System;
using System.Collections.Generic;

namespace TiledCS
{
    public class TiledMapTileset
    {
        public int firstgid;
        public string source;
    }

    public class TiledProperty
    {
        public string name;
        public string type;
        public string value;
    }

    public class TiledLayer
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

    public class TiledObject
    {
        public int id;
        public string name;
        public string type;
        public float x;
        public float y;
        public int rotation;
        public float width;
        public float height;
        public TiledProperty[] properties;
    }

    public class TiledTile
    {
        public int id;
        public int[] terrain;
        public TiledProperty[] properties;
        public TiledTileAnimation[] animation;
    }

    public class TiledTileAnimation
    {
        public int tileid;
        public int duration;
    }

    public class TiledTerrain
    {
        public string name;
        public int tile;
    }
}