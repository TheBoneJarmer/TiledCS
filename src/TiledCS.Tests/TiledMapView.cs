using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiledCS
{
    public class TiledMapView
    {
        #region lifecycle

        public static TiledMapView Load(string filePath)
        {
            var map = new TiledMap(filePath);
            var view = new TiledMapView(map);
            return view ?? throw new TiledException(map.Orientation + " not supported");
        }        

        protected TiledMapView(TiledMap map)
        {            
            var tiles = TileView.CreateFrom(map.Tilesets);
            this.Layers = map.Layers
                .Select(item =>LayerView.CreateFrom(map,item, tiles))
                .ToList();
        }        

        #endregion

        #region data        

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        protected IReadOnlyList<LayerView> Layers { get; }

        #endregion

        #region API

        public IEnumerable<(int X, int Y, TileView T)> GetDrawables(int x, int y, int w, int h)
        {
            return Layers.SelectMany(item => item.GetDrawables(x, y, w, h));
        }

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("{Image} ({ImageX},{ImageY}) ({ImageW}x{ImageH})")]
    public readonly struct TileView
    {
        #region lifecycle       

        public static IReadOnlyList<TileView> CreateFrom(IReadOnlyList<TiledTileset> tiles)
        {
            var list = new List<TileView>();

            foreach (var tset in tiles)
            {
                int idx = tset.FirstGlobalId;
                foreach (var tv in CreateFrom(tset))
                {
                    while (list.Count <= idx) list.Add(default);

                    list[idx] = tv;

                    ++idx;
                }
            }

            return list;
        }

        public static IEnumerable<TileView> CreateFrom(TiledTileset tiles)
        {
            var dict = tiles.Tiles.ToDictionary(item => item.Id, item => item);

            for (int i = 0; i < tiles.TileCount; ++i)
            {
                int x = tiles.Columns == 0 ? 0 : i % tiles.Columns;
                int y = tiles.Columns == 0 ? 0 : i / tiles.Columns;

                // todo: handle spacing and margin

                x *= tiles.TileWidth;
                y *= tiles.TileHeight;

                int w = tiles.TileWidth;
                int h = tiles.TileHeight;

                if (dict.TryGetValue(i, out var tile))
                {
                    if (tile.Image != null)
                    {
                        w = tile.Image.Width;
                        h = tile.Image.Height;
                    }
                }

                yield return new TileView(tiles, i, x, y, w, h);
            }
        }

        private TileView(TiledTileset tset, int tkey, int px, int py, int pw, int ph)
        {
            Tile = tset.Tiles.FirstOrDefault(item => item.Id == tkey);

            Image = tset.Image ?? Tile.Image;

            ImageX = px;
            ImageY = py;
            ImageW = pw;
            ImageH = ph;

            OffsetX = tset.TileOffset?.x ?? 0;
            OffsetY = tset.TileOffset?.y ?? 0;
        }

        #endregion        

        #region properties
        public bool IsEmpty => Image == null;
        public TiledTile Tile { get; }
        public TiledImage Image { get; }
        public int ImageX { get; }
        public int ImageY { get; }
        public int ImageW { get; }
        public int ImageH { get; }

        public int OffsetX { get; }
        public int OffsetY { get; }

        #endregion
    }

    public abstract class LayerView
    {
        #region lifecycle
        public static LayerView CreateFrom(TiledMap map, TiledLayer layer, IReadOnlyList<TileView> tiles)
        {
            if (layer is TiledTilesLayer tilesLayer)
            {
                if (tilesLayer.Data != null && tilesLayer.Data.Length > 0) return new TilesLayerView(map, tilesLayer, tiles);
            }

            if (layer is TiledObjectsLayer objectsLayer)
            {
                return new ObjectsLayerView(map, objectsLayer, tiles);
            }

            throw new NotImplementedException();
        }

        protected LayerView(TiledMap map, TiledLayer layer, IReadOnlyList<TileView> tiles)
        {
            Map = map;
            Layer = layer;
            Tiles = tiles;
        }

        #endregion

        #region data

        protected TiledMap Map { get; }
        protected TiledLayer Layer { get; }
        protected IReadOnlyList<TileView> Tiles { get; }

        #endregion

        #region API

        public abstract IEnumerable<(int X, int Y, TileView T)> GetDrawables(int x, int y, int w, int h);

        #endregion
    }

    [System.Diagnostics.DebuggerDisplay("Tiles {Layer.width}x{Layer.height}")]
    internal class TilesLayerView : LayerView
    {
        internal TilesLayerView(TiledMap map, TiledLayer layer, IReadOnlyList<TileView> tiles)
            : base(map,layer,tiles)
        {
            _Tiles = layer as TiledTilesLayer;
        }

        private readonly TiledTilesLayer _Tiles;

        public override IEnumerable<(int X,int Y, TileView T)> GetDrawables(int sx, int sy, int sw, int sh)
        {
            sx = (sx - _Tiles.OffsetX) / Map.TileWidth;
            sy = (sy - _Tiles.OffsetY) / Map.TileHeight;
            sw /= Map.TileWidth;
            sh /= Map.TileHeight;

            sx = Math.Max(0, sx - 1);
            sy = Math.Max(0, sy - 1);
            sw = Math.Min(_Tiles.Width, sw + 2);
            sh = Math.Min(_Tiles.Height, sh + 2);

            if (_Tiles.Data != null)
            {
                for (int y = sy; y < sh; ++y)
                {
                    for (int x = sx; x < sw; ++x)
                    {
                        var item = GetTile(x, y);
                        if (item.T.IsEmpty) continue;
                        yield return item;
                    }
                }
            }
        }

        private (int X, int Y, TileView T) GetTile(int x, int y)
        {
            var mapIdx = y * _Tiles.Width + x;
            var tileId = _Tiles.Data[mapIdx];
            if (tileId.Index == 0) return default;

            var tile = Tiles[tileId.Index];

            var tx = x * Map.TileWidth + tile.OffsetX + _Tiles.OffsetX;
            var ty = y * Map.TileHeight + tile.OffsetY + _Tiles.OffsetY;

            return (tx, ty, tile);
        }
    }

    [System.Diagnostics.DebuggerDisplay("Objects")]
    internal class ObjectsLayerView : LayerView
    {
        internal ObjectsLayerView(TiledMap map, TiledLayer layer, IReadOnlyList<TileView> tiles)
            : base(map, layer, tiles) { }

        public override IEnumerable<(int X, int Y, TileView T)> GetDrawables(int x, int y, int w, int h)
        {
            yield break;
        }
    }
    
}
