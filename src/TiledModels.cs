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
        /// Defines if the layer is visible in the editor
        /// </summary>
        public bool visible;
        /// <summary>
        /// An int array of gid numbers which define which tile is being used where. The length of the array equals the layer width * the layer height. Is null when the layer is not a tilelayer.
        /// </summary>
        public int[] data;
        /// <summary>
        /// A parallel array to data which stores the rotation flags of the tile.
        /// Bit 3 is is horizontal flip,
        /// bit 2 is vertical flip, and
        /// bit 1 is (anti) diagonal flip.
        /// Is null when the layer is not a tilelayer.
        /// </summary>
        public byte[] dataRotationFlags;
        /// <summary>
        /// The list of objects in case of an objectgroup layer. Is null when the layer has no objects.
        /// </summary>
        public TiledObject[] objects;

        const uint FLIPPED_HORIZONTALLY_FLAG = 0b10000000000000000000000000000000;
        const uint FLIPPED_VERTICALLY_FLAG   = 0b01000000000000000000000000000000;
        const uint FLIPPED_DIAGONALLY_FLAG   = 0b00100000000000000000000000000000;
        /// <summary>
        /// How many times we shift the FLIPPED flags to the right in order to store it in a byte.
        /// For example: 0b10100000000000000000000000000000 >> SHIFT_FLIP_FLAG_TO_BYTE = 0b00000101
        /// </summary>
        const int SHIFT_FLIP_FLAG_TO_BYTE = 29;
        /// <summary>
        /// Reads the data from the comma separated values into data and rotationFlags.
        /// </summary>
        public void ReadGids(string csvText)
        {
            string[] csvs = csvText.Split(',');
            data = new int[csvs.Length];
            dataRotationFlags = new byte[csvs.Length];

            for (int i = 0; i < csvs.Length; i++)
            {
                uint rawID = uint.Parse(csvs[i]);
                uint hor = ((rawID & FLIPPED_HORIZONTALLY_FLAG));
                uint ver = ((rawID & FLIPPED_VERTICALLY_FLAG));
                uint dia = ((rawID & FLIPPED_DIAGONALLY_FLAG));
                dataRotationFlags[i] = (byte)((hor | ver | dia) >> SHIFT_FLIP_FLAG_TO_BYTE);

                // assign data to rawID with the rotation flags cleared
                data[i] = (int)(rawID & ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG | FLIPPED_DIAGONALLY_FLAG));
            }
        }

        private void _GetTileRotation(int dataIndex, out bool horizontal, out bool vertical, out bool diagonal)
        {
            horizontal = (dataRotationFlags[dataIndex] & (FLIPPED_HORIZONTALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
            vertical = (dataRotationFlags[dataIndex] & (FLIPPED_VERTICALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
            diagonal = (dataRotationFlags[dataIndex] & (FLIPPED_DIAGONALLY_FLAG >> SHIFT_FLIP_FLAG_TO_BYTE)) > 0;
        }

        /// <summary>
        /// Retrieves the rotations of a tile at the given index.
        /// </summary>
        public void GetTileRotation(int dataIndex, out bool horizontal, out bool vertical, out bool diagonal) {
            _GetTileRotation(dataIndex, out horizontal, out vertical, out diagonal);
        }

        /// <summary>
        /// Retrieves the rotations of a tile at the given x and y coordinates.
        /// </summary>
        public void GetTileRotation(int x, int y, out bool horizontal, out bool vertical, out bool diagonal) {
            int dataIndex = x + (y * width);
            _GetTileRotation(dataIndex, out horizontal, out vertical, out diagonal);
        }
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
        /// An array of properties. Is null if none were defined.
        /// </summary>
        public TiledProperty[] properties;
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
}