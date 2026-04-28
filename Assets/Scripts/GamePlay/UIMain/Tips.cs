using DG.Tweening;
using Foundation;
using TMPro;
using UnityEngine;

namespace GamePlay.UIMain
{
    [Window("Tips", WindowLayer.Tip)]
    public class Tips : UIWindow
    {
        [UIBinder("text")] private TextMeshProUGUI text;
        [UIBinder("bg")] private CanvasGroup canvas;

        public override void OnCreate()
        {
            base.OnCreate();
            SubScribeEvent<ShowTips>(OnShowTips);
            canvas.alpha = 0;
        }

        private async void OnShowTips(ShowTips obj)
        {
            if (obj.isdebug)
            {
#if !UNITY_DEBUG || !DEBUG_APP || !DEBUG || !DEVELOPMENT_BUILD 
            return;
#endif
                
            }
            canvas.DOKill();
            text.text = obj.text;
            canvas.DOFade(1, 0.3f);
            await UniTaskMgr.Instance.WaitForSecond(1f);
            canvas.DOFade(0, 0.08f);

        }
    }
}