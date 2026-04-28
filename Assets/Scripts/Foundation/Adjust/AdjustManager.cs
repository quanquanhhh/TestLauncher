using AdjustSdk;
using DG.Tweening;
using UnityEngine;

namespace Foundation
{
    public class AdjustManager : SingletonComponent<AdjustManager>
    {
        public string mediaSource;
        public int AdjustStatus = -1; // -1 没返回  0 natualuser A 1 aduser B
        public bool AdjustBack = false;

        private string useKey;
        public void InitAdjust()
        {
            Debug.Log("[Adjust]  InitAdjust");
#if UNITY_IOS
            Adjust.RequestAppTrackingAuthorization(b => Debug.Log(b));
#endif
            Adjust.AddGlobalCallbackParameter("customer_user_id", SystemInfo.deviceUniqueIdentifier);
            
            AdjustConfig adjustConfig = new AdjustConfig(ConfigManager.AdjustDevKey, AdjustEnvironment.Production);
#if DEBUG_APP || UNITY_EDITOR
            adjustConfig.LogLevel = AdjustLogLevel.Verbose;
#else
            adjustConfig.LogLevel = AdjustLogLevel.Suppress;
#endif
            adjustConfig.AttributionChangedDelegate = OnAttributionChanged;
            adjustConfig.SessionSuccessDelegate = OnSessionSuccess;
            adjustConfig.SessionFailureDelegate = OnSessionFailure;
            adjustConfig.EventFailureDelegate = OnEventFailureDelegate;
            adjustConfig.EventSuccessDelegate = OnEventSuccessDelegate;
            DOVirtual.DelayedCall(1f, delegate()
            {
                Debug.Log("[Adjust]  Adjust.InitSdk()");
                Adjust.InitSdk(adjustConfig);
            });
        }
        private void OnEventSuccessDelegate(AdjustEventSuccess obj)
        {
            Debug.Log("[Adjust]  AdjustEventSuccess");
        }
        private void OnEventFailureDelegate(AdjustEventFailure obj)
        {
            Debug.Log("[Adjust]  AdjustEventFailure");
        }
        private void OnSessionSuccess(AdjustSessionSuccess pRet)
        {
            Debug.Log("[Adjust]  AdjustSessionSuccess");
        }

        private void OnSessionFailure(AdjustSessionFailure pRet)
        {
            Debug.Log("[Adjust]  AdjustSessionFailure");
        }
        private void OnAttributionChanged(AdjustAttribution adjustAttribution)
        { 
            Debug.Log(" [TODO-------ADJUST] OnAttributionChanged" + adjustAttribution.Network.ToLower() + " || " +  adjustAttribution.TrackerName + " || " +  Application.identifier);
            
            mediaSource = adjustAttribution.Network;
            if (adjustAttribution.Network.ToLower().Equals("organic")
                || adjustAttribution.TrackerName.ToLower().Equals("organic")
                || adjustAttribution.Network.ToLower().Contains("no user consent")
                || adjustAttribution.TrackerName.ToLower().Equals("no user consent"))
            {
           
                if (AdjustStatus == -1)
                {
                    AdjustStatus = 0;
                    Event.Instance.SendEvent(new AdjustBack(AdjustStatus));
                }
            }
            else
            {
                AdjustStatus = 1;
                Event.Instance.SendEvent(new AdjustBack(AdjustStatus));
            }
            AdjustBack = true;
        }
        public void LogAdjustRevenue(string sKey, double amount, string currency)
        {
            AdjustEvent adjustEvent = new AdjustEvent(sKey);
            adjustEvent.SetRevenue(amount, currency);
            Adjust.TrackEvent(adjustEvent);
        }
        public void LogAdjustRevenue(MaxSdkBase.AdInfo adInfo, string currency)
        {
            AdjustAdRevenue adRevenue = new AdjustAdRevenue("applovin_max_sdk");
            adRevenue.SetRevenue(adInfo.Revenue, currency);
            adRevenue.AdRevenueNetwork = adInfo.NetworkName;
            adRevenue.AdRevenueUnit = adInfo.AdUnitIdentifier;
            adRevenue.AdRevenuePlacement = adInfo.Placement;     
            
#if UNITY_IOS
                adRevenue.AddCallbackParameter("platform", "ios");
#elif UNITY_ANDROID
            adRevenue.AddCallbackParameter("platform", "android");
#else
            adRevenue.AddCallbackParameter("platform", "other");
#endif      
            Adjust.TrackAdRevenue(adRevenue);
        }
        public void LogAdjustRevenue2(float timer)
        {
            AdjustAdRevenue adRevenue = new AdjustAdRevenue("applovin_max_sdk");
            adRevenue.SetRevenue(timer, "USD"); 

#if UNITY_IOS
                adRevenue.AddCallbackParameter("platform", "ios");
#elif UNITY_ANDROID
            adRevenue.AddCallbackParameter("platform", "android");
#else
            adRevenue.AddCallbackParameter("platform", "other");
#endif      
            Adjust.TrackAdRevenue(adRevenue);
        }
        
        
    }
}