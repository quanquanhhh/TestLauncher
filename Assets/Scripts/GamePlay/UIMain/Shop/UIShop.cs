using System.Collections.Generic;
using Foundation;
using GameConfig;
using GamePlay.UIMain.Widget;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.UIMain.Shop
{
    [Window("UIShop", WindowLayer.Popup)]
    public class UIShop : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;
        
        [UIBinder("ShopType1")] private Transform shopType1;
        [UIBinder("ShopType2")] private Transform shopType2;

        [UIBinder("Coin")] private Transform coinwidget;
        [UIBinder("Diamond")] private Transform diamondwidget;
        [UIBinder("Content","")] private RectTransform _rectTransform;
        [UIBinder("Shop")] private RectTransform shopUI;
        
        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(Close);
            shopType1.gameObject.SetActive(false);
            shopType2.gameObject.SetActive(false);

            var scl = ViewUtility.GetEnoughXScale();
            shopUI.localScale = Vector3.one * scl;
            CreateShopPage();

            AddWidget<CoinWidget>(coinwidget.gameObject);
            AddWidget<DiamondWidget>(diamondwidget.gameObject);
            _rectTransform.offsetMax -= new Vector2( 0,ViewUtility.AdjustTopHeight);
            
        }
 

        private void CreateShopPage()
        { 
            var shopinfos = new List<GameConfig.Shop>(GameConfigSys.shop);
            List<ShopPack> pages = new();

            int index = 0;
            while (shopinfos.Count > 0)
            {
                
                var info = shopinfos[index];
                if (!info.isShow)
                {
                    shopinfos.Remove(info);
                    continue;
                }
                var infos = shopinfos.FindAll(x => x.isShow && x.ShowType == info.ShowType && x.group == info.group);
                
                var page = CreatePackType(infos, info.ShowType);

                foreach (var i in infos)
                {
                    shopinfos.Remove(i);
                }
                pages.Add(page);
            } 
            foreach (var p in pages)
            {
                p.gameObject.SetActive(true);
            }
        }

        private ShopPack CreatePackType(List<GameConfig.Shop> info, int showType)
        {
            var model = showType == 1 ? shopType1 : shopType2;
            var pack = GameObject.Instantiate(model, model.parent); 
            var item = AddWidget<ShopPack>(pack.gameObject, false, info);
            return item;
        }

        public void ForceRebuild()
        {
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(shopUI);
            
        }
    }

    public class ShopPack : UIWidget
    {
        [UIBinder("Pack")] private Transform model;
        [UIBinder("tittleText")] private TextMeshProUGUI  tittleText;
        private List<GameConfig.Shop> infos;
        public override void OnCreate()
        {
            base.OnCreate();
            infos = (List<GameConfig.Shop>)userDatas[0];
            model.gameObject.SetActive(false);
            
           
            tittleText.SetText(infos[0].group.ToString());
            CreatePackItem();

        }

        private void CreatePackItem()
        {
            for (int index = 0; index < infos.Count; index++)
            {
                var info = infos[index];
                var obj = GameObject.Instantiate(model,model.parent);
                obj.name = info.id.ToString(); 
                if (info.ShowType == 1)
                {
                    AddWidget<ShopPackItem1>(obj.gameObject, true, info);
                }
                else
                { 
                    if (info.buyItem[0] == (int)ItemType.Photo)
                    {
                        bool enough = GameConfigSys.CheckEnoughShopPhoto(info.buyCount[0]);
                        if(!enough) continue;
                    } 
                    AddWidget<ShopPackItem2>(obj.gameObject, true, info);
                }
            }
        }
        
    }
    public class ShopPackItem1 : UIWidget
    {
        [UIBinder("Reward")] private Transform reward;
        [UIBinder("item")] private Transform model;
        [UIBinder("PackName")] private TextMeshProUGUI packName;
        [UIBinder("BuyButton")] private Transform buttonGroup;
        
        private GameConfig.Shop info;
        private Transform _phototransforms;
        public override async void OnCreate()
        {
            base.OnCreate();
            info = (GameConfig.Shop)userDatas[0];
            
            for (int index = 0; index < info.buyItem.Count; index++)
            {
                if (index == 0)
                {
                    ChangeSpineOrImage(reward.Find("main"), info.buyItem[index], info.buyCount[index]);
                }
                else
                {
                    if (info.buyItem[index] == (int)ItemType.Photo)
                    {
                        bool enough = GameConfigSys.CheckEnoughShopPhoto(info.buyCount[index]);
                        if(!enough) continue;
                    }
                    var obj = GameObject.Instantiate(model, model.parent);
                    ChangeSpineOrImage(obj, info.buyItem[index], info.buyCount[index]);
                    if (info.buyItem[index] == (int)ItemType.Photo)
                    {
                        _phototransforms = obj;
                    }
                }
            }
            packName.SetText(info.name);
            model.gameObject.SetActive(false);
            SubScribeEvent<PhotoInShopCountChange>(OnPhotoInShopCountChange);
            
            AddWidget<BuyButton>(buttonGroup.gameObject, true, info);
        }

        private void OnPhotoInShopCountChange(PhotoInShopCountChange obj)
        {
            for (int i = 0; i < info.buyCount.Count; i++)
            {
                int item = info.buyItem[i];
                if (item == (int)ItemType.Photo)
                {
                    bool enough = GameConfigSys.CheckEnoughShopPhoto(info.buyCount[i]);
                    _phototransforms.gameObject.SetActive(enough);
                }
            }
            
        }

        private async void ChangeSpineOrImage(Transform trans, int item, int count)
        {
            trans.Find("icon").gameObject.SetActive(item == (int)ItemType.Photo);
            trans.Find("icon_spine").gameObject.SetActive(item != (int)ItemType.Photo);
            if (item != (int)ItemType.Photo)
            {
                ChangeItemSpine(trans, item);
            }
            else
            {
                trans.Find("icon").GetComponent<Image>().sprite = GUtility.GetItemIcon((ItemType)item); 
            }
            trans.Find("amount").GetComponent<TextMeshProUGUI>().text = count.ToString();
        }

        private async void ChangeItemSpine(Transform trans, int item)
        {
            
            await trans.Find("icon_spine").GetComponent<SkeletonGraphic>().ChangeDataAsset("item_icon_"+item);
            trans.Find("icon_spine").GetComponent<SkeletonGraphic>().PlayAsync("animation", true);
        }
    }
    
    public class ShopPackItem2 : UIWidget
    {
        [UIBinder("icon")] private Image img;
        [UIBinder("icon_spine")] private SkeletonGraphic spine;
        [UIBinder("amount")] private TextMeshProUGUI amount;
        [UIBinder("BuyButton")] private Transform buttonGroup;
        private GameConfig.Shop info; 
        public override void OnCreate()
        {
            base.OnCreate();
            info = (GameConfig.Shop)userDatas[0];
            string fix = ""; 
            ChangeSpineOrImage(info.buyItem[0], info.buyCount[0]);
            AddWidget<BuyButton>(buttonGroup.gameObject, true, info);
            if (info.buyItem[0] == (int)ItemType.Photo)
            {
                SubScribeEvent<PhotoInShopCountChange>(OnPhotoInShopCountChange);
            }
        }

        private void OnPhotoInShopCountChange(PhotoInShopCountChange obj)
        {
            bool enough = GameConfigSys.CheckEnoughShopPhoto(info.buyCount[0]);
            gameObject.SetActive(enough);
            if (!enough)
            {
                UIModule.Instance.Get<UIShop>().ForceRebuild();
            }
        }

        private async void ChangeSpineOrImage( int item, int count)
        {
            
            img.gameObject.SetActive(item == (int)ItemType.Photo);
            spine.gameObject.SetActive(item != (int)ItemType.Photo);
            if (item != (int)ItemType.Photo)
            {
                ChangeItemSpine(item); 
            }
            else
            {
                img.sprite = GUtility.GetItemIcon((ItemType)item); 
            }
            amount.text = count.ToString();
        }
        private async void ChangeItemSpine( int item)
        {
            
            await spine.ChangeDataAsset("item_icon_"+item);
            spine.PlayAsync("animation", true);
        }
    }

    public class BuyButton : UIWidget
    {
        [UIBinder("BuyBtnAds")] private Button adsBuy;
        [UIBinder("BuyBtnCoin")] private Button coinBuy;
        [UIBinder("BuyBtnMoney")] private Button purchers; 
        private GameConfig.Shop info;

        public override void OnCreate()
        {
            base.OnCreate();
            info = (GameConfig.Shop)userDatas[0];
            
            
            adsBuy.gameObject.SetActive(info.buyType == 0);
            coinBuy.gameObject.SetActive(info.buyType == 1 || info.buyType == 2);
            purchers.gameObject.SetActive(info.buyType == 3);
 
            if (info.buyType == 1 || info.buyType == 2)
            {
                coinBuy.transform.Find("icon").GetComponent<Image>().sprite = GUtility.GetItemIcon((ItemType)info.buyType); 
                coinBuy.transform.Find("price").GetComponent<TextMeshProUGUI>().SetText(info.price.ToString());
                coinBuy.onClick.AddListener(CoinClaim);
            }
            else if (info.buyType == 3)
            { 
                //iap表拿价
                string str = IAPManager.Instance.GetLocalizedPrice(info.buyKey);
                purchers.transform.Find("price").GetComponent<TextMeshProUGUI>().SetText(str);
                purchers.onClick.AddListener(BuyItems);
            }
            else
            {
                adsBuy.onClick.AddListener(AdClaim);
            }
 
        }
        
        private void BuyItems()
        { 
            GameIAP.Purchase(info.buyKey, Claim,"shop"); 
        }

        private void CoinClaim()
        { 
            if (GUtility.IsEnoughItem((ItemType)info.buyType, info.price))
            {
                Event.Instance.SendEvent(new SubItem((int)ItemType.Coin, info.price));
                Claim();
            }
        }

 
        private void Claim()
        {
            for (int index = 0; index < info.buyItem.Count; index++)
            {
                int item = info.buyItem[index];
                int amount = info.buyCount[index];
                if (item == (int)ItemType.Photo && GameConfigSys.CheckEnoughShopPhoto(amount))
                {
                    Event.Instance.SendEvent(new AddPhoto((int)PhotoType.Shop, amount));
                    
                    Event.Instance.SendEvent(new PhotoInShopCountChange());
                }
                else
                {
                    Event.Instance.SendEvent(new AddItem(item, amount, rectTransform.position));
                }
            }

            if (GameFsm.Instance.InGame())
            {
                Event.Instance.SendEvent(new UpdatePropCount());
            }
            
        }
        private void AdClaim()
        { 
            AdMgr.Instance.PlayRV(Claim, "Shop_"+info.name);
        }
    }
}