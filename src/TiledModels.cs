namespace TiledCS
{
    /// <summary>
    /// Represents an element within the Tilesets array of a TiledMap object
    /// </summary>
    public class TiledMapTileset
    {
        /// <summary>
        /// The first gid defines which gid matches the tile with source vector 0,0. Is used to determine which tileset belongs to which gid
        /// </summary>
        public int firstgid;
        /// <summary>
        /// The tsx file path as defined in the map file itself
        /// </summary>
        public string source;
    }

    /// <summary>
    /// Represents a property object in both tilesets, maps, layers and objects. Values are all in string but you can use the 'type' property for conversions
    /// </summary>
    public class TiledProperty
    {
        /// <summary>
        /// The property name or key in string format
        /// </summary>
        public string name;
        /// <summary>
        /// The property type as used in Tiled. Can be bool, number, string, ...
        /// </summary>
        public string type;
        /// <summary>
        /// The value in string format
        /// </summary>
        public string value;
    }

    /// <summary>
    /// Represents a tile layer as well as an object layer within a tile map
    /// </summary>
    public class TiledLayer
    {
        /// <summary>
        /// The layer id
        /// </summary>
        public int id;
        /// <summary>
        /// The layer name
        /// </summary>
        public string name;
        /// <summary>
        /// Total horizontal tiles
        /// </summary>
        public int width;
        /// <summary>
        /// Total vertical tiles
        /// </summary>
        public int height;
        /// <summary>
        /// The layer type. Usually this is "objectgroup" or "tilelayer".
        /// </summary>
        public string type;
        /// <summary>
        /// The tint color set by the user in hex code
        /// </summary>
        public string tintcolor;
        /// <summary>
        /// Defines if the layer is visible in the editor
        /// </summary>
        public bool visible;
        /// <summary>
        /// Is true when the layer is locked
        /// </summary>
        public bool locked;
        /// <summary>
        /// The horizontal offset
        /// </summary>
        public int offsetX;
        /// <summary>
        /// The vertical offset
        /// </summary>
        public int offsetY;
        /// <summary>
        /// An int array of gid numbers which define which tile is being used where. The length of the array equals the layer width * the layer height. Is null when the layer is not a tilelayer.
        /// </summary>
        public int[] data;
        /// <summary>
        /// A parallel array to data which stores the rotation flags of the tile.
        /// Bit 3 is horizontal flip,
        /// bit 2 is vertical flip, and
        /// bit 1 is (anti) diagonal flip.
        /// Is null when the layer is not a tilelayer.
        /// </summary>
        public byte[] dataRotationFlags;
        /// <summary>
        /// The list of objects in case of an objectgroup layer. Is null when the layer has no objects.
        /// </summary>
        public TiledObject[] objects;
        /// <summary>
        /// The layer properties if set
        /// </summary>
        public TiledProperty[] properties;

        public TiledImage image;
    }

    /// <summary>
    /// Represents an tiled object defined in object layers
    /// </summary>
    public class TiledObject
    {
        /// <summary>
        /// The object id
        /// </summary>
        public int id;
        /// <summary>
        /// The object's name
        /// </summary>
        public string name;
        /// <summary>
        /// The object type if defined. Null if none was set.
        /// </summary>
        public string type;
        /// <summary>
        /// The object's x position in pixels
        /// </summary>
        public float x;
        /// <summary>
        /// The object's y position in pixels
        /// </summary>
        public float y;
        /// <summary>
        /// The object's rotation
        /// </summary>
        public int rotation;
        /// <summary>
        /// The object's width in pixels
        /// </summary>
        public float width;
        /// <summary>
        /// The object's height in pixels
        /// </summary>
        public float height;
        /// <summary>
        /// The tileset gid when the object is linked to a tile
        /// </summary>
        public int gid;
        /// <summary>
        /// An array of properties. Is null if none were defined.
        /// </summary>
        public TiledProperty[] properties;
        /// <summary>
        /// If an object was set to a polygon shape, this property will be set and can be used to access the polygon's data
        /// </summary>
        public TiledPolygon polygon;
        /// <summary>
        /// If an object was set to a polyline shape, this property will be set and can be used to access the polyline's data
        /// </summary>
        public TiledPolyline polyline;
        /// <summary>
        /// If an object was set to a point shape, this property will be set
        /// </summary>
        public TiledPoint point;
        /// <summary>
        /// If an object was set to an ellipse shape, this property will be set
        /// </summary>
        public TiledEllipse ellipse;
    }

    /// <summary>
    /// Represents a polygon shape
    /// </summary>
    public class TiledPolygon
    {
        /// <summary>
        /// The array of vertices where each two elements represent an x and y position. Like 'x,y,x,y,x,y,x,y'.
        /// </summary>
        public float[] points;
    }

    /// <summary>
    /// Represents a poly line shape
    /// </summary>
    public class TiledPolyline
    {
        /// <summary>
        /// The array of vertices where each two elements represent an x and y position. Like 'x,y,x,y,x,y,x,y'.
        /// </summary>
        public float[] points;
    }

    /// <summary>
    /// Represents a point shape
    /// </summary>
    public class TiledPoint
    {
        
    }

    /// <summary>
    /// Represents an ellipse shape
    /// </summary>
    public class TiledEllipse
    {
        
    }

    /// <summary>
    /// Represents a tile within a tileset
    /// </summary>
    /// <remarks>These are not defined for all tiles within a tileset, only the ones with properties, terrains and animations.</remarks>
    public class TiledTile
    {
        /// <summary>
        /// The tile id
        /// </summary>
        public int id;
        /// <summary>
        /// The custom tile type, set by the user
        /// </summary>
        public string type;
        /// <summary>
        /// The terrain definitions as int array. These are indices indicating what part of a terrain and which terrain this tile represents.
        /// </summary>
        /// <remarks>In the map file empty space is used to indicate null or no value. However, since it is an int array I needed something so I decided to replace empty values with -1.</remarks>
        public int[] terrain;
        /// <summary>
        /// An array of properties. Is null if none were defined.
        /// </summary>
        public TiledProperty[] properties;
        /// <summary>
        /// An array of tile animations. Is null if none were defined. 
        /// </summary>
        public TiledTileAnimation[] animation;
        /// <summary>
        /// The individual tile image
        /// </summary>
        public TiledImage image;
        /// <summary>
        /// Objects that this tile can have
        /// </summary>
        public TiledObject[] objects;
    }

    /// <summary>
    /// Represents an image
    /// </summary>
    public class TiledImage
    {
        /// <summary>
        /// The image width
        /// </summary>
        public int width;
        
        /// <summary>
        /// The image height
        /// </summary>
        public int height;
        
        /// <summary>
        /// The image source path
        /// </summary>
        public string source;
    }

    /// <summary>
    /// Represents a tile animation. Tile animations are a group of tiles which act as frames for an animation.
    /// </summary>
    public class TiledTileAnimation
    {
        /// <summary>
        /// The tile id within a tileset
        /// </summary>
        public int tileid;
        /// <summary>
        /// The duration in miliseconds
        /// </summary>
        public int duration;
    }

    /// <summary>
    /// Represents a terrain definition.
    /// </summary>
    public class TiledTerrain
    {
        /// <summary>
        /// The terrain name
        /// </summary>
        public string name;
        /// <summary>
        /// The tile used as icon for the terrain editor
        /// </summary>
        public int tile;
    }

    /// <summary>
    /// Used as data type for the GetSourceRect method. Represents basically a rectangle.
    /// </summary>
    public class TiledSourceRect
    {
        /// <summary>
        /// The x position in pixels from the tile location in the source image
        /// </summary>
        public int x;
        /// <summary>
        /// The y position in pixels from the tile location in the source image
        /// </summary>
        public int y;
        /// <summary>
        /// The width in pixels from the tile in the source image
        /// </summary>
        public int width;
        /// <summary>
        /// The height in pixels from the tile in the source image
        /// </summary>
        public int height;
    }

    /// <summary>
    /// Represents a layer or object group
    /// </summary>
    public class TiledGroup
    {
        /// <summary>
        /// The group's id
        /// </summary>
        public int id;
        /// <summary>
        /// The group's name
        /// </summary>
        public string name;
        /// <summary>
        /// The group's visibility
        /// </summary>
        public bool visible;
        /// <summary>
        /// The group's locked state
        /// </summary>
        public bool locked;
        /// <summary>
        /// The group's user properties
        /// </summary>
        public TiledProperty[] properties;
        /// <summary>
        /// The group's layers
        /// </summary>
        public TiledLayer[] layers;
        /// <summary>
        /// The group's objects
        /// </summary>
        public TiledObject[] objects;
        /// <summary>
        /// The group's subgroups
        /// </summary>
        public TiledGroup[] groups;
    }
}