using System.Collections.Generic;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.UIMain
{
    [Window("RemoveAds", WindowLayer.Popup)]
    public class RemoveADSView : UIWindow
    {
        [UIBinder("MainIcon")] private Image rightIcon;
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("BuyBtn")] private Button buyBtn;
        [UIBinder("price")] private TextMeshProUGUI price;

        [UIBinder("Rewrad1")] private Transform rewardM;
        
        [UIBinder("iconA")] private GameObject iconA;
        [UIBinder("iconB")] private GameObject iconB;
        GameConfig.Shop shop = null;

        private List<Vector3> icons = new ();
        public override void OnCreate()
        {
            base.OnCreate();
            iconA.SetActive(UserUtility.UserType == "A");
            iconB.SetActive(UserUtility.UserType == "B");
            shop = GameConfigSys.GetIAPKey("RemoveAds");
            closeBtn.onClick.AddListener(() =>
            {
                Close();
                AudioModule.Instance.ClickAudio();
            });
            buyBtn.onClick.AddListener(BuyRemoveAds);
            price.text = IAPManager.Instance.GetLocalizedPrice(shop.buyKey);
            CreateReward();
        }

        private void CreateReward()
        {
            for (int i = 0; i < shop.buyItem.Count; i++)
            {
                var item =  shop.buyItem[i];
                if (item == (int)ItemType.RemoveAds)
                {
                    icons.Add(Vector3.zero);
                    continue;
                }

                var obj = GameObject.Instantiate(rewardM, rewardM.parent);
                obj.Find("Icon").GetComponent<Image>().sprite = GUtility.GetItemIcon((ItemType)item);
                obj.Find("Amount").GetComponent<TextMeshProUGUI>().text = "x" + shop.buyCount[i].ToString();
                icons.Add(obj.Find("Icon").position);
            }
            rewardM.gameObject.SetActive(false);
        }


        private void BuyRemoveAds()
        {
            AudioModule.Instance.ClickAudio();
            GameIAP.Purchase(shop.buyKey, SuccessFun,"removeads");
            // IAPManager.Instance.Purchase(shop.buyKey, SuccessFun); 
        }
        
        private void SuccessFun()
        {
            Event.Instance.SendEvent(new AdsStateChange());
            for (int index = 0; index < shop.buyItem.Count; index++)
            {
                var item = shop.buyItem[index];
                var count = shop.buyCount[index];
                if (item == (int)ItemType.Photo)
                {
                    
                }
                else
                {
                    Event.Instance.SendEvent(new AddItem(item, count, icons[index]));
                }
            }
            
            Close();
        }
    }
}