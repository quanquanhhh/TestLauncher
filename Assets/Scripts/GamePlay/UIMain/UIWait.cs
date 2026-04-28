using Cysharp.Threading.Tasks;
using DG.Tweening;
using Foundation;
using GamePlay.Game;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    [Window("UIWait", WindowLayer.Popup)]
    public class UIWait : UIWindow
    {
        [UIBinder("Waiting")] private Transform waitTrans;

        public override void OnCreate()
        {
            base.OnCreate();
            
            waitTrans.gameObject.SetActive(true);
            waitTrans.localScale = Vector3.one;
            waitTrans.GetComponent<Image>().DOFade(1f, 0.5f);
            waitTrans.DOScale(1.1f, 0.9f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        
        public void BreakLoop()
        {
            waitTrans.DOKill();
            waitTrans.DOScale(15f, 0.8f);
            waitTrans.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
            {
                Close();
            }).SetEase(Ease.OutSine);
            UniTaskMgr.Instance.WaitForSecond(0.2f, () =>
            { 
                UIModule.Instance.Get<GameMain>().ShowMain(); 
            }).Forget();
        }

    }
}