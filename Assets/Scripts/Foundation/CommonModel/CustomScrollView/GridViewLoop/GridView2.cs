using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Foundation.GridViewLoop
{
    /// <summary>
    /// GridView2
    /// 1. 仅支持 Vertical ScrollRect
    /// 2. 固定列数
    /// 3. 支持多个 prefab，且 prefab 尺寸可不同
    /// 4. 每一行高度 = 该行所有 item 的最大高度
    /// 5. 每一列宽度 = 该列所有 item 的最大宽度
    /// 6. 使用对象池 + 可视区复用
    /// 7. 支持同一个 index 在刷新后切换 prefab 类型
    /// </summary>
    [DisallowMultipleComponent]
    public class GridView2 : MonoBehaviour
    {
        [Serializable]
        public class InitParam
        {
            public int FixedColumnCount = 1;
            public RectOffset Padding = new RectOffset();
            public Vector2 Spacing = Vector2.zero;
            public Vector2 RecyclePadding = new Vector2(100, 100);
            public bool ForceTopLeftAnchor = false;
        }

        [SerializeField] private List<GridViewItemPrefabConfData> mItemPrefabDataList = new();
        [SerializeField] private int mFixedColumnCount = 1;
        [SerializeField] private RectOffset mPadding = new RectOffset();
        [SerializeField] private Vector2 mSpacing = Vector2.zero;
        [SerializeField] private Vector2 mRecyclePadding = new Vector2(100, 100);
        [SerializeField] private bool mForceTopLeftAnchor = false;

        private ScrollRect mScrollRect;
        private RectTransform mScrollRectTransform;
        private RectTransform mViewPortRectTransform;
        private RectTransform mContainerTrans;
        private RectTransform mPoolRoot;

        private bool mInited;
        private int mItemTotalCount;
        private int mRowCount;
        private int mColumnCount;

        private Func<GridView2, int, string> mGetPrefabNameByIndex;
        private Action<GridView2, LoopGridViewItem, int> mOnBindItem;

        private readonly Dictionary<string, GameObject> mPrefabDict = new();
        private readonly Dictionary<string, Vector2> mPrefabSizeDict = new();
        private readonly Dictionary<string, Queue<LoopGridViewItem>> mPoolDict = new();

        private readonly Dictionary<int, LoopGridViewItem> mVisibleItemDict = new();
        private readonly List<int> mTmpRecycleIndexList = new();
        private readonly HashSet<int> mWantedIndexSet = new();
        private readonly HashSet<string> mWarnedStretchPrefabNames = new();

        private readonly List<string> mItemPrefabNames = new();
        private readonly List<Vector2> mItemSizes = new();

        private readonly List<float> mRowHeights = new();
        private readonly List<float> mColumnWidths = new();
        private readonly List<float> mRowStartY = new();
        private readonly List<float> mColumnStartX = new();
        
        

        private int mLastMinRow = -1;
        private int mLastMaxRow = -1;

        public List<GridViewItemPrefabConfData> ItemPrefabDataList => mItemPrefabDataList;
        public int ItemTotalCount => mItemTotalCount;
        public int RowCount => mRowCount;
        public int ColumnCount => mColumnCount;
        public RectTransform ContainerTrans => mContainerTrans;
        public float SetItemScale = 1;
        public float ViewPortWidth => mViewPortRectTransform != null ? mViewPortRectTransform.rect.width : 0f;
        public float ViewPortHeight => mViewPortRectTransform != null ? mViewPortRectTransform.rect.height : 0f;

        public void InitGridView(
            int itemTotalCount,
            Func<GridView2, int, string> getPrefabNameByIndex,
            Action<GridView2, LoopGridViewItem, int> onBindItem,
            InitParam initParam = null)
        {
            if (mInited)
            {
                Debug.LogError("GridView2.InitGridView can only be called once.");
                return;
            }

            mInited = true;
            mItemTotalCount = Mathf.Max(0, itemTotalCount);
            mGetPrefabNameByIndex = getPrefabNameByIndex;
            mOnBindItem = onBindItem;

            if (initParam != null)
            {
                mFixedColumnCount = Mathf.Max(1, initParam.FixedColumnCount);
                mPadding = initParam.Padding ?? new RectOffset();
                mSpacing = initParam.Spacing;
                mRecyclePadding = initParam.RecyclePadding;
                mForceTopLeftAnchor = initParam.ForceTopLeftAnchor;
            }
            else
            {
                mFixedColumnCount = Mathf.Max(1, mFixedColumnCount);
            }

            mScrollRect = GetComponent<ScrollRect>();
            if (mScrollRect == null)
            {
                Debug.LogError("GridView2 requires ScrollRect.");
                return;
            }

            if (!mScrollRect.vertical)
            {
                Debug.LogWarning("GridView2 currently only supports vertical ScrollRect.");
            }

            mScrollRectTransform = mScrollRect.GetComponent<RectTransform>();
            mContainerTrans = mScrollRect.content;
            mViewPortRectTransform = mScrollRect.viewport != null ? mScrollRect.viewport : mScrollRectTransform;

            if (mContainerTrans == null)
            {
                Debug.LogError("GridView2 requires ScrollRect.content.");
                return;
            }

            // 这里只处理 content，本身仍然保持左上角定位
            if (mForceTopLeftAnchor)
            {
                ApplyTopLeft(mContainerTrans);
            }

            CreatePoolRoot();
            BuildPrefabCache();
            RebuildLayoutCache();
            UpdateContentSize();
            UpdateVisibleItems(true);
        }

        public void SetListItemCount(int itemCount, bool resetPos = true)
        {
            itemCount = Mathf.Max(0, itemCount);
            if (mItemTotalCount == itemCount)
                return;

            mItemTotalCount = itemCount;

            RecycleAllVisibleItems();
            RebuildLayoutCache();
            UpdateContentSize();

            if (resetPos)
            {
                Vector2 pos = mContainerTrans.anchoredPosition;
                pos.y = 0f;
                mContainerTrans.anchoredPosition = pos;
            }
            else
            {
                ClampContainerPosition();
            }

            UpdateVisibleItems(true);
        }

        public void SetColumnCount(int columnCount, bool resetPos = true)
        {
            columnCount = Mathf.Max(1, columnCount);
            if (mFixedColumnCount == columnCount)
                return;

            mFixedColumnCount = columnCount;

            RecycleAllVisibleItems();
            RebuildLayoutCache();
            UpdateContentSize();

            if (resetPos)
            {
                Vector2 pos = mContainerTrans.anchoredPosition;
                pos.y = 0f;
                mContainerTrans.anchoredPosition = pos;
            }
            else
            {
                ClampContainerPosition();
            }

            UpdateVisibleItems(true);
        }

        /// <summary>
        /// 当 GetPrefabNameByIndex 的逻辑变化，但数量不一定变化时，调用这个。
        /// 例如 index 0 之前是 Item1，刷新后变成 Item2。
        /// </summary>
        public void RebuildLayoutAndVisibleItems(bool keepPos = true)
        {
            RecycleAllVisibleItems();
            RebuildLayoutCache();
            UpdateContentSize();

            if (keepPos)
                ClampContainerPosition();
            else
                mContainerTrans.anchoredPosition = new Vector2(mContainerTrans.anchoredPosition.x, 0f);

            UpdateVisibleItems(true);
        }

        public void RefreshAllShownItems()
        {
            foreach (var kv in mVisibleItemDict)
            {
                if (kv.Value != null)
                    mOnBindItem?.Invoke(this, kv.Value, kv.Key);
            }
        }

        public void RefreshItemByItemIndex(int itemIndex)
        {
            if (mVisibleItemDict.TryGetValue(itemIndex, out var item) && item != null)
            {
                mOnBindItem?.Invoke(this, item, itemIndex);
            }
        }

        public LoopGridViewItem GetShownItemByItemIndex(int itemIndex)
        {
            mVisibleItemDict.TryGetValue(itemIndex, out var item);
            return item;
        }

        public Vector2 GetItemPos(int itemIndex)
        {
            if (itemIndex < 0 || itemIndex >= mItemTotalCount)
                return Vector2.zero;

            int row = itemIndex / mColumnCount;
            int column = itemIndex % mColumnCount;

            float x = mColumnStartX[column];
            float y = mRowStartY[row];
            return new Vector2(x, -y);
        }

        public void MovePanelToItemByIndex(int itemIndex, float offsetY = 0f)
        {
            if (mItemTotalCount <= 0)
                return;

            itemIndex = Mathf.Clamp(itemIndex, 0, mItemTotalCount - 1);

            int row = itemIndex / mColumnCount;
            float targetY = mRowStartY[row] + offsetY;

            float maxCanMoveY = Mathf.Max(mContainerTrans.rect.height - ViewPortHeight, 0);
            targetY = Mathf.Clamp(targetY, 0, maxCanMoveY);

            Vector2 pos = mContainerTrans.anchoredPosition;
            pos.y = targetY;
            mContainerTrans.anchoredPosition = pos;

            UpdateVisibleItems(true);
        }

        public void ForceRebuildVisibleItems()
        {
            RecycleAllVisibleItems();
            UpdateVisibleItems(true);
        }

        private void LateUpdate()
        {
            if (!mInited)
                return;

            UpdateVisibleItems(false);
        }

        private void BuildPrefabCache()
        {
            mPrefabDict.Clear();
            mPrefabSizeDict.Clear();
            mPoolDict.Clear();
            mWarnedStretchPrefabNames.Clear();

            foreach (var data in mItemPrefabDataList)
            {
                if (data == null || data.mItemPrefab == null)
                    continue;

                string prefabName = data.mItemPrefab.name;
                if (mPrefabDict.ContainsKey(prefabName))
                {
                    Debug.LogError($"Duplicate prefab name in GridView2: {prefabName}");
                    continue;
                }

                mPrefabDict.Add(prefabName, data.mItemPrefab);
                mPoolDict.Add(prefabName, new Queue<LoopGridViewItem>());

                RectTransform rtf = data.mItemPrefab.GetComponent<RectTransform>();
                if (rtf == null)
                {
                    Debug.LogError($"Prefab {prefabName} does not contain RectTransform.");
                    continue;
                } 
                mPrefabSizeDict[prefabName] = rtf.rect.size * SetItemScale;

                int initCount = Mathf.Max(0, data.mInitCreateCount);
                for (int i = 0; i < initCount; i++)
                {
                    var item = CreateItemInstance(prefabName);
                    RecycleItem(item, prefabName);
                }
            }
        }

        private void RebuildLayoutCache()
        {
            mItemPrefabNames.Clear();
            mItemSizes.Clear();
            mRowHeights.Clear();
            mColumnWidths.Clear();
            mRowStartY.Clear();
            mColumnStartX.Clear();

            mColumnCount = Mathf.Max(1, mFixedColumnCount);
            if (mItemTotalCount == 0)
            {
                mRowCount = 0;
                return;
            }

            if (mItemTotalCount < mColumnCount)
                mColumnCount = mItemTotalCount;

            mRowCount = Mathf.CeilToInt((float)mItemTotalCount / mColumnCount);

            for (int i = 0; i < mItemTotalCount; i++)
            {
                string prefabName = ResolvePrefabName(i);
                Vector2 size = ResolvePrefabSize(prefabName);

                mItemPrefabNames.Add(prefabName);
                mItemSizes.Add(size);
            }

            for (int r = 0; r < mRowCount; r++)
                mRowHeights.Add(0f);

            for (int c = 0; c < mColumnCount; c++)
                mColumnWidths.Add(0f);

            for (int i = 0; i < mItemTotalCount; i++)
            {
                int row = i / mColumnCount;
                int column = i % mColumnCount;
                Vector2 size = mItemSizes[i];

                if (size.y > mRowHeights[row])
                    mRowHeights[row] = size.y;

                if (size.x > mColumnWidths[column])
                    mColumnWidths[column] = size.x;
            }

            float curY = mPadding.top;
            for (int r = 0; r < mRowCount; r++)
            {
                mRowStartY.Add(curY);
                curY += mRowHeights[r];
                if (r < mRowCount - 1)
                    curY += mSpacing.y;
            }

            float curX = mPadding.left;
            for (int c = 0; c < mColumnCount; c++)
            {
                mColumnStartX.Add(curX);
                curX += mColumnWidths[c];
                if (c < mColumnCount - 1)
                    curX += mSpacing.x;
            }

            mLastMinRow = -1;
            mLastMaxRow = -1;
        }

        private void UpdateContentSize()
        {
            float width = mPadding.left + mPadding.right;
            for (int i = 0; i < mColumnWidths.Count; i++)
            {
                width += mColumnWidths[i];
                if (i < mColumnWidths.Count - 1)
                    width += mSpacing.x;
            }

            float height = mPadding.top + mPadding.bottom;
            for (int i = 0; i < mRowHeights.Count; i++)
            {
                height += mRowHeights[i];
                if (i < mRowHeights.Count - 1)
                    height += mSpacing.y;
            }

            // mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            mContainerTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        private void UpdateVisibleItems(bool forceRefresh)
        {
            if (mItemTotalCount <= 0 || mRowCount <= 0)
            {
                RecycleAllVisibleItems();
                return;
            }

            ClampContainerPosition();

            float top = Mathf.Max(mContainerTrans.anchoredPosition.y - mRecyclePadding.y, 0f);
            float bottom = mContainerTrans.anchoredPosition.y + ViewPortHeight + mRecyclePadding.y;

            int minRow = FindFirstVisibleRow(top);
            int maxRow = FindLastVisibleRow(bottom);

            if (minRow < 0 || maxRow < 0 || minRow > maxRow)
            {
                RecycleAllVisibleItems();
                return;
            }

            if (!forceRefresh && minRow == mLastMinRow && maxRow == mLastMaxRow)
                return;

            mLastMinRow = minRow;
            mLastMaxRow = maxRow;

            mWantedIndexSet.Clear();

            for (int row = minRow; row <= maxRow; row++)
            {
                int startIndex = row * mColumnCount;
                int endIndex = Mathf.Min(startIndex + mColumnCount, mItemTotalCount);

                for (int i = startIndex; i < endIndex; i++)
                {
                    mWantedIndexSet.Add(i);

                    string wantedPrefabName = ResolvePrefabName(i);

                    if (mVisibleItemDict.TryGetValue(i, out var item))
                    {
                        if (item == null || item.ItemPrefabName != wantedPrefabName)
                        {
                            if (item != null)
                            {
                                RecycleItem(item, item.ItemPrefabName);
                            }

                            mVisibleItemDict.Remove(i);
                            item = null;
                        }
                    }

                    if (item == null)
                    {
                        item = GetOrCreateItem(i);
                        if (item != null)
                            mVisibleItemDict[i] = item;
                    }

                    if (item == null)
                        continue;

                    SetItemTransform(item, i);
                    mOnBindItem?.Invoke(this, item, i);
                }
            }

            mTmpRecycleIndexList.Clear();
            foreach (var kv in mVisibleItemDict)
            {
                if (!mWantedIndexSet.Contains(kv.Key))
                    mTmpRecycleIndexList.Add(kv.Key);
            }

            for (int i = 0; i < mTmpRecycleIndexList.Count; i++)
            {
                int index = mTmpRecycleIndexList[i];
                var item = mVisibleItemDict[index];
                if (item != null)
                    RecycleItem(item, item.ItemPrefabName);
                mVisibleItemDict.Remove(index);
            }
        }

        private LoopGridViewItem GetOrCreateItem(int itemIndex)
        {
            string prefabName = ResolvePrefabName(itemIndex);
            if (!mPoolDict.TryGetValue(prefabName, out var queue))
            {
                Debug.LogError($"Pool not found for prefab: {prefabName}");
                return null;
            }

            LoopGridViewItem item = null;

            while (queue.Count > 0 && item == null)
            {
                var temp = queue.Dequeue();
                if (temp == null)
                    continue;

                if (temp.ItemPrefabName != prefabName)
                {
                    Debug.LogWarning($"GridView2 pool corrected. want={prefabName}, got={temp.ItemPrefabName}, item={temp.name}");
                    Destroy(temp.gameObject);
                    continue;
                }

                item = temp;
            }

            if (item == null)
            {
                item = CreateItemInstance(prefabName);
            }

            if (item != null)
            {
                item.ItemPrefabName = prefabName;
                item.gameObject.SetActive(true);
                item.transform.SetParent(mContainerTrans, false);
            }

            return item;
        }

        private LoopGridViewItem CreateItemInstance(string prefabName)
        {
            if (!mPrefabDict.TryGetValue(prefabName, out var prefab))
            {
                Debug.LogError($"Prefab not found: {prefabName}");
                return null;
            }

            var go = Instantiate(prefab, mPoolRoot);
            go.name = prefab.name;

            // 这里不再强制修改 item 自己的 anchors / pivot
            RectTransform rtf = go.GetComponent<RectTransform>();
            if (rtf != null && IsStretchAnchor(rtf))
            {
                if (mWarnedStretchPrefabNames.Add(prefabName))
                {
                    Debug.LogWarning(
                        $"GridView2 prefab [{prefabName}] root RectTransform is using stretch anchors. " +
                        $"This mode is not recommended for GridView item root, position may be inaccurate.");
                }
            }

            var item = go.GetComponent<LoopGridViewItem>();
            if (item == null)
            {
                item = go.AddComponent<LoopGridViewItem>();
            }

            item.ItemPrefabName = prefabName;
            go.SetActive(false);
            return item;
        }

        private void RecycleItem(LoopGridViewItem item, string prefabName)
        {
            if (item == null)
                return;

            if (string.IsNullOrEmpty(prefabName))
                prefabName = item.ItemPrefabName;

            item.gameObject.SetActive(false);
            item.transform.SetParent(mPoolRoot, false);

            if (!mPoolDict.TryGetValue(prefabName, out var queue))
            {
                queue = new Queue<LoopGridViewItem>();
                mPoolDict[prefabName] = queue;
            }

            queue.Enqueue(item);
        }

        private void RecycleAllVisibleItems()
        {
            foreach (var kv in mVisibleItemDict)
            {
                var item = kv.Value;
                if (item != null)
                {
                    RecycleItem(item, item.ItemPrefabName);
                }
            }

            mVisibleItemDict.Clear();
            mLastMinRow = -1;
            mLastMaxRow = -1;
        }

        private void SetItemTransform(LoopGridViewItem item, int itemIndex)
        {
            if (item == null)
                return;

            RectTransform rtf = item.transform as RectTransform;
            if (rtf == null)
                return;

            int row = itemIndex / mColumnCount;
            int column = itemIndex % mColumnCount;

            float left = mColumnStartX[column];
            float top = mRowStartY[row];

            // 目标是让 item 的左上角落在 (left, top)
            // 但不再强制要求 item 的 pivot/anchor 必须是左上角
            if (IsStretchAnchor(rtf))
            {
                // stretch 情况下很难做精确复用定位，这里尽量兼容，但建议 prefab 根节点不要用 stretch
                rtf.anchoredPosition = new Vector2(left, -top);
            }
            else
            {
                Vector2 size = rtf.rect.size;
                Vector2 pivot = rtf.pivot;
                Vector2 anchor = rtf.anchorMin; // 非 stretch 时 anchorMin == anchorMax

                float pivotPosX = left + size.x * pivot.x;
                float pivotPosY = -top - size.y * (1f - pivot.y);

                float anchorRefX = mContainerTrans.rect.width * anchor.x;
                float anchorRefY = -(1f - anchor.y) * mContainerTrans.rect.height;

                rtf.anchoredPosition = new Vector2(
                    pivotPosX - anchorRefX,
                    pivotPosY - anchorRefY
                );
            }

            rtf.localScale = Vector3.one * SetItemScale;
            rtf.localEulerAngles = Vector3.zero;

            item.ItemIndex = itemIndex;
            item.Row = row;
            item.Column = column;
        }

        private string ResolvePrefabName(int itemIndex)
        {
            string prefabName = mGetPrefabNameByIndex != null ? mGetPrefabNameByIndex(this, itemIndex) : null;

            if (!string.IsNullOrEmpty(prefabName) && mPrefabDict.ContainsKey(prefabName))
                return prefabName;

            foreach (var kv in mPrefabDict)
                return kv.Key;

            Debug.LogError("GridView2 has no valid prefab configured.");
            return string.Empty;
        }

        private Vector2 ResolvePrefabSize(string prefabName)
        {
            if (!string.IsNullOrEmpty(prefabName) && mPrefabSizeDict.TryGetValue(prefabName, out var size))
                return size;

            return Vector2.zero;
        }

        private int FindFirstVisibleRow(float top)
        {
            if (mRowCount <= 0)
                return -1;

            int left = 0;
            int right = mRowCount - 1;
            int ans = mRowCount - 1;

            while (left <= right)
            {
                int mid = (left + right) >> 1;
                float rowBottom = mRowStartY[mid] + mRowHeights[mid];

                if (rowBottom >= top)
                {
                    ans = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            return ans;
        }

        private int FindLastVisibleRow(float bottom)
        {
            if (mRowCount <= 0)
                return -1;

            int left = 0;
            int right = mRowCount - 1;
            int ans = 0;

            while (left <= right)
            {
                int mid = (left + right) >> 1;
                float rowTop = mRowStartY[mid];

                if (rowTop <= bottom)
                {
                    ans = mid;
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return ans;
        }

        private void ClampContainerPosition()
        {
            if (mContainerTrans == null)
                return;

            Vector2 pos = mContainerTrans.anchoredPosition;

            float maxY = Mathf.Max(mContainerTrans.rect.height - ViewPortHeight, 0f);
            pos.y = Mathf.Clamp(pos.y, 0f, maxY);

            mContainerTrans.anchoredPosition = pos;
        }

        private void CreatePoolRoot()
        {
            if (mPoolRoot != null)
                return;

            var go = new GameObject("__GridView2Pool", typeof(RectTransform));
            go.SetActive(false);
            go.transform.SetParent(transform, false);
            mPoolRoot = go.GetComponent<RectTransform>();

            if (mForceTopLeftAnchor)
            {
                ApplyTopLeft(mPoolRoot);
            }
        }

        private bool IsStretchAnchor(RectTransform rt)
        {
            return !Mathf.Approximately(rt.anchorMin.x, rt.anchorMax.x) ||
                   !Mathf.Approximately(rt.anchorMin.y, rt.anchorMax.y);
        }

        private void ApplyTopLeft(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
        }
    }
}