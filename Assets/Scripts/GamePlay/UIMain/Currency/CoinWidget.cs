using DG.Tweening;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Shop;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain.Widget
{
    public class CoinWidget : UIWidget
    {

        [UIBinder("Icon")] public Transform icon;
        [UIBinder("Btn")] private Button buybtn;
        [UIBinder("Text")] private TextMeshProUGUI text;
        [UIBinder("")] private Canvas canvas; 
        private int oldOrder;

        private int current = 0;
        private int showCount = 0;
        public override void OnCreate()
        {
            base.OnCreate();
            int count = StorageManager.Instance.GetStorage<BaseInfo>().Coin;
            text.text = count.GetCommaFormat();
            current = count;
            showCount = count;
            
            if (userDatas.Length > 0)
            {
                oldOrder = (int)userDatas[0];
                
                if (canvas != null)
                {
                    canvas.sortingOrder = oldOrder + 1;
                    SubScribeEvent<ChangeTopUIOrder>(OnChangeTopUIOrder);
                }
            }
            
            CurrencyWidgetDic.AddCoinWidget(this); 
            
            SubScribeEvent<ItemCountChangeShow>(OnChangeAmount);
            if (buybtn != null)
            {
                buybtn.onClick.AddListener(OpenShop);
            }
            
        }

        private void OpenShop()
        {
            UIModule.Instance.ShowAsync<UIShop>();
        }

        private void OnChangeAmount(ItemCountChangeShow obj)
        {
            if (obj.itemType == (int)ItemType.Coin)
            {
                int count = StorageManager.Instance.GetStorage<BaseInfo>().Coin;
                var targetwidget = CurrencyWidgetDic.GetCurCoinWidget();
                if (count == current)
                {
                    return;
                }
                else if ((targetwidget != null && targetwidget != this) || count < current)
                {
                    text.text = count .GetCommaFormat(); 
                    showCount = count;
                    current = count;
                }
                else
                {
                    if (obj.isFly)
                    {
                        var startpos = obj.flystartPos;
                        if (canvas)
                        {
                            canvas.sortingOrder = GUtility.TopUISorting;
                        }
                        FlyUtilty.FlyItem((int)ItemType.Coin, 100, startpos, icon.position,0.8f, UpdateCoinShow, () =>
                        {
                            canvas.sortingOrder = oldOrder + 1;
                        });
                    }
                    else
                    {
                        UpdateCoinShow();
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            CurrencyWidgetDic.DelCoinWidget(this);
            base.OnDestroy();
        }

        private void OnChangeTopUIOrder(ChangeTopUIOrder obj)
        {
            if (canvas== null)
            {
                return;
            }
            if (obj.hasCoinWidget && obj.isTop)
            {
                canvas.sortingOrder = GUtility.TopUISorting;
            }
            else if (!obj.isTop)
            {
                canvas.sortingOrder = oldOrder;
            }
        }

 
        private void UpdateCoinShow()
        {
            int count = StorageManager.Instance.GetStorage<BaseInfo>().Coin;
            
            int target = count;
            int v = showCount;
            current = count;
            DOTween.To(() => v, (x) =>
            {
                v = x;
                text.text = v.GetCommaFormat();
                showCount = v;
            }, target, 1.5f).OnComplete(() =>
            {
                text.text = target.GetCommaFormat(); 
                showCount = v;
            });
            
        }
    }
}