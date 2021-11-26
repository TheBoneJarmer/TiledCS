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
        public TiledMapTileset GetTiledMapTileset(int gid)
        {
            if (Tilesets == null)
            {
                return null;
            }

            for (var i = 0; i < Tilesets.Length; i++)
            {
                if (i < Tilesets.Length - 1)
                {
                    int gid1 = Tilesets[i + 0].firstgid;
                    int gid2 = Tilesets[i + 1].firstgid;

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

            return new TiledMapTileset();
        }
        /// <summary>
        /// Loads external tilesets and matches them to firstGids from elements within the Tilesets array
        /// </summary>
        /// <param name="src">The folder where the TiledMap file is located</param>
        /// <returns>A dictionary where the key represents the firstGid of the associated TiledMapTileset and the value the TiledTileset object</returns>
        public Dictionary<int, TiledTileset> GetTiledTilesets(string src)
        {
            var tilesets = new Dictionary<int, TiledTileset>();
            var info = new FileInfo(src);
            var srcFolder = info.Directory;

            if (Tilesets == null)
            {
                return tilesets;
            }

            foreach (var mapTileset in Tilesets)
            {
                var path = srcFolder + "/" + mapTileset.source;

                if (File.Exists(path))
                {
                    tilesets.Add(mapTileset.firstgid, new TiledTileset(path));
                }
            }

            return tilesets;
        }
        /// <summary>
        /// Locates a specific TiledTile object
        /// </summary>
        /// <param name="mapTileset">An element within the Tilesets array</param>
        /// <param name="tileset">An instance of the TiledTileset class</param>
        /// <param name="gid">An element from within a TiledLayer.data array</param>
        /// <returns>An entry of the TiledTileset.tiles array or null if none of the tile id's matches the gid</returns>
        /// <remarks>Tip: Use the GetTiledMapTileset and GetTiledTilesets methods for retrieving the correct TiledMapTileset and TiledTileset objects</remarks>
        public static TiledTile GetTiledTile(TiledMapTileset mapTileset, TiledTileset tileset, int gid)
        {
            foreach (var tile in tileset.Tiles)
            {
                if (tile.id == gid - mapTileset.firstgid)
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
        public static int[] GetSourceVector(TiledMapTileset mapTileset, TiledTileset tileset, int gid)
        {
            var tileHor = 0;
            var tileVert = 0;

            for (var i = 0; i < tileset.TileCount; i++)
            {
                if (i == gid - mapTileset.firstgid)
                {
                    return new[] { tileHor, tileVert };
                }

                // Update x and y position
                tileHor++;

                if (tileHor == tileset.Image.width / tileset.TileWidth)
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
        public static TiledSourceRect GetSourceRect(TiledMapTileset mapTileset, TiledTileset tileset, int gid)
        {
            var tileHor = 0;
            var tileVert = 0;

            for (var i = 0; i < tileset.TileCount; i++)
            {
                if (i == gid - mapTileset.firstgid)
                {
                    var result = new TiledSourceRect();
                    result.x = tileHor * tileset.TileWidth;
                    result.y = tileVert * tileset.TileHeight;
                    result.width = tileset.TileWidth;
                    result.height = tileset.TileHeight;

                    return result;
                }

                // Update x and y position
                tileHor++;

                if (tileHor == tileset.Image.width / tileset.TileWidth)
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
        public static bool IsTileFlippedHorizontal(TiledLayer layer, int tileHor, int tileVert)
        {
            return layer.IsTileFlippedHorizontal(tileHor, tileVert);
        }
        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public static bool IsTileFlippedHorizontal(TiledLayer layer, int dataIndex)
        {
            return layer.IsTileFlippedHorizontal(dataIndex);
        }
        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        public static bool IsTileFlippedVertical(TiledLayer layer, int tileHor, int tileVert)
        {
            return layer.IsTileFlippedVertical(tileHor, tileVert);
        }
        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        public static bool IsTileFlippedVertical(TiledLayer layer, int dataIndex)
        {
            return layer.IsTileFlippedVertical(dataIndex);
        }
        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public static bool IsTileFlippedDiagonal(TiledLayer layer, int tileHor, int tileVert)
        {
            return layer.IsTileFlippedDiagonal(tileHor, tileVert);
        }
        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>
        /// <param name="layer">An entry of the TiledMap.layers array</param>
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public static bool IsTileFlippedDiagonal(TiledLayer layer, int dataIndex)
        {
            return layer.IsTileFlippedDiagonal(dataIndex);
        }
    }

    partial class TiledLayer
    {
        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>        
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedHorizontal(int tileHor, int tileVert)
        {
            return IsTileFlippedHorizontal(tileHor + (tileVert * width));
        }
        /// <summary>
        /// Checks is a tile is flipped horizontally
        /// </summary>        
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped horizontally or False if not</returns>
        public bool IsTileFlippedHorizontal(int dataIndex)
        {
            return this.data[dataIndex].HasHorizontalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>        
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        public bool IsTileFlippedVertical(int tileHor, int tileVert)
        {
            return IsTileFlippedVertical(tileHor + (tileVert * width));
        }
        /// <summary>
        /// Checks is a tile is flipped vertically
        /// </summary>        
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped vertically or False if not</returns>
        public bool IsTileFlippedVertical(int dataIndex)
        {
            return data[dataIndex].HasVerticalFlip;
        }
        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>        
        /// <param name="tileHor">The tile's horizontal position</param>
        /// <param name="tileVert">The tile's vertical position</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public bool IsTileFlippedDiagonal(int tileHor, int tileVert)
        {
            return IsTileFlippedDiagonal(tileHor + (tileVert * width));
        }
        /// <summary>
        /// Checks is a tile is flipped diagonally
        /// </summary>        
        /// <param name="dataIndex">An index of the TiledLayer.data array</param>
        /// <returns>True if the tile was flipped diagonally or False if not</returns>
        public bool IsTileFlippedDiagonal(int dataIndex)
        {
            return data[dataIndex].HasDiagonalFlip;
        }
    }
}
