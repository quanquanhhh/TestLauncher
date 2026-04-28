using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Messaging;
#if UNITY_ANDROID && !UNITY_EDITOR
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif
using UnityEngine; 

namespace Foundation.Notification
{
    public class FirebaseNotification : SingletonScript<FirebaseNotification>
    {
        private static bool mIsGoogleServiceOk = false;
        public static int hasNotificationOpened = 0;
        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        
        public void OnInit()
        { 
    #if UNITY_ANDROID && !UNITY_EDITOR
            FirebaseMessaging.MessageReceived += (sender, args) =>
            {
                hasNotificationOpened = 1;
            };
            FirebaseMessaging.TokenReceived += (sender, args) =>
            {
                Debug.Log($"----Firebase token received:{args.Token}");
            };
            if (AndroidNotificationCenter.GetLastNotificationIntent() != null)
            {
                AndroidNotificationIntentData data = AndroidNotificationCenter.GetLastNotificationIntent();
                Debug.Log($"----local notification intent:{data.Channel}");
                hasNotificationOpened = 2;
            }
            Rigester();
    #endif
        }


        void Rigester()
        {
    #if UNITY_ANDROID && !UNITY_EDITOR
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
    #endif
        }

        void InitializeFCM()
        {
            FirebaseMessaging.TokenRegistrationOnInitEnabled = true;
            FirebaseMessaging.MessageReceived += OnMessageReceived;
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.SubscribeAsync("h04");
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
                    Debug.Log("Firebase Pass��State: " + dependencyTask.Result);
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                }
                else
                {
                    Debug.LogError("Firebase Error��State: " + dependencyTask.Result);
                }
            }
            else if (dependencyTask.IsFaulted)
            {
                Debug.LogError("Firebase Faile: " + dependencyTask.Exception);
            }
        }
        void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
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

        public static void OpenNewNotificationSettings()
        {
            
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Debug.Log("Already has permission.");
                return;
            }
            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += (permission) => HandleGranted();
            callbacks.PermissionDenied += (permission) => HandleDenied();
            callbacks.PermissionDeniedAndDontAskAgain += (permission) => HandlePermanentDenial();
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS", callbacks);
#endif
        }

        static void HandleGranted()
        {
            Debug.Log("Permission granted. Proceed to enable notifications.");
        }

        static void HandleDenied()
        {
            Debug.Log("Permission denied. Fallback logic here.");
        }

        static void HandlePermanentDenial()
        {
            Debug.Log("User selected 'Don't ask again'. Guide to settings.");
        }
        
    }
}