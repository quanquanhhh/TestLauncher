using Foundation;
using UnityEngine;

namespace GamePlay.UIMain
{
    [Window("ProtectScreen",WindowLayer.Loading)]
    public class ProtectScreen : UIWindow
    {
        public static GameObject window;
        public override void OnCreate()
        {
            base.OnCreate();
            gameObject.SetActive(false);
            SubScribeEvent<FocusLeft>(OnFocusLeft);
            SubScribeEvent<FocusEnter>(OnFocusEnter);
        }

        private void OnFocusEnter(FocusEnter obj)
        {
            HideProtect();
        }

        private void OnFocusLeft(FocusLeft obj)
        {
            ShowProtect();
        }

        public void ShowProtect()
        {
            gameObject.SetActive(true);
        }

        public void HideProtect()
        {
            gameObject.SetActive(false);
        }
    }
}