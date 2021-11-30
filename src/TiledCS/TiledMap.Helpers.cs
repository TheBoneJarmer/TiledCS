using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TiledCS
{
    partial class TiledMap
    {
        /// <summary>
        /// Locates the right TiledMapTileset object for you within the Tilesets array
        /// </summary>
        /// <param name="gid">A value from the TiledLayer.data array</param>
        /// <returns>An element within the Tilesets array or null if no match was found</returns>
        public TiledTileset GetTiledMapTileset(int gid)
        {
            if (Tilesets == null)
            {
                return null;
            }

            for (var i = 0; i < Tilesets.Length; i++)
            {
                if (i < Tilesets.Length - 1)
                {
                    int gid1 = Tilesets[i + 0].FirstGlobalId;
                    int gid2 = Tilesets[i + 1].FirstGlobalId;

                    if (gid >= gid1 && gid < gid2)
                    {
                        return Tilesets[i];
                    }
                }
                else
                {
                    return Tilesets[i];
                }
            }

            return new TiledTileset();
        }
        
        /// <summary>
        /// Locates a specific TiledTile object
        /// </summary>
        /// <param name="mapTileset">An element within the Tilesets array</param>
        /// <param name="tileset">An instance of the TiledTileset class</param>
        /// <param name="gid">An element from within a TiledLayer.data array</param>
        /// <returns>An entry of the TiledTileset.tiles array or null if none of the tile id's matches the gid</returns>
        /// <remarks>Tip: Use the GetTiledMapTileset and GetTiledTilesets methods for retrieving the correct TiledMapTileset and TiledTileset objects</remarks>
        public static TiledTile GetTiledTile(TiledTilesetSource mapTileset, TiledTileset tileset, int gid)
        {
            foreach (var tile in tileset.Tiles)
            {
                if (tile.Id == gid - mapTileset.FirstGlobalId)
                {
                    return tile;
                }
            }

            return null;
        }
        /// <summary>
        /// This method can be used to figure out the x and y position on a Tileset image for rendering tiles. 
        /// </summary>
        /// <param name="mapTileset">An element of the Tilesets array</param>
        /// <param name="tileset">An instance of the TiledTileset class</param>
        /// <param name="gid">An element within a TiledLayer.data array</param>
        /// <returns>An int array of length 2 containing the x and y position of the source rect of the tileset image. Multiply the values by the tile width and height in pixels to get the actual x and y position. Returns null if the gid was not found</returns>
        /// <remarks>This method currently doesn't take margin into account</remarks>
        [Obsolete("Please use GetSourceRect instead because with future versions of Tiled this method may no longer be sufficient")]
        public static int[] GetSourceVector(TiledTilesetSource mapTileset, TiledTileset tileset, int gid)
        {
            var tileHor = 0;
            var tileVert = 0;

            for (var i = 0; i < tileset.TileCount; i++)
            {
                if (i == gid - mapTileset.FirstGlobalId)
                {
                    return new[] { tileHor, tileVert };
                }

                // Update x and y position
                tileHor++;

                if (tileHor == tileset.Image.Width / tileset.TileWidth)
                {
                    tileHor = 0;
                    tileVert++;
                }
            }

            return null;
        }

        /// <summary>
        /// This method can be used to figure out the source rect on a Tileset image for rendering tiles.
        /// </summary>
        /// <param name="mapTileset"></param>
        /// <param name="tileset"></param>
        /// <param name="gid"></param>
        /// <returns>An instance of the class TiledSourceRect that represents a rectangle. Returns null if the provided gid was not found within the tileset.</returns>
        [Obsolete("data already contains the flags")]
        public static TiledSourceRect GetSourceRect(TiledTilesetSource mapTileset, TiledTileset tileset, int gid)
        {
            var tileHor = 0;
            var tileVert = 0;

            for (var i = 0; i < tileset.TileCount; i++)
            {
                if (i == gid - mapTileset.FirstGlobalId)
                {
                    var result = new TiledSourceRect();
                    result.X = tileHor * tileset.TileWidth;
                    result.Y = tileVert * tileset.TileHeight;
                    result.Width = tileset.TileWidth;
                    result.Height = tileset.TileHeight;

                    return result;
                }

                // Update x and y position
                tileHor++;

                if (tileHor == tileset.Image.Width / tileset.TileWidth)
                {
                    tileHor = 0;
                    tileVert++;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        [Obsolete("data already contains the flags")]
        public static bool IsTileFlippedHorizontal(TiledTilesLayer layer, int tileHor, int tileVert)
        {
            return layer.Data[tileHor + layer.Width * tileVert].HasHorizontalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        [Obsolete("data already contains the flags")]
        public static bool IsTileFlippedHorizontal(TiledTilesLayer layer, int dataIndex)
        {
            return layer.Data[dataIndex].HasHorizontalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        [Obsolete("data already contains the flags")]
        public static bool IsTileFlippedVertical(TiledTilesLayer layer, int tileHor, int tileVert)
        {
            return layer.Data[tileHor + layer.Width * tileVert].HasVerticalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        [Obsolete("data already contains the flags")]
        public static bool IsTileFlippedVertical(TiledTilesLayer layer, int dataIndex)
        {
            return layer.Data[dataIndex].HasVerticalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        [Obsolete("data already contains the flags")]
        public static bool IsTileFlippedDiagonal(TiledTilesLayer layer, int tileHor, int tileVert)
        {
            return layer.Data[tileHor + layer.Width * tileVert].HasDiagonalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public static bool IsTileFlippedDiagonal(TiledTilesLayer layer, int dataIndex)
        {
            return layer.Data[dataIndex].HasDiagonalFlip;
        }
    }
}
