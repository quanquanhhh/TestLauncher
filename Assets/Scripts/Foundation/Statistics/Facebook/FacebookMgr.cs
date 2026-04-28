using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

namespace Foundation.Statistics.Facebook
{
    public class FacebookMgr : SingletonComponent<FacebookMgr>
    { 
        bool InitSuccess = false; 
        public void Init()
        {
            string sFBID = ConfigManager.useConfig.libs.fb_appid;
            string sFBToken = ConfigManager.useConfig.libs.fb_apptoken;
            if (string.IsNullOrEmpty(sFBID) || string.IsNullOrEmpty(sFBToken)) return;
            if (!FB.IsInitialized)
            {
                Debug.Log("Facebook SDK Init Start"); 
                FB.Init(sFBID, sFBToken, true, true, true, false, true, null, "en_US", GameQuite, InitCallback);
            }
            else
            {
                FB.ActivateApp();
            }
        }
        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                Debug.Log("Facebook SDK Init Success");  
                InitSuccess = true;
            }
            else
            {
                Debug.Log("Facebook SDK Init Failed");   
            }
        }
        private void GameQuite(bool IsGameQuite)
        {
            if (!IsGameQuite)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
        public void StatrPlayGame()
        {
            if (InitSuccess)
            {
                FB.LogAppEvent("game_start");
            }
        }
        public void TrackPurchaseEvent(double pValue)
        {
            try
            {
                if (InitSuccess)
                {
                    FB.LogPurchase(new decimal(pValue));
                }
            }
            catch { }
        }
        public void TrackAdRevenueEvent(MaxSdkBase.AdInfo adInfo)
        {
            try
            {
                if (InitSuccess)
                {
                    var dic = new Dictionary<string, object>();
                    dic[AppEventParameterName.Currency] = "USD";
                    FB.LogAppEvent("AdImpression",(float)adInfo.Revenue,dic);
                }
            }
            catch { }
        }
        
    }
}