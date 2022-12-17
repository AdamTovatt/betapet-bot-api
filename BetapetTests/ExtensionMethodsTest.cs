using Betapet.Helpers;
using Betapet.Models.InGame;

namespace BetapetTests
{
    [TestClass]
    public class ExtensionMethodsTest
    {
        [TestMethod]
        public void TestContainsTiles1()
        {
            List<Tile> baseTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("B"),
                new Tile("A"),
                new Tile("I"),
                new Tile("O"),
            };

            List<Tile> checkTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("I"),
                new Tile("O"),
            };

            Assert.IsTrue(baseTiles.ContainsTiles(checkTiles));
        }

        [TestMethod]
        public void TestContainsTiles2()
        {
            List<Tile> baseTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("P"),
                new Tile("A"),
                new Tile("C"),
                new Tile("E"),
            };

            List<Tile> checkTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("A"),
                new Tile("P"),
            };

            Assert.IsTrue(baseTiles.ContainsTiles(checkTiles));
        }

        [TestMethod]
        public void TestContainsTiles3()
        {
            List<Tile> baseTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("B"),
                new Tile("S"),
                new Tile("I"),
                new Tile("O"),
            };

            List<Tile> checkTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("A"),
                new Tile("O"),
            };

            Assert.IsFalse(baseTiles.ContainsTiles(checkTiles));
        }

        [TestMethod]
        public void TestContainsTiles4()
        {
            List<Tile> baseTiles = new List<Tile>() { };

            List<Tile> checkTiles = new List<Tile>()
            {
                new Tile("A"),
                new Tile("I"),
                new Tile("O"),
            };

            Assert.IsFalse(baseTiles.ContainsTiles(checkTiles));
        }
    }
}