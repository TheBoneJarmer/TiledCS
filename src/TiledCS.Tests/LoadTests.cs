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
        public void LoadTMX(string tmxPath)
        {
            var map = new TiledMap(tmxPath);
            Assert.NotNull(map);

            TestContext.WriteLine($"Version:{map.TiledVersion} Size:{map.Width}x{map.Height} Orientation:{map.Orientation} RenderOrder:{map.RenderOrder} Infinite:{map.Infinite}");

            
        }

        [TestCase("Resources\\wangtiles\\grassAndWater.tsx")]
        [TestCase("Resources\\wangtiles\\PathAndObjects.tsx")]
        [TestCase("Resources\\wangtiles\\walkways.tsx")]
        [TestCase("Resources\\wangtiles\\wangblob.tsx")]
        public void LoadTSX(string tsxPath)
        {
            var tiles = new TiledTileset(tsxPath);
            Assert.NotNull(tiles);
        }
    }
}