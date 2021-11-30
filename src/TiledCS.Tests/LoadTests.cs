using System.Linq;

using NUnit.Framework;

namespace TiledCS
{
    public class LoadTests
    {        
        [TestCase("Resources\\automapping\\1\\1.tmx")]
        [TestCase("Resources\\automapping\\1\\1.ver172.tmx")]
        [TestCase("Resources\\automapping\\2\\2.tmx")]
        [TestCase("Resources\\automapping\\3\\3.tmx")]
        [TestCase("Resources\\automapping\\4\\4.tmx")]
        [TestCase("Resources\\data\\mapobject.tmx")]
        [TestCase("Resources\\exotic\\Properties.tmx")]
        public void LoadTMX(string tmxPath)
        {
            var map = new TiledMap(tmxPath);
            Assert.NotNull(map);

            TestContext.WriteLine($"Version:{map.Version} Size:{map.Width}x{map.Height} Orientation:{map.Orientation} RenderOrder:{map.RenderOrder} Infinite:{map.Infinite}");                        
        }

        
        [TestCase("Resources\\exotic\\infinite.tmx")]
        public void TestLoadingInfiniteTMX(string tmxPath)
        {
            var map = new TiledMap(tmxPath);            
            Assert.NotNull(map);            
        }

        [TestCase("Resources\\examples\\rpg\\beach_tileset.tsx")]
        [TestCase("Resources\\exotic\\Tiles32x32.A.tsx")]
        [TestCase("Resources\\examples\\perspective_walls.tsx")]        
        public void LoadTSX(string tsxPath)
        {
            var tiles = new TiledTileset(tsxPath);
            Assert.NotNull(tiles);

            var view = TileView.CreateFrom(tiles).ToArray();
        }

        [Test]
        public void TestLoadComplexTMX()
        {
            var map = new TiledMap("Resources\\examples\\orthogonal-outside.tmx");
            Assert.NotNull(map);
            Assert.AreEqual(3, map.Layers.Length);

            var groundLayer = map.Layers[0] as TiledTilesLayer;
            var fringeLayer = map.Layers[1] as TiledTilesLayer;
            var objectsLayer = map.Layers[2] as TiledObjectsLayer;

            Assert.AreEqual(29, objectsLayer.Objects.Length);            
        }

        [TestCase("Resources\\examples\\orthogonal-outside.tmx")]        
        public void TestEvaluation(string tmxPath)
        {
            var mapView = TiledMapView.Load(tmxPath);

            var drawables = mapView
                .GetDrawables(30, 50, 800, 600)
                .ToArray();
        }
    }
}