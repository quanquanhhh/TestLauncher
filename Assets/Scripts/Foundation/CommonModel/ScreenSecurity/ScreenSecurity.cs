using UnityEngine;

namespace Foundation.ScreenSecurity
{
    public class ScreenSecurity
    {
        public static void EnableAndroidSecureFlag()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Run on UI thread to avoid CalledFromWrongThreadException
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                try {
                    AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");
                    AndroidJavaClass windowManagerLayoutParams = new AndroidJavaClass("android.view.WindowManager$LayoutParams");
                    int flagSecure = windowManagerLayoutParams.GetStatic<int>("FLAG_SECURE");
                    window.Call("addFlags", flagSecure);
                } catch (System.Exception e) {
                    Debug.LogError("Error setting FLAG_SECURE: " + e.Message);
                }
            }));
#endif
        }
 
    }
}