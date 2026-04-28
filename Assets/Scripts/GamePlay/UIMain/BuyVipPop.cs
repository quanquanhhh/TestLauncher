using System;
using System.Collections.Generic;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using Spine.Unity;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;
using Product = UnityEngine.Purchasing.Product;

namespace GamePlay.UIMain
{
    [Window("BuyVipPop", WindowLayer.Popup)]
    public class BuyVipPop : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("BuyBtn")] private Button buyBtn;
        [UIBinder("Privacy")] private Button PrivacyBtn;
        [UIBinder("Term")] private Button TermBtn;
        
        [UIBinder("Select")] private Transform selectObj;
        [UIBinder("Select1")] private Toggle select1;
        [UIBinder("Select2")] private Toggle select2;

        [UIBinder("price1")] private TextMeshProUGUI price1;
        [UIBinder("price2")] private TextMeshProUGUI price2;
        
        
        [UIBinder("Item")] private Transform item;
        [UIBinder("Title")] private RectTransform title;
        [UIBinder("Spine")] private SkeletonGraphic  skeleton;
        
        private bool selectMonth = true;
        private string monthVIP;
        private string lifeVIP;

        private List<VIP> vips = new();
        public override void OnCreate()
        {
            base.OnCreate();
            int initShow = 2;
            if (userDatas.Length > 0)
            {
                initShow = (int)userDatas[0];
            }
            title.anchoredPosition -= new Vector2(0,  ViewUtility.AdjustTopHeight);
            closeBtn.onClick.AddListener(()=>
            {
                AudioModule.Instance.ClickAudio();
                
                Close();
            });
            buyBtn.onClick.AddListener(BuyVip);
            select1.onValueChanged.AddListener(Select);
            Toggle initToggle = initShow == 2 ? select1 : select2;
            selectMonth = initShow == 2 ? true : false;
            initToggle.transform.localScale = Vector3.one * 1.17f;
            initToggle.isOn = true; 
            ChangeVip(initShow);
            CreateItem();
            price1.text = IAPManager.Instance.GetLocalizedPrice(monthVIP);
            price2.text = IAPManager.Instance.GetLocalizedPrice(lifeVIP);
            PrivacyBtn.onClick.AddListener(OpenPrivacy);
            var x = ViewUtility.GetEnoughXScale();
            selectObj.localScale = Vector3.one * x;
        }

        private void OpenPrivacy()
        {
            
            Application.OpenURL("https://desiregirls.net/privacy.html");
        }

        private void ChangeVip(int vip)
        {
            skeleton.initialSkinName = "vip"+vip;
            skeleton.Initialize(true);
        }

        private void CreateItem()
        {
            vips = GameConfigSys.vips;
            foreach (var vip in vips)
            {
                var obj = GameObject.Instantiate(item,item.parent);
                var textt = obj.Find("text").GetComponent<LocalizeStringEvent>();
                textt.SetEntry(vip.desc);
                obj.name = vip.desc;
            }
            item.gameObject.SetActive(false);
            ChangeItem();
            monthVIP = vips[0].buykey;
            lifeVIP = vips[1].buykey;
        }

        private void ChangeItem()
        {
            foreach (var vip in vips)
            {
                var it = item.parent.Find(vip.desc);
                var isRight = selectMonth ? vip.normalvip : vip.lifevip;
                it.Find("right").gameObject.SetActive(isRight);
                it.Find("wrong").gameObject.SetActive(!isRight);
            }
        }

        private void Select(bool select)
        {
            if (selectMonth == select)
            {
                return;
            }
            AudioModule.Instance.ClickAudio();
            selectMonth = select;
            ChangeItem();
            
            ChangeVip(selectMonth ? 2:1);
            float scl = select ? 1.17f : 1f;
            float scl2 = !select ? 1.17f : 1f;
            select1.transform.localScale = Vector3.one * scl;
            select2.transform.localScale = Vector3.one * scl2;
        }
        
        private void BuyVip()
        {
            AudioModule.Instance.ClickAudio();
            
            string key = selectMonth ? monthVIP : lifeVIP;
            string str = selectMonth ? "month" : "life";
            GameIAP.Purchase(key, BuySuccess,"buyvip" + str,CheckExpire);
            // ItemUtility.Instance.OpenPurcher(key,BuySuccess);
            
        }

        private void CheckExpire(Product obj)
        {
            var getexpire = IAPManager.GetSubscriptionExpire(obj);
            
            StorageManager.Instance.GetStorage<BaseInfo>().Buff.VipExpire = getexpire;
        }

        private void BuySuccess()
        {
            StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsVip = true;
            StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds = true;
            if (!selectMonth)
            {
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent = true;
            } 
            Close();
            
            Event.Instance.SendEvent(new VIPStateChange());
        }
    }
}