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
    public class DiamondWidget : UIWidget
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
            
            int count = StorageManager.Instance.GetStorage<BaseInfo>().Diamond;
            text.text = count.GetCommaFormat();
            current = count;
            showCount = count;
            
            
            if (userDatas.Length>0)
            {
                oldOrder = (int)userDatas[0];
                if (canvas != null)
                {
                    canvas.overrideSorting =  true;
                    canvas.sortingOrder = oldOrder + 1;
                    SubScribeEvent<ChangeTopUIOrder>(OnChangeTopUIOrder);
                }
            }
            CurrencyWidgetDic.AddDiamondWidget(this);
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

        public override void OnDestroy()
        {
            CurrencyWidgetDic.DelDiamondWidget(this);
            base.OnDestroy();
        }

        private void OnChangeTopUIOrder(ChangeTopUIOrder obj)
        {
            if (canvas== null)
            {
                return;
            }
            if (obj.hasDiamondWidget && obj.isTop)
            {
                canvas.sortingOrder = GUtility.TopUISorting;
            }
            else if (!obj.isTop)
            {
                canvas.sortingOrder = oldOrder;
            }
        }
        
        private void OnChangeAmount(ItemCountChangeShow obj)
        {
            if (obj.itemType == (int)ItemType.Diamond)
            {
                int count = StorageManager.Instance.GetStorage<BaseInfo>().Diamond;
                var targetwidget = CurrencyWidgetDic.GetCurDiamondWidget();
                if (count == current)
                {
                    return;
                }
                else if ((targetwidget != null && targetwidget != this) ||count < current)
                {
                    
                    text.text = count .GetCommaFormat(); 
                    showCount = count;
                    current = count;
                }
                else
                {
                    if (obj.isFly)
                    {
                        if (canvas != null)
                        {
                            canvas.sortingOrder = GUtility.TopUISorting;
                        }
                        FlyUtilty.FlyItem((int)ItemType.Diamond, 100, Vector3.zero, icon.position,0.8f, UpdateDiamondShow,
                            () =>
                            {
                                canvas.sortingOrder = oldOrder + 1;
                            });
                    }
                    else
                    {
                        UpdateDiamondShow();
                    }
                }
            }
        }
        private void OnShowItem(AddItem obj)
        {
            if (obj.itemType == (int)ItemType.Diamond && obj.itemCount != 0)
            {
                FlyUtilty.FlyItem((int)ItemType.Diamond, 100, Vector3.zero, icon.position,0.8f, UpdateDiamondShow);
            }
        }

        private void UpdateDiamondShow( )
        {
            int count = StorageManager.Instance.GetStorage<BaseInfo>().Diamond;
            int target = count;
            current = count;
            
            int v = showCount;
             DOTween.To(() => v, (x) =>
            {
                v = x;
                text.text = v.GetCommaFormat();
                showCount = v;
            }, target, 1.0f).OnComplete(() =>
            {
                text.text = target.GetCommaFormat(); 
                showCount = v;
            });
            
        }
    }
}