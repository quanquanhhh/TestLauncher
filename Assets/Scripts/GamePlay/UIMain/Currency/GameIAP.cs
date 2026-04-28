using System;
using Foundation;
using GameConfig;
using UnityEngine.Purchasing;
using XGame.Scripts.IAP;

namespace GamePlay.UIMain.Widget
{
    public class GameIAP
    {
        public static void Purchase(string key, Action act, string from, Action<Product> productAct = null)
        {
            var info = GameConfigSys.iap.Find(x => x.productId == key);
            if (info == null)
            {
                Event.Instance.SendEvent(new ShowTips("Purchase Failed"));
                return;
            }
            else
            {
                Event.Instance.SendEvent(new ShowTips("Purchase Sucess"));
                
            }
            IAPManager.Instance.Purchase(key, act, productAct, info.AdjustKey, info.price, info.name, from);
        }
    }
}