using DG.Tweening;
using Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.Game
{
    [Window("UIGuide_Game",WindowLayer.Popup)]
    public class GameGuide : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;

        [UIBinder("NextBtn")] private Button nextBtn;
        [UIBinder("PlayBtn")] private Button PlayBtn;
        
        [UIBinder("Page1")] private Transform page1;
        [UIBinder("Page2")] private Transform page2;
        
        private float scl;
        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(Close); 
            page1.localScale = Vector3.zero;
            page2.localScale = Vector3.zero;
            page2.gameObject.SetActive(false);
            scl = ViewUtility.GetEnoughXScale();
            page1.DOScale(Vector3.one * scl, 0.5f);
            
            nextBtn.onClick.AddListener(ToNextPage);
            PlayBtn.onClick.AddListener(Close);
            
        }

        private void ToNextPage()
        {
            page1.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
            {
                page1.gameObject.SetActive(false);
            });
            page2.gameObject.SetActive(true);
            page2.DOScale(Vector3.one * scl, 0.5f);
        }
    }
}