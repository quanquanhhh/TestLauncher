using System;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

namespace GamePlay
{
    public static class ViewUtility
    {
        private static float match = 0.5f;
        public static readonly Vector2 DesignSize = new Vector2(1080, 1920);
        public static Vector2 UISize { get; private set; }

        public static float UIScale { get; private set; }
        public static float AdjustTopHeight;
        public static void SetUpViewSize()
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float scaleX = screenWidth / DesignSize.x;
            float scaleY = screenHeight / DesignSize.y;
            
            float logX = Mathf.Log(scaleX, 2);
            float logY = Mathf.Log(scaleY, 2);
            float logAverage = Mathf.Lerp(logX, logY, match);
            UIScale = Mathf.Pow(2, logAverage); 
            
            float uiWidth = screenWidth / UIScale;
            float uiHeight = screenHeight / UIScale;

            UISize = new Vector2(uiWidth, uiHeight);
            
            Rect safeArea = Screen.safeArea;
            float topUnsafePixels = Screen.height - safeArea.yMax;
            AdjustTopHeight = topUnsafePixels / UIScale /2; 
            Debug.Log($" [ViewUtility ]  Screen : {screenWidth}-{ screenHeight}   UISIZE : {UISize}  UISCALE  : {UIScale}");
        }

        public static float GetEnoughXScale(float weight = 1080)
        {
            var scale = Screen.width / weight;
            return scale / UIScale;
        }
    }
}