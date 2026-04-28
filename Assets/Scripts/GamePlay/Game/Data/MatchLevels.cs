using System.Collections.Generic;
using UnityEngine;
using System;

namespace TripleMatch
{
    [CreateAssetMenu(fileName = "MatchLevels", menuName = "ScriptableObject/MatchLevels", order = 0)]
    public class MatchLevels: ScriptableObject
    {
        public List<MatchLevelData> LevelList = new List<MatchLevelData>();
    }
    [Serializable]
    public class MatchLevelData
    {
        public List<MatchTileData> TileList = new List<MatchTileData>();
    }
    [Serializable]
    public class MatchTileData
    {
        public Vector2 Position = Vector2.zero;
    }
}
