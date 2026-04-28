using System.Linq;
using Foundation.Statistics.Facebook;
using GameConfig;
using UnityEngine;
using XGame.Scripts.IAP;

namespace Foundation
{
 
 
    public class GameCommon
    {
        public static string GameName;
        public static bool PlayerFirstOpenApp = false ;
        public static bool AdSwitch = true; //资源包
        public static bool IsCoin = false;
        public static void InitSDKsByPlatform()
        {
            
            AdMgr.Instance.OnInitMaxSDK();
            AdjustManager.Instance.InitAdjust(); 
            FacebookMgr.Instance.Init();
            
        }

        public static void InitIAP()
        {
            var products = GameConfigSys.GetAllProductName(); 
            var names = products.Keys.ToList(); 
            IAPManager.Instance.InitProduct(names, products); 
            
        }
 
    }
}