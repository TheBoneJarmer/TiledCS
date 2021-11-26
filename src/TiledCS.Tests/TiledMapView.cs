using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiledCS
{
    public abstract class TiledMapView
    {
        #region lifecycle

        public static TiledMapView Load(string filePath)
        {
            var map = new TiledMap(filePath);
            var tiles = map.GetTiledTilesets(filePath);

            var view = Create(map, tiles);

            return view != null
                ? view :
                throw new ArgumentException(map.Orientation + " not supported", nameof(filePath));
        }

        private static TiledMapView Create(TiledMap map, IReadOnlyDictionary<int,TiledTileset> tiles)
        {
            if (map.Orientation == "orthogonal") return new TiledMapOrthogonalView(map, tiles);

            return null;            
        }

        protected TiledMapView(TiledMap map, IReadOnlyDictionary<int, TiledTileset> tiles)
        {
            this.Map = map;
            this.Tiles = Flatten(tiles);
        }

        public static IReadOnlyList<TiledTile> Flatten(IReadOnlyDictionary<int, TiledTileset> tiles)
        {
            var list = new List<TiledTile>();

            foreach (var kvp in tiles)
            {
                for (int i = 0; i < kvp.Value.TileCount; ++i)
                {
                    var idx = kvp.Key + i;

                    while (list.Count <= idx) list.Add(null);

                    list[idx] = kvp.Value.Tiles[i];
                }
            }

            return list;
        }

        #endregion

        #region data

        protected TiledMap Map { get; }
        protected IReadOnlyList<TiledTile> Tiles { get; }

        #endregion

        #region API

        #endregion
    }

    public class TiledMapOrthogonalView : TiledMapView
    {
        internal TiledMapOrthogonalView(TiledMap map, IReadOnlyDictionary<int, TiledTileset> tiles) : base(map, tiles)
        {
        }          
    }

    class TiledLayerView
    {
        private TiledLayer Layer { get; }
        private IReadOnlyList<TiledTile> Tiles { get; }
        

        public void GetDrawables()
        {
            if (Layer.type == "tilelayer")
            {
                for(int mapY=0; mapY < Layer.height; ++mapY)
                {
                    for(int mapX=0; mapX < Layer.width; ++mapX)
                    {
                        var mapIdx = mapY * Layer.width + mapX;
                        var tileId = Layer.data[mapIdx];                        
                    }
                }
            }
        }
    }
    
}
