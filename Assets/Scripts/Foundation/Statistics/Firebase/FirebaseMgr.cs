using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using Firebase.Messaging;
using Unity.Notifications.Android;
using UnityEngine;

namespace Foundation.Statistics.FirebaseMgr
{
    public class FirebaseMgr : SingletonComponent<FirebaseMgr>
    {
        

        public static int hasNotificationOpened = 0;
        private static bool mIsGoogleServiceOk = false;
        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        
        public override void OnInit()
        {
            base.OnInit();
            SetNotificationStatus();
            FBInstance();
        }
        private void SetNotificationStatus()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return;
#endif
            FirebaseMessaging.MessageReceived += (sender, args) =>
            {
                hasNotificationOpened = 1;
            };
            FirebaseMessaging.TokenReceived += (sender, args) =>
            {
                Debug.Log($"----Firebase token received:{args.Token}");
            };
#if UNITY_ANDROID
            if (AndroidNotificationCenter.GetLastNotificationIntent() != null)
            {
                AndroidNotificationIntentData data = AndroidNotificationCenter.GetLastNotificationIntent();
                Debug.Log($"----local notification intent:{data.Channel}");
                hasNotificationOpened = 2;
            }
#endif
        }
        void FBInstance()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return;
#endif
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                dependencyStatus = task.Result;
                FLog(" callback " + dependencyStatus);
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FLog("start init");
                    mIsGoogleServiceOk = true;
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    FLog("Firebase initialized successfully");
                    InitializeFCM();
                }
                else
                {
                    FLog($"Could not resolve all Firebase dependencies: {task.Result}");
                }
            });
        }
        void InitializeFCM()
        {
            FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            
            FirebaseMessaging.SubscribeAsync(ConfigManager.useConfig.firebase_topic);
            FLog("FCM Topic: " + ConfigManager.useConfig.firebase_topic);
            FLog("FCM initialized");
            CheckFirebaseDependencies();
        }
        async void CheckFirebaseDependencies()
        {
            Task<DependencyStatus> dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

            await dependencyTask;

            if (dependencyTask.IsCompleted)
            {
                if (dependencyTask.Result == DependencyStatus.Available)
                {
                    Debug.Log("Firebase Pass，State: " + dependencyTask.Result);
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                }
                else
                {
                    Debug.LogError("Firebase Error，State: " + dependencyTask.Result);
                }
            }
            else if (dependencyTask.IsFaulted)
            {
                Debug.LogError("Firebase Faile: " + dependencyTask.Exception);
            }
        }
        void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //TBA
            FLog("Received a new message " + Newtonsoft.Json.JsonConvert.SerializeObject(e.Message));
            
            if (e.Message.Notification != null)
            {
                FLog($"Title: {e.Message.Notification.Title}");
                FLog($"Body: {e.Message.Notification.Body}");
            }

            foreach (var pair in e.Message.Data)
            {
                FLog($"{pair.Key}: {pair.Value}");
            }
        }
        void OnTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            FLog($"Received registration token: {e.Token}");
        }

        static void FLog(string psgm)
        {
            UnityEngine.Debug.Log("FirebaseAC " + psgm);
        }
        public static bool CheckNotificationPermission()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var notificationManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "notification"))
            {
                return notificationManager.Call<bool>("areNotificationsEnabled");
            }
        }

        public static void OpenNotificationSettings()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                var intent = new AndroidJavaObject("android.content.Intent", "android.settings.APP_NOTIFICATION_SETTINGS");
                intent.Call<AndroidJavaObject>("putExtra", "android.provider.extra.APP_PACKAGE", currentActivity.Call<string>("getPackageName"));
                currentActivity.Call("startActivity", intent);
            }
        }

        /// <summary>
    /// 上报收入数据到 Firebase Analytics
    /// </summary>
    /// <param name="productId">产品 ID</param>
    /// <param name="productName">产品名称</param>
    /// <param name="price">价格</param>
    /// <param name="currency">货币代码 (例如:"USD")</param>
    /// <param name="transactionId">交易 ID</param>
    public void ReportRevenue(string productId, string productName, double price, string currency, string transactionId)
    {
    #if UNITY_ANDROID && !UNITY_EDITOR
        if (mIsGoogleServiceOk)
        {
            try
            {
                Parameter[] parameters = new Parameter[]
                {
                    new Parameter(FirebaseAnalytics.ParameterPromotionID, productId),
                    new Parameter(FirebaseAnalytics.ParameterPromotionName, productName),
                    new Parameter(FirebaseAnalytics.ParameterPrice, price),
                    new Parameter(FirebaseAnalytics.ParameterCurrency, currency),
                    new Parameter(FirebaseAnalytics.ParameterTransactionID, transactionId)
                };
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, parameters);
                FLog($"Revenue reported: {productName} - {price} ({currency})");
            }
            catch (Exception e)
            {
                FLog($"Error reporting revenue: {e.Message}");
            }
        }
    #endif
    }
    /// <summary>
    /// 上报广告收入数据到 Firebase Analytics
    /// </summary>
    /// <param name="adSource">广告来源 (例如: "AdMob", "UnityAds")</param>
    /// <param name="adFormat">广告格式 (例如: "banner", "interstitial", "rewarded")</param>
    /// <param name="adUnitId">广告单元 ID</param>
    /// <param name="revenue">广告收入</param>
    /// <param name="currency">货币代码 (例如:"USD")</param>
    public static void ReportAdRevenue(MaxSdkBase.AdInfo adInfo)
    {
        //string adSource, string adFormat, string adUnitId, double revenue, string currency
#if UNITY_ANDROID && !UNITY_EDITOR
        if (mIsGoogleServiceOk)
        {
            try
            {
                // 从adInfo中获取所有参数
                string adSource = adInfo.NetworkName;
                string adFormat = adInfo.AdFormat.ToString();
                string adUnitId = adInfo.AdUnitIdentifier;
                double revenue = adInfo.Revenue;
                string currency = "USD";
                
                Parameter[] parameters = new Firebase.Analytics.Parameter[]
                {
                    new Parameter("ad_source", adSource),
                    new Parameter("ad_format", adFormat),
                    new Parameter("ad_unit_id", adUnitId),
                    new Parameter("value", (long)(revenue)),
                    new Parameter("currency", currency)
                };
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAdImpression, parameters);
                FLog($"Ad revenue reported: {revenue} ({currency}) from {adSource}");
            }
            catch (System.Exception e)
            {
                FLog($"Error reporting ad revenue: {e.Message}");
            }
        }
#endif
    }
 
    }
}