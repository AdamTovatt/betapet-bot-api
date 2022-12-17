using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Betapet.Models.InGame
{
    public class UniqueTileCollection
    {
        public List<Tile> Tiles { get { return tiles.Values.ToList(); } }

        private Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();

        public void AddTile(Tile tile)
        {
            if (!tiles.ContainsKey(tile.GetHash()))
                tiles.Add(tile.GetHash(), tile);
        }

        public void AddTiles(List<Tile> tiles)
        {
            foreach(Tile tile in tiles)
            {
                AddTile(tile);
            }
        }

        public void Clear()
        {
            tiles.Clear();
        }
    }
}
