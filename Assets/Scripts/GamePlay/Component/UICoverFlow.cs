using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePlay.Component
{
    [DisallowMultipleComponent]
    public class UICoverFlow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public enum AutoScrollDirection
        {
            RightToLeft,   // 自动往左滚（显示下一个）
            LeftToRight    // 自动往右滚（显示上一个）
        }
 
        private RectTransform itemsRoot;
        private List<RectTransform> items = new List<RectTransform>(); 
        private bool loop = false;
        private float falloff = 0.5f; 
        private float itemSpacing = 260f;
        private float dragPixelsPerItem = 260f;
        private float snapSpeed = 12f;
        
        [Header("Layout")]  
        [SerializeField] private float sideYOffset = 18f;
        [SerializeField] private float centerScale = 1.0f;
        [SerializeField] private float sideScale = 0.72f;
        [SerializeField] private float centerAlpha = 1.0f;
        [SerializeField] private float sideAlpha = 0.45f;

        public float SideScale
        {
            get{return sideScale;}
            set{sideScale = value;}
        }
 
        public float SideYOffset
        {
            get{return sideYOffset;}
            set{sideYOffset = value;}
        }
 

        [Header("Auto Scroll")]
        [SerializeField] private bool autoScroll = false;
        [SerializeField] private float autoScrollInterval = 2.5f;
        [SerializeField] private AutoScrollDirection autoScrollDirection = AutoScrollDirection.RightToLeft;
        [SerializeField] private bool pauseAutoScrollWhileDragging = true;
 
        private readonly List<int> _sortBuffer = new List<int>();
        private readonly List<float> _distanceBuffer = new List<float>();

        private string index ;
        private bool _dragging;
        private float _autoTimer;

        // 当前显示中心的“浮点索引”
        private float _currentIndex;
        private float _targetIndex;

        public int Count => items.Count;

        public void SetLoop(bool isloop)
        {
            loop = isloop;
        }

        public void SetContent(List<RectTransform> items, float offsetx = 0)
        {
            this.items = items;
            RefreshVisual(true);
            itemSpacing =   items[0].rect.width + offsetx;
            dragPixelsPerItem = itemSpacing;
            index = gameObject.name + "_";
        }
        
        private void Awake()
        {
            if (itemsRoot == null)
                itemsRoot = transform as RectTransform;
            index = gameObject.name;
            _autoTimer = 0f;
            RefreshVisual(true);
        }
 

        private void Update()
        {
            if (items.Count == 0)
                return;
            NormalizeIndexIfNeeded();

            if (!_dragging)
            {
                float t = 1f - Mathf.Exp(-snapSpeed * Time.unscaledDeltaTime);
                _currentIndex = Mathf.Lerp(_currentIndex, _targetIndex, t);

                if (Mathf.Abs(_currentIndex - _targetIndex) < 0.001f)
                    _currentIndex = _targetIndex;
            }

            UpdateAutoScroll();
            RefreshVisual(false);
        }

        private void UpdateAutoScroll()
        {
            if (!autoScroll || items.Count <= 1)
                return;

            if (_dragging && pauseAutoScrollWhileDragging)
                return;

            _autoTimer += Time.unscaledDeltaTime;
            if (_autoTimer < autoScrollInterval)
                return;

            _autoTimer = 0f;

            if (autoScrollDirection == AutoScrollDirection.RightToLeft)
                MoveNext();
            else
                MovePrev();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (items.Count <= 1)
                return;

            _dragging = true;
            _autoTimer = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (items.Count <= 1)
                return;

            float deltaIndex = eventData.delta.x / Mathf.Max(1f, dragPixelsPerItem);

            // 手指向右拖，内容往右移动，中心索引减小
            _currentIndex -= deltaIndex;
            _targetIndex = _currentIndex;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (items.Count <= 1)
                return;

            _dragging = false;
            _autoTimer = 0f;

            if (loop)
            {
                _targetIndex = Mathf.Round(_currentIndex);
            }
            else
            {
                _targetIndex = Mathf.Clamp(Mathf.Round(_currentIndex), 0, items.Count - 1);
            }
        }
  
        private void RefreshVisual(bool immediate)
        {
            int count = items.Count;
            if (count == 0)
                return;

            _distanceBuffer.Clear();
            _sortBuffer.Clear();

            for (int i = 0; i < count; i++)
            {
                float relative = GetRelativeOffset(i, _currentIndex, count);
                float abs = Mathf.Abs(relative);

                _distanceBuffer.Add(abs);
                _sortBuffer.Add(i);

                float normalized = Mathf.Clamp01(abs / 2f);
                float eased = Mathf.Pow(normalized, falloff);

                float scale = Mathf.Lerp(centerScale, sideScale, eased);
                float alpha = Mathf.Lerp(centerAlpha, sideAlpha, eased);

                float x = relative * itemSpacing;
                float y = -abs * sideYOffset;

                RectTransform item = items[i];
                if (item == null)
                {
                    continue;
                }
                item.anchoredPosition = new Vector2(x, y);
                item.localScale = Vector3.one * scale;
                item.DOKill();
                item.GetComponentInChildren<RawImage>().color = new Color(alpha, alpha, alpha);
            }

            // 离中心越远越先放到底层，中心卡放最上层
            _sortBuffer.Sort((a, b) => _distanceBuffer[b].CompareTo(_distanceBuffer[a]));
            for (int i = 0; i < _sortBuffer.Count; i++)
            {
                items[_sortBuffer[i]].SetSiblingIndex(i);
            }
        }
        private float GetRelativeOffset(int itemIndex, float currentIndex, int count)
        {
            float raw = itemIndex - currentIndex;

            if (!loop)
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
            if (!loop || items.Count <= 0)
                return;

            if (Mathf.Abs(_currentIndex) > 10000f || Mathf.Abs(_targetIndex) > 10000f)
            {
                _currentIndex = WrapFloat(_currentIndex, items.Count);
                _targetIndex = WrapFloat(_targetIndex, items.Count);
            }
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

        public void MoveNext()
        {
            if (items.Count <= 1)
                return;

            if (loop)
                _targetIndex += 1f;
            else
                _targetIndex = Mathf.Clamp(_targetIndex + 1f, 0, items.Count - 1);

            _autoTimer = 0f;
        }

        public void MovePrev()
        {
            if (items.Count <= 1)
                return;

            if (loop)
                _targetIndex -= 1f;
            else
                _targetIndex = Mathf.Clamp(_targetIndex - 1f, 0, items.Count - 1);

            _autoTimer = 0f;
        }
 

        private int Mod(int value, int mod)
        {
            int r = value % mod;
            return r < 0 ? r + mod : r;
        }

        public int GetCenterIndex()
        {
            if (items.Count == 0)
                return 0;

            int index = Mathf.RoundToInt(_currentIndex);
            return Mod(index, items.Count);
        }

        public void SetAutoScroll(bool enable)
        {
            autoScroll = enable;
            _autoTimer = 0f;
        }

        public void SetAutoScrollInterval(float interval)
        {
            autoScrollInterval = Mathf.Max(0.1f, interval);
            _autoTimer = 0f;
        }

        public void SetAutoScrollDirection(AutoScrollDirection direction)
        {
            autoScrollDirection = direction;
            _autoTimer = 0f;
        }
        
        public void JumpTo(int index, bool immediate = false)
        {
            if (items == null || items.Count == 0)
                return;

            if (loop)
            {
                index = Mod(index, items.Count);
            }
            else
            {
                index = Mathf.Clamp(index, 0, items.Count - 1);
            }

            _targetIndex = index;

            if (immediate)
            {
                _currentIndex = _targetIndex;
                RefreshVisual(true);
            }

            _autoTimer = 0f;
        }

        public void JumpToByTween(int index)
        {
            if (items == null || items.Count == 0)
                return;

            if (loop)
                index = Mod(index, items.Count);
            else
                index = Mathf.Clamp(index, 0, items.Count - 1);

            _targetIndex = index;
            _autoTimer = 0f;
        }
        
    }
}