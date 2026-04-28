using System;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.UIMain.Widget
{
    [Window("PermanentVIP",WindowLayer.Popup)]
    public class PermanentVIP : UIWindow
    {
        [UIBinder("Price")] private TextMeshProUGUI price;
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("BuyBtn")] private Button BuyBtn;

        [UIBinder("iconA")] private GameObject iconA;
        [UIBinder("iconB")] private GameObject iconB;
        public override void OnCreate()
        {
            base.OnCreate();
            iconA.SetActive(UserUtility.UserType == "A");
            iconB.SetActive(UserUtility.UserType == "B");
            closeBtn.onClick.AddListener(() =>
            {
                
                AudioModule.Instance.ClickAudio();
                Close();
            });
            BuyBtn.onClick.AddListener(BuyPermanentVip);
            price.text = IAPManager.Instance.GetLocalizedPrice("permanentvip");
            
        }

        private void BuyPermanentVip()
        {
            AudioModule.Instance.ClickAudio();
            Action act = () =>
            {

                StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsVip = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.RemoveAds = true;
                StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent = true;
                var count = GameConfigSys.activityPhotos[PhotoType.VIP].Count;
                Event.Instance.SendEvent(new AddPhoto((int)PhotoType.VIP, count,GameConfigSys.activityPhotos[PhotoType.VIP] )); 
                Event.Instance.SendEvent(new VIPStateChange());
                Close();
            };
            GameIAP.Purchase("permanentvip", act,"permanentvip");  
        }
    }
}