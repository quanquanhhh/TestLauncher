using System;
using System.Collections.Generic;
using Foundation;
using Foundation.GridViewLoop;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Spine.Event;

namespace GamePlay.Activity
{
    [Window("SecretViewPhotos", WindowLayer.Popup)]
    public class SecretViewPhotos : UIWindow
    {
        [UIBinder("CloseBtn")] private Button CloseBtn;
        [UIBinder("Photos")] private GameObject photos;
        [UIBinder("Title")] private TextMeshProUGUI title;
        [UIBinder("BuyBtn")] private Button buyBtn;
        [UIBinder("price")] private  TextMeshProUGUI price;
        [UIBinder("Content")] private  RectTransform _content;
        private SecretGift config;
        private SecretViewPhotosLoop _loop;
        private bool isbuy;
        private string currentKey;
        private List<string> names;
        private SecretGiftCell fromCell;
        public override void OnCreate()
        {
            base.OnCreate();
            names = (List<string>) userDatas[0];
            config = (SecretGift)userDatas[1];
            fromCell = (SecretGiftCell)userDatas[2];
            title.text = config.name;
           _loop = AddWidget<SecretViewPhotosLoop>(photos, true,names, config.name);

            buyBtn.onClick.AddListener(BuyFun);
            price.text = IAPManager.Instance.GetLocalizedPrice(config.BuyKey);
            isbuy = StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys.ContainsKey(config.name) &&
                    StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[config.name];
            CloseBtn.onClick.AddListener(CloseFun);
            currentKey = config.BuyKey;
            buyBtn.gameObject.SetActive(!isbuy);
            _content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            
            
            var dic = new Dictionary<string, object>();
            dic["name"] = config.name;
            TBAMgr.Instance.SendLogEvent("secretshow",dic);
        }

        private void CloseFun()
        {
            isbuy = StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys.ContainsKey(config.name) &&
                    StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.Buys[config.name];
            if (isbuy)
            {
                Close();
                return;
            }
            else 
            {
                bool isToday =
                    DateTimeOffset.FromUnixTimeSeconds(StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.LastShowDiscountTime).LocalDateTime.Date
                    == DateTime.Now.Date;
                if (isToday)
                {
                    Close();
                    return;
                }
                UIModule.Instance.ShowAsync<SecretKeep>(config.DiscountKey, config.name);
            }
            
        }

        public void UpdateAllCell()
        { 
            fromCell.Isbuyed();
            buyBtn.gameObject.SetActive(false);
            _loop.RefreshAll();
        }

        private void BuyFun()
        {
            GameIAP.Purchase(config.BuyKey, () =>
            {
                var dic = new Dictionary<string, object>();
                dic["name"] = config.name;
                dic["type"] = "iap";
                TBAMgr.Instance.SendLogEvent("secretbuy",dic);
                UpdateAllCell(); 
            },"secret"); 
        }
    }

    public class SecretViewPhotosItem
    {
        private GameObject lockImg;
        private GameObject kuang;
        private GameObject videoTag;
        private Image img;
        private SecretGiftInfo _secretGiftInfo;
        public SecretViewPhotosItem(GameObject obj)
        {
            lockImg = obj.transform.Find("Lock").gameObject;
            kuang = obj.transform.Find("kuang").gameObject;
            videoTag = obj.transform.Find("Tag").gameObject;
            img = obj.transform.Find("mask/Img").GetComponent<Image>();

            _secretGiftInfo= StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo;
        }

        public void UpdateContent(int index, string name, string tag)
        {
            lockImg.SetActive(!_secretGiftInfo.Buys.ContainsKey(tag) || !_secretGiftInfo.Buys[tag]);
            var config = GameConfigSys.GetPhotoByName(name);
            bool isvideo = config.name.ToLower().Contains(".mp4");
            videoTag.SetActive(isvideo);
            kuang.SetActive(!isvideo);

            string atlas = GameConfigSys.GetPhotoAtlasName(config.name);
            img.sprite = AssetLoad.Instance.LoadSprite(GUtility.GetPhotoName(config.name), atlas);
            GUtility.ApplyAspect(img.sprite, img.transform.parent.GetComponent<RectTransform>(), img.GetComponent<RectTransform>() );
        }
    }
    public class SecretViewPhotosLoop : UIWidget
    {
        
        [UIBinder("")] private GridView mLoopGridView;

        // private List<PhotoItem> itemData;
        private List<string> itemData;
        public int mTotalDataCount = 100;//total item count in the GridView  
        int mCurrentSelectIndex = -1;

        private bool gridInit = false;
        private string tag = "";
        public override void OnCreate()
        {
            base.OnCreate();
            itemData = (List<string>)userDatas[0];
            tag = (string)userDatas[1];
            mTotalDataCount = itemData.Count;
            var s = ViewUtility.GetEnoughXScale();
            mLoopGridView.ItemSclae = s;
            
            mLoopGridView.InitGridView(mTotalDataCount, OnGetItemByRowColumn);
        }

        public void RefreshAll()
        {
            mLoopGridView.RefreshAllShownItem();
            
        }
        LoopGridViewItem OnGetItemByRowColumn(GridView gridView, int index, int row, int column)
        {
            if (index < 0)
            {
                return null;
            } 
            LoopGridViewItem item = gridView.NewListViewItem("Item");
            var proxy = item.TryGetOrAddComponent<MonoCustomDataProxy>();
            var pitem = proxy.GetCustomData<SecretViewPhotosItem>();
            if (pitem == null)
            {
                pitem = new SecretViewPhotosItem(item.gameObject);
                proxy.SetCustomData(pitem);
            }

            pitem.UpdateContent(index,itemData[index],tag);
            return item;
        }
    }
}