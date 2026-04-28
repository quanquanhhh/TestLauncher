using UnityEngine;

namespace Foundation.GridViewLoop
{   
    public enum GridItemArrangeType
    {
        TopLeftToBottomRight = 0,
        BottomLeftToTopRight,
        TopRightToBottomLeft,
        BottomRightToTopLeft,
    }
    public class LoopGridViewSettingParam
    {
        public object mItemSize = null;
        public object mPadding = null;
        public object mItemPadding = null;
        public object mGridFixedType = null;
        public object mFixedRowOrColumnCount = null;
    }

    public class LoopGridViewInitParam
    {
        // all the default values
        public float mSmoothDumpRate = 0.3f;
        public float mSnapFinishThreshold = 0.01f;
        public float mSnapVecThreshold = 145;

        public static LoopGridViewInitParam CopyDefaultInitParam()
        {
            return new LoopGridViewInitParam();
        }
    }

    public enum GridFixedType
    {
        ColumnCountFixed = 0,
        RowCountFixed,
    }
    
    [System.Serializable]
    public class GridViewItemPrefabConfData
    {
        public GameObject mItemPrefab = null;
        public int mInitCreateCount = 0;
    }
     
}