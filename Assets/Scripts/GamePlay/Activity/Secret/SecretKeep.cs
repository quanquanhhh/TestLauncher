using System;
using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine.UI;
using XGame.Scripts.IAP;

namespace GamePlay.Activity
{
    [Window("SecretKeep", WindowLayer.Popup)]
    public class SecretKeep : UIWindow
    {
        [UIBinder("BuyBtn")] private Button buyBtn;
        [UIBinder("price")] private TextMeshProUGUI price;
        [UIBinder("Close")] private Button closeBtn;

        private string BuyKey;
        private string name;
        public override void OnCreate()
        {
            base.OnCreate();
            BuyKey = (string)userDatas[0];
            name = (string)userDatas[1];
            price.text = IAPManager.Instance.GetLocalizedPrice(BuyKey);
            closeBtn.onClick.AddListener(() =>
            {
                UIModule.Instance.Close<SecretViewPhotos>();
                Close();
            });
            buyBtn.onClick.AddListener(BuyFun);
            StorageManager.Instance.GetStorage<ActivityInfo>().SecretGiftInfo.LastShowDiscountTime =  DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void BuyFun()
        {
            GameIAP.Purchase(BuyKey, () =>
            {
                var dic = new Dictionary<string, object>();
                dic["name"] = name;
                dic["type"] = "iap";
                TBAMgr.Instance.SendLogEvent("secretdiscount", dic);
                UIModule.Instance.Get<SecretViewPhotos>().UpdateAllCell();
                
            },"secretdiscount");    
        }
    }
}