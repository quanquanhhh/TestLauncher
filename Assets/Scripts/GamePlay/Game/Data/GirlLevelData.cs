using System;
using System.Collections.Generic;
using TripleMatch;

namespace GamePlay.Game.Data
{
    [Serializable]
    public class GirlLevelData
    {
        // public bool HasExtra = false;
        public List<GirlTileData> FirstTileList = new List<GirlTileData>();
        public List<GirlTileData> SecondTileList = new List<GirlTileData>();
        public List<GirlTileData> ThirdTileList = new List<GirlTileData>();
        public List<int> OrderList = new List<int>();
        public GirlLevelData() { }
        public GirlLevelData(MatchLevelData levelData)
        {
            for (int i = 0; i < levelData.TileList.Count; i++)
            {
                MatchTileData matchTileData = levelData.TileList[i];
                GirlTileData girlTileData = new GirlTileData();
                FirstTileList.Add(girlTileData);
                girlTileData.X = (int)(matchTileData.Position.x);
                girlTileData.Y = (int)(matchTileData.Position.y);
                girlTileData.Sibling = i;
            }
        }
        public bool IsLevelEmpty()
        {
            if (FirstTileList.Count > 0) return false;
            if (SecondTileList.Count > 0) return false;
            if (ThirdTileList.Count > 0) return false;
            return true;
        }
    }
    [Serializable]
    public class GirlTileData
    {
        public int X = 0;
        public int Y = 0;
        public int Sibling = 0;
        public int Icon = -1;
        // public int TrashId = -1;
        // public int InTrashId = -1;
    }
}