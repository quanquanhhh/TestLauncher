using System;
using System.Collections.Generic;
using Foundation;
using Foundation.GridViewLoop;
using GameConfig;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Activity
{
    [Window("SecretView", WindowLayer.Popup)]
    public class SecretView2 : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Content")] private RectTransform _content;
        [UIBinder("LoopContent")] private RectTransform _loopContent;
        [UIBinder("Photos")] private GameObject photosTrans;

        private List<SecretGift> info;
        private GridView2 _gridView;

        private bool _hasTopItem;
        private int _rowCount;

        public override void OnCreate()
        {
            base.OnCreate();

            // _loopContent.localScale = Vector3.one * ViewUtility.GetEnoughXScale();
            _content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            info = GameConfigSys.GetSecretGiftInfo();

            closeBtn.onClick.AddListener(Close);

            SetUpGrid();
        }

        private void SetUpGrid()
        {
            _gridView = photosTrans.TryGetOrAddComponent<GridView2>();

            _gridView.ItemPrefabDataList.Clear();

            var sx = ViewUtility.GetEnoughXScale();
            _gridView.SetItemScale = sx;
            for (int i = 0; i < 2; i++)
            { 
                var model = _loopContent.Find("Item" + (i + 1)).gameObject; 
                
                
                model.TryGetOrAddComponent<MonoCustomDataProxy>();
                model.SetActive(false); // 模板隐藏

                GridViewItemPrefabConfData itemData = new GridViewItemPrefabConfData
                {
                    mItemPrefab = model,
                    mInitCreateCount = 1
                };

                _gridView.ItemPrefabDataList.Add(itemData);
            }

            RefreshGrid(true);
        }

        private void RefreshGrid(bool resetPos = false)
        {
            var leftName = GameConfigSys.CheckSecretBuy();
            bool oldHasTopItem = _hasTopItem;
            _hasTopItem = leftName.Count >= 1;

            int normalRowCount = Mathf.CeilToInt(info.Count / 2f);
            _rowCount = normalRowCount + (_hasTopItem ? 1 : 0);

            if (!_gridView.enabled)
                _gridView.enabled = true;

            var initParam = new GridView2.InitParam
            {
                FixedColumnCount = 1,
                Padding = new RectOffset(0, 0, 0, 0),
                Spacing = new Vector2(0, 8),
                RecyclePadding = new Vector2(0, 100),
                ForceTopLeftAnchor = false
            };

            if (resetPos || _gridView.ItemTotalCount == 0)
            {
                _gridView.InitGridView(
                    _rowCount,
                    GetPrefabNameByIndex,
                    OnBindItemByIndex,
                    initParam
                );
            }
            else
            {
                _gridView.SetListItemCount(_rowCount, false);

                if (oldHasTopItem != _hasTopItem)
                    _gridView.RebuildLayoutAndVisibleItems(true);
                else
                    _gridView.RefreshAllShownItems();
            }
        }

        public void UpdateLoopcell()
        {
            RefreshGrid(false);
        }

        private string GetPrefabNameByIndex(GridView2 gridView, int index)
        {
            if (_hasTopItem && index == 0)
                return "Item1";

            return "Item2";
        }

        private void OnBindItemByIndex(GridView2 gridView, LoopGridViewItem item, int index)
        {
            if (index < 0 || index >= _rowCount || item == null)
                return;

            if (_hasTopItem && index == 0) //top
            {
                var proxy = item.GetComponent<MonoCustomDataProxy>();
                var gift = proxy.GetCustomData<SecretGiftItem>();
                if (gift == null)
                {
                    gift = new SecretGiftItem(item.gameObject);
                    proxy.SetCustomData(gift);
                }

                gift.UpdateContent();
                return;
            }

            int dataRow = _hasTopItem ? index - 1 : index;
            bool title =  dataRow == 0;
            int first = dataRow * 2;
            int second = first + 1;

            if (first < 0 || first >= info.Count)
                return;

            SecretGift finfo = info[first];
            SecretGift sinfo = second < info.Count ? info[second] : null;

            var proxy2 = item.GetComponent<MonoCustomDataProxy>();
            var cell = proxy2.GetCustomData<SecretGiftCell>();
            if (cell == null)
            {
                cell = new SecretGiftCell(item.gameObject);
                proxy2.SetCustomData(cell);
            }

            cell.UpdateContent(index, finfo, sinfo,title);
        }
    }
}