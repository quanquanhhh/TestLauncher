using Foundation;
using Foundation.AudioModule;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    [Window("UIPrivacy", WindowLayer.Popup)]
    public class UIPrivacy : UIWindow
    {
        [UIBinder("closeBtn")] private Button  closeBtn;
        [UIBinder("Content")] private RectTransform  _content;

        public override void OnCreate()
        {
            base.OnCreate();
            _content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            closeBtn.onClick.AddListener(() =>
            {
                closeBtn.enabled = false;
                AudioModule.Instance.ClickAudio();
                Close();
            });
        }
    }
}