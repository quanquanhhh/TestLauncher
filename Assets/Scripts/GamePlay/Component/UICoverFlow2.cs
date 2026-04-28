using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePlay.Component
{
    [DisallowMultipleComponent]
    public class UICoverFlow2 : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum AutoScrollDirection
        {
            RightToLeft,
            LeftToRight
        }

        [Header("Slot Count")]
        [SerializeField] private int visibleSlotCount = 3;   // 可视槽数量，建议奇数
        [SerializeField] private int bufferSlotCount = 2;    // 额外替换槽数量，建议偶数

        [Header("Layout")]
        [SerializeField] private float falloff = 0.5f;
        [SerializeField] private float itemSpacing = 260f;
        [SerializeField] private float dragPixelsPerItem = 260f;
        [SerializeField] private float snapSpeed = 12f;
        [SerializeField] private float sideYOffset = 18f;
        [SerializeField] private float centerScale = 1.0f;
        [SerializeField] private float sideScale = 0.72f;
        [SerializeField] private float centerAlpha = 1.0f;
        [SerializeField] private float sideAlpha = 0.45f;

        [Header("Auto Scroll")]
        [SerializeField] private bool autoScroll = false;
        [SerializeField] private float autoScrollInterval = 2.5f;
        [SerializeField] private AutoScrollDirection autoScrollDirection = AutoScrollDirection.RightToLeft;
        [SerializeField] private bool pauseAutoScrollWhileDragging = true;

        private RectTransform itemsRoot;
        private RectTransform itemModel;
        private RectTransform itemParent;

        private readonly List<object> datas = new();
        private readonly List<RectTransform> items = new();

        // 每个物理槽当前对应的“逻辑索引”（可超出 0~Count-1，loop 时再归一化）
        private readonly List<int> slotLogicalIndices = new();
        private readonly List<int> boundDataIndices = new();
        private readonly List<float> distanceBuffer = new();
        private readonly List<int> sortBuffer = new();

        private bool loop = false;
        private bool dragging = false;
        private bool noMove = false;
        private float autoTimer = 0f;

        private float currentIndex = 0f;
        private float targetIndex = 0f;

        // 当前窗口中心（整数）
        private int bindingCenterIndex = 0;
        private int currentShowIndex = 0;

        public Action<RectTransform, object, int> OnBindItem;
        public Action<RectTransform> OnClearItem;
        public Action<object, int> OnMoveUpdate;

        public int Count => datas.Count;
        public float SideScale { get => sideScale; set => sideScale = value; }
        public float SideYOffset { get => sideYOffset; set => sideYOffset = value; }
        public float CenterScale { get => centerScale; set => centerScale = value; }
        public float CenterAlpha { get => centerAlpha; set => centerAlpha = value; }
        public float SideAlpha { get => sideAlpha; set => sideAlpha = value; }

        private int TotalSlotCount => Mathf.Max(1, visibleSlotCount + bufferSlotCount);
        private int TotalHalf => TotalSlotCount / 2;

        private sealed class ItemReuseState : MonoBehaviour
        {
            public int BindVersion = 0;
            public int DataIndex = -1;
        }

        private void Awake()
        {
            if (itemsRoot == null)
                itemsRoot = transform as RectTransform;

            NormalizeSlotConfig();
        }

        public void SetContain(int a = 3, int b = 2)
        {
            visibleSlotCount = a;
            bufferSlotCount = b;
            NormalizeSlotConfig();

            if (itemModel == null)
                return;

            EnsureRuntimeItemCount(TotalSlotCount);

            if (datas.Count > 0)
                RebuildWindow(bindingCenterIndex, true);
            else
                RefreshVisual(true);
        }

        public void SetLoop(bool isLoop)
        {
            loop = isLoop;
            ClampIndices();

            if (datas.Count > 1)
                RebuildWindow(NormalizeDataIndex(Mathf.RoundToInt(currentIndex)), true);
            else
                RefreshVisual(true);
        }

        public void SetData<T>(IList<T> data, RectTransform model, float offsetX = 0f, bool keepIndex = false, int index = 0)
        {
            if (model == null)
            {
                Debug.LogError("UICoverFlow2.SetData: model is null.");
                return;
            }

            if (itemsRoot == null)
                itemsRoot = transform as RectTransform;

            NormalizeSlotConfig();

            if (itemModel != model)
            {
                itemModel = model;
                itemParent = model.parent as RectTransform;
                ClearRuntimeItems();
            }

            if (itemParent == null)
                itemParent = itemsRoot;

            itemModel.gameObject.SetActive(false);

            datas.Clear();
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                    datas.Add(data[i]);
            }

            EnsureRuntimeItemCount(datas.Count <=  1 ? datas.Count : TotalSlotCount);

            if (itemModel != null)
            {
                itemSpacing = itemModel.rect.width + offsetX;
                dragPixelsPerItem = Mathf.Max(1f, itemSpacing);
            }

            if (datas.Count == 0)
            {
                currentIndex = 0f;
                targetIndex = 0f;
                bindingCenterIndex = 0;
                currentShowIndex = 0;
                autoTimer = 0f;
                dragging = false;
                noMove = true;
                RefreshVisual(true);
                return;
            }

            if (!keepIndex)
            {
                int startIndex = NormalizeDataIndex(index);
                currentIndex = startIndex;
                targetIndex = startIndex;
                bindingCenterIndex = startIndex;
                currentShowIndex = startIndex;
            }
            else
            {
                ClampIndices();
                bindingCenterIndex = NormalizeDataIndex(Mathf.RoundToInt(currentIndex));
                currentShowIndex = NormalizeDataIndex(Mathf.RoundToInt(currentIndex));
            }

            dragging = false;
            noMove = true;
            autoTimer = 0f;

            RebuildWindow(bindingCenterIndex, true);
            RefreshVisual(true);
            OnMoveUpdate?.Invoke(datas[currentShowIndex], currentShowIndex);
        }

        public void RefreshData()
        {
            if (datas.Count == 0 || items.Count == 0)
                return;

            for (int i = 0; i < items.Count; i++)
                RebindSlotByLogicalIndex(i, slotLogicalIndices[i], true);

            RefreshVisual(true);
        }

        public void UpdateData<T>(int index, T data)
        {
            if (index < 0 || index >= datas.Count)
                return;

            datas[index] = data;

            for (int i = 0; i < items.Count; i++)
            {
                if (!IsLogicalIndexValid(slotLogicalIndices[i]))
                    continue;

                int dataIndex = NormalizeDataIndex(slotLogicalIndices[i]);
                if (dataIndex == index)
                    RebindSlotByLogicalIndex(i, slotLogicalIndices[i], true);
            }
        }

        public void JumpTo(int index, bool immediate = false)
        {
            if (datas.Count == 0)
                return;

            index = NormalizeDataIndex(index);
            targetIndex = index;

            if (immediate)
            {
                currentIndex = index;
                targetIndex = index;
                bindingCenterIndex = index;
                currentShowIndex = index;
                noMove = true;
                RebuildWindow(bindingCenterIndex, true);
                RefreshVisual(true);
                OnMoveUpdate?.Invoke(datas[currentShowIndex], currentShowIndex);
            }

            autoTimer = 0f;
        }

        public void MoveNext()
        {
            if (datas.Count <= 1)
                return;

            if (loop)
                targetIndex += 1f;
            else
                targetIndex = Mathf.Clamp(targetIndex + 1f, 0, datas.Count - 1);

            autoTimer = 0f;
        }

        public void MovePrev()
        {
            if (datas.Count <= 1)
                return;

            if (loop)
                targetIndex -= 1f;
            else
                targetIndex = Mathf.Clamp(targetIndex - 1f, 0, datas.Count - 1);

            autoTimer = 0f;
        }

        public int GetCenterIndex()
        {
            if (datas.Count == 0)
                return 0;

            return NormalizeDataIndex(Mathf.RoundToInt(currentIndex));
        }

        public void SetAutoScroll(bool enable)
        {
            autoScroll = enable;
            autoTimer = 0f;
        }

        public void SetAutoScrollInterval(float interval)
        {
            autoScrollInterval = Mathf.Max(0.1f, interval);
            autoTimer = 0f;
        }

        public void SetAutoScrollDirection(AutoScrollDirection direction)
        {
            autoScrollDirection = direction;
            autoTimer = 0f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (datas.Count <= 1)
                return;

            dragging = true;
            autoTimer = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (datas.Count <= 1)
                return;

            float deltaIndex = eventData.delta.x / Mathf.Max(1f, dragPixelsPerItem);
            currentIndex -= deltaIndex;
            targetIndex = currentIndex;

            if (!loop)
                ClampIndices();

            UpdateWindowByMovement();
            RefreshVisual(false);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (datas.Count <= 1)
                return;

            dragging = false;
            autoTimer = 0f;

            if (loop)
                targetIndex = Mathf.Round(currentIndex);
            else
                targetIndex = Mathf.Clamp(Mathf.Round(currentIndex), 0, datas.Count - 1);
        }

        public int GetBindVersion(RectTransform item)
        {
            if (item == null)
                return 0;

            var state = item.GetComponent<ItemReuseState>();
            return state != null ? state.BindVersion : 0;
        }

        public bool IsCurrentBinding(RectTransform item, int bindVersion, int dataIndex = -1)
        {
            if (item == null)
                return false;

            var state = item.GetComponent<ItemReuseState>();
            if (state == null)
                return false;

            if (state.BindVersion != bindVersion)
                return false;

            if (dataIndex >= 0 && state.DataIndex != dataIndex)
                return false;

            return true;
        }

        private void Update()
        {
            if (datas.Count == 0 || items.Count == 0 || (!autoScroll && !dragging && noMove && Mathf.Approximately(targetIndex, currentIndex)))
                return;

            NormalizeIndexIfNeeded();

            noMove = false;
            if (!dragging)
            {
                float t = 1f - Mathf.Exp(-snapSpeed * Time.unscaledDeltaTime);
                currentIndex = Mathf.Lerp(currentIndex, targetIndex, t);
                if (Mathf.Abs(currentIndex - targetIndex) < 0.001f)
                {
                    currentIndex = targetIndex;
                    noMove = true;
                }
            }

            UpdateAutoScroll();
            UpdateWindowByMovement();
            RefreshVisual(false);

            int showIndex = NormalizeDataIndex(Mathf.RoundToInt(currentIndex));
            if (currentShowIndex != showIndex && Mathf.Abs(currentIndex - targetIndex) < 0.1f)
            {
                currentShowIndex = showIndex;
                OnMoveUpdate?.Invoke(datas[currentShowIndex], currentShowIndex);
            }
        }

        private void NormalizeSlotConfig()
        {
            visibleSlotCount = Mathf.Max(1, visibleSlotCount);
            if (visibleSlotCount % 2 == 0)
                visibleSlotCount += 1;

            bufferSlotCount = Mathf.Max(0, bufferSlotCount);

            if ((visibleSlotCount + bufferSlotCount) % 2 == 0)
                bufferSlotCount += 1;
        }

        private void UpdateAutoScroll()
        {
            if (!autoScroll || datas.Count <= 1)
                return;

            if (dragging && pauseAutoScrollWhileDragging)
                return;

            autoTimer += Time.unscaledDeltaTime;
            if (autoTimer < autoScrollInterval)
                return;

            autoTimer = 0f;

            if (autoScrollDirection == AutoScrollDirection.RightToLeft)
                MoveNext();
            else
                MovePrev();
        }

        private void UpdateWindowByMovement()
        {
            if (datas.Count == 0 || items.Count == 0)
                return;

            while (currentIndex - bindingCenterIndex >= 1f)
            {
                bindingCenterIndex++;
                RecycleOneStepForward();
            }

            while (currentIndex - bindingCenterIndex <= -1f)
            {
                bindingCenterIndex--;
                RecycleOneStepBackward();
            }

            if (!loop)
                bindingCenterIndex = Mathf.Clamp(bindingCenterIndex, 0, Mathf.Max(0, datas.Count - 1));
        }

        private void RecycleOneStepForward()
        {
            int newMin = bindingCenterIndex - TotalHalf;
            int newMax = bindingCenterIndex + TotalHalf;

            int slotToRecycle = -1;
            int smallestLogical = int.MaxValue;

            for (int i = 0; i < slotLogicalIndices.Count; i++)
            {
                if (slotLogicalIndices[i] < newMin && slotLogicalIndices[i] < smallestLogical)
                {
                    smallestLogical = slotLogicalIndices[i];
                    slotToRecycle = i;
                }
            }

            if (slotToRecycle >= 0)
                RebindSlotByLogicalIndex(slotToRecycle, newMax, true);
        }

        private void RecycleOneStepBackward()
        {
            int newMin = bindingCenterIndex - TotalHalf;
            int newMax = bindingCenterIndex + TotalHalf;

            int slotToRecycle = -1;
            int largestLogical = int.MinValue;

            for (int i = 0; i < slotLogicalIndices.Count; i++)
            {
                if (slotLogicalIndices[i] > newMax && slotLogicalIndices[i] > largestLogical)
                {
                    largestLogical = slotLogicalIndices[i];
                    slotToRecycle = i;
                }
            }

            if (slotToRecycle >= 0)
                RebindSlotByLogicalIndex(slotToRecycle, newMin, true);
        }

        private void RebuildWindow(int centerIndex, bool force)
        {
            if (datas.Count == 0 || items.Count == 0)
                return;

            bindingCenterIndex = centerIndex;

            int startLogical = centerIndex - TotalHalf;
            for (int i = 0; i < items.Count; i++)
            {
                RebindSlotByLogicalIndex(i, startLogical + i, force);
            }
        }

        private void RefreshVisual(bool immediate)
        {
            if (datas.Count == 0 || items.Count == 0)
                return;

            sortBuffer.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                if (!IsLogicalIndexValid(slotLogicalIndices[i]))
                {
                    distanceBuffer[i] = float.MaxValue;
                    continue;
                }

                if (!items[i].gameObject.activeSelf)
                    items[i].gameObject.SetActive(true);

                float relative = GetRelativeOffset(slotLogicalIndices[i], currentIndex, datas.Count);
                ApplyVisualToItem(items[i], relative, i);
            }

            sortBuffer.Sort((a, b) => distanceBuffer[b].CompareTo(distanceBuffer[a]));
            // for (int i = 0; i < sortBuffer.Count; i++)
            // {
            //     items[sortBuffer[i]].SetSiblingIndex(i);
            // }
        }

        private void ApplyVisualToItem(RectTransform item, float relative, int slotIndex)
        {
            float abs = Mathf.Abs(relative);
            distanceBuffer[slotIndex] = abs;
            sortBuffer.Add(slotIndex);

            float normalized = Mathf.Clamp01(abs / Mathf.Max(1f, visibleSlotCount - 1));
            float eased = Mathf.Pow(normalized, falloff);

            float scale = Mathf.Lerp(centerScale, sideScale, eased);
            float alpha = Mathf.Lerp(centerAlpha, sideAlpha, eased);

            float x = relative * itemSpacing;
            float y = -abs * sideYOffset;

            item.anchoredPosition = new Vector2(x, y);
            item.localScale = Vector3.one * scale;
            item.DOKill();

            var raw = item.GetComponentInChildren<RawImage>(true);
            if (raw != null)
            {
                var c = raw.color;
                c.a = alpha;
                raw.color = c;
            }

            var image = item.GetComponentInChildren<Image>(true);
            if (image != null)
            {
                var c = image.color;
                c.a = alpha;
                image.color = c;
            }
        }

        private void RebindSlotByLogicalIndex(int slotIndex, int logicalIndex, bool force)
        {
            if (slotIndex < 0 || slotIndex >= items.Count)
                return;

            slotLogicalIndices[slotIndex] = logicalIndex;

            if (!IsLogicalIndexValid(logicalIndex))
            {
                DeactivateSlot(slotIndex);
                return;
            }

            int dataIndex = NormalizeDataIndex(logicalIndex);
            BindSlot(slotIndex, dataIndex, force);
            items[slotIndex].gameObject.SetActive(true);
        }

        private void BindSlot(int slotIndex, int dataIndex, bool force)
        {
            if (slotIndex < 0 || slotIndex >= items.Count)
                return;
            if (dataIndex < 0 || dataIndex >= datas.Count)
                return;

            if (!force && boundDataIndices[slotIndex] == dataIndex)
                return;

            RectTransform item = items[slotIndex];
            var reuseState = GetOrAddReuseState(item);
            reuseState.BindVersion++;
            reuseState.DataIndex = dataIndex;

            OnClearItem?.Invoke(item);

            boundDataIndices[slotIndex] = dataIndex;
            item.name = $"{itemModel.name}_{dataIndex}";

            OnBindItem?.Invoke(item, datas[dataIndex], dataIndex);
        }

        private void DeactivateSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Count)
                return;

            var item = items[slotIndex];
            if (item == null)
                return;

            if (boundDataIndices[slotIndex] != -1)
            {
                var reuseState = GetOrAddReuseState(item);
                reuseState.BindVersion++;
                reuseState.DataIndex = -1;
                OnClearItem?.Invoke(item);
            }

            boundDataIndices[slotIndex] = -1;
            item.gameObject.SetActive(false);
        }

        private ItemReuseState GetOrAddReuseState(RectTransform item)
        {
            var state = item.GetComponent<ItemReuseState>();
            if (state == null)
                state = item.gameObject.AddComponent<ItemReuseState>();
            return state;
        }

        private void EnsureRuntimeItemCount(int count)
        {
            while (items.Count < count)
            {
                var obj = Instantiate(itemModel, itemParent);
                obj.gameObject.SetActive(true);

                items.Add(obj);
                slotLogicalIndices.Add(0);
                boundDataIndices.Add(-1);
                distanceBuffer.Add(0f);
            }

            while (items.Count > count)
            {
                int last = items.Count - 1;
                if (items[last] != null)
                    Destroy(items[last].gameObject);

                items.RemoveAt(last);
                slotLogicalIndices.RemoveAt(last);
                boundDataIndices.RemoveAt(last);
                distanceBuffer.RemoveAt(last);
            }
        }

        private void ClearRuntimeItems()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                    Destroy(items[i].gameObject);
            }

            items.Clear();
            slotLogicalIndices.Clear();
            boundDataIndices.Clear();
            distanceBuffer.Clear();
        }

        private bool IsLogicalIndexValid(int logicalIndex)
        {
            if (datas.Count <= 0)
                return false;

            if (loop)
                return true;

            return logicalIndex >= 0 && logicalIndex < datas.Count;
        }

        private int NormalizeDataIndex(int index)
        {
            if (datas.Count <= 0)
                return 0;

            if (loop)
            {
                int mod = index % datas.Count;
                if (mod < 0)
                    mod += datas.Count;
                return mod;
            }

            return Mathf.Clamp(index, 0, datas.Count - 1);
        }

        private float GetRelativeOffset(float logicalIndex, float curIndex, int count)
        {
            float raw = logicalIndex - curIndex;

            if (!loop || count <= 0)
                return raw;

            float half = count * 0.5f;

            while (raw > half)
                raw -= count;

            while (raw < -half)
                raw += count;

            return raw;
        }

        private void NormalizeIndexIfNeeded()
        {
            if (!loop || datas.Count <= 0)
                return;

            if (Mathf.Abs(currentIndex) <= 10000f && Mathf.Abs(targetIndex) <= 10000f && Mathf.Abs(bindingCenterIndex) <= 10000)
                return;

            int count = datas.Count;

            currentIndex = WrapFloat(currentIndex, count);
            targetIndex = WrapFloat(targetIndex, count);

            int offset = Mathf.FloorToInt((float)bindingCenterIndex / count) * count;
            bindingCenterIndex -= offset;

            for (int i = 0; i < slotLogicalIndices.Count; i++)
                slotLogicalIndices[i] -= offset;
        }

        private float WrapFloat(float value, int count)
        {
            if (count <= 0)
                return 0f;

            value %= count;
            if (value < 0)
                value += count;
            return value;
        }

        private void ClampIndices()
        {
            if (datas.Count <= 0)
            {
                currentIndex = 0f;
                targetIndex = 0f;
                bindingCenterIndex = 0;
                return;
            }

            if (loop)
            {
                currentIndex = WrapFloat(currentIndex, datas.Count);
                targetIndex = WrapFloat(targetIndex, datas.Count);
            }
            else
            {
                currentIndex = Mathf.Clamp(currentIndex, 0, datas.Count - 1);
                targetIndex = Mathf.Clamp(targetIndex, 0, datas.Count - 1);
                bindingCenterIndex = Mathf.Clamp(bindingCenterIndex, 0, datas.Count - 1);
            }
        }
    }
}