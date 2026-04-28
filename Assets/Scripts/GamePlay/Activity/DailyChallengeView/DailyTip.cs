using Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    [Window("DailyTip",WindowLayer.Popup)]
    public class DailyTip : UIWindow
    {
        [UIBinder("CloseBtn")] private Button  closeBtn;
        [UIBinder("A")] private GameObject img1;
        [UIBinder("B")] private GameObject img2;
        

        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(Close);
            img1.SetActive(UserUtility.UserType == "A");
            img2.SetActive(UserUtility.UserType == "B");
        }
    }
}