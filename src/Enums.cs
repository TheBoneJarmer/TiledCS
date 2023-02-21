using System;

namespace TiledCS
{
    /// <summary>
    /// Represents the layer type
    /// </summary>
    public enum TiledLayerType
    {
        /// <summary>
        /// Indicates that the layer is an object layer
        /// </summary>
        ObjectLayer,
        
        /// <summary>
        /// Indicates that the layer is a tile layer
        /// </summary>
        TileLayer,
        
        /// <summary>
        /// Indicates that the layer is an image layer
        /// </summary>
        ImageLayer
    }

    /// <summary>
    /// Represents property's value data type
    /// </summary>
    public enum TiledPropertyType
    {
        /// <summary>
        /// A string value
        /// </summary>
        String,
        
        /// <summary>
        /// A bool value
        /// </summary>
        Bool,
        
        /// <summary>
        /// A color value in hex format
        /// </summary>
        Color,
        
        /// <summary>
        /// A file path as string
        /// </summary>
        File,
        
        /// <summary>
        /// A float value
        /// </summary>
        Float,
        
        /// <summary>
        /// An int value
        /// </summary>
        Int,
        
        /// <summary>
        /// An object value which is the id of an object in the map
        /// </summary>
        Object
    }

    /// <summary>
    /// Bitmap of flip states
    /// </summary>
    [Flags]
    public enum Flip
    {
        /// <summary>
        /// Tile is not flipped
        /// </summary>
        None = 0,

        /// <summary>
        /// Tile is flipped horizontally
        /// </summary>
        Horizontal = 1 << 2,

        /// <summary>
        /// Tile is flipped vertically
        /// </summary>
        Vertical = 1 << 1,

        /// <summary>
        /// Tile is flipped diagonally
        /// </summary>
        Diagonal = 1 << 0,
    }

    [Flags]
    internal enum Trans
    {
        None = 0,
        Flip_H = 1 << 2,
        Flip_V = 1 << 1,
        Flip_D = 1 << 0,

        Rotate_90 = Flip_D | Flip_H,
        Rotate_180 = Flip_H | Flip_V,
        Rotate_270 = Flip_V | Flip_D,

        Rotate_90AndFlip_H = Flip_H | Flip_V | Flip_D,
    }
}