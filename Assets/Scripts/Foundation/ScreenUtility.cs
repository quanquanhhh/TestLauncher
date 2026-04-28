using UnityEngine;

namespace Foundation
{
    public class ScreenUtility
    {
        
        public static float CamerScale = 9.5f;
        public static float AdjustScale = 1f;
        public static float ScreenWeight = 0;
        public static float AdjustTopHeight;
        public static void CheckRate()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer)
                Screen.SetResolution(480, 960, false);
             
            Rect safeArea = Screen.safeArea;
            AdjustTopHeight = Screen.height - safeArea.yMax;
            ScreenWeight = Screen.width;
            float screenRate = (float)Screen.width / Screen.height;
            float designRate = 1080.0f / 1920.0f; 
            AdjustScale = screenRate / designRate; 
            CamerScale =  CamerScale / AdjustScale;
#if UNITY_IOS
             AdjustTopHeight /= 2; 
#endif

        }
    }
}