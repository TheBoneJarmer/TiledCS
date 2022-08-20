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
}