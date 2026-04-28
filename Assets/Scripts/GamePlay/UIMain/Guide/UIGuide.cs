using Cysharp.Threading.Tasks;
using Foundation;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    [Window("UIGuide",WindowLayer.Popup)]
    public class UIGuide : UIWindow
    { 
        private Transform target;
        private Vector2 offsetPos = Vector2.zero;
        [UIBinder("hand")] private RectTransform hand;
        public override void OnCreate()
        {
            base.OnCreate();
            target = (Transform)userDatas[0];
            if (userDatas.Length > 1)
            {
                offsetPos = (Vector2)userDatas[1];
            }
            hand.gameObject.SetActive(false);
            CreateMask();
        }

        private void CreateMask()
        {
            UniTaskMgr.Instance.WaitForFrame(1, CheckTargetToTop).Forget();
            UniTaskMgr.Instance.WaitForFrame(2, ChangeHand).Forget();
        }

        private void ChangeHand()
        {
            hand.gameObject.SetActive(true);
            var targetCanvas = hand.gameObject.AddComponent<Canvas>();
            targetCanvas.overrideSorting = true;
            targetCanvas.sortingLayerName = "Guide";
            targetCanvas.sortingOrder = 999;
            hand.position = target.position;
            hand.anchoredPosition += offsetPos;
        }

        private void CheckTargetToTop()
        {
            if (target != null  )
            {
                if (target.transform is RectTransform)
                {
                    if (target.gameObject.GetComponent<Canvas>() == null)
                    {
                        var targetCanvas = target.gameObject.AddComponent<Canvas>();
                        targetCanvas.overrideSorting = true;
                        targetCanvas.sortingLayerName = "Guide";
                        target.gameObject.AddComponent<GraphicRaycaster>();
                    }
                } 
            }
        }

        public override void OnDestroy()
        {
            RecoverTarget(); 
            base.OnDestroy();
            
        }
        // public override void OnDestroy()
        // {
        //     RecoverTarget(); 
        // }

        private void RecoverTarget()
        {

            if (target.transform is RectTransform)
            {
                if (target.gameObject.GetComponent<GraphicRaycaster>() != null)
                {
                    var graphicRaycaster = target.gameObject.GetComponent<GraphicRaycaster>();
                    GameObject.Destroy(graphicRaycaster);
                }
                    
                if (target.gameObject.GetComponent<Canvas>() != null)
                {
                    var targetCanvas = target.gameObject.GetComponent<Canvas>();
                    GameObject.Destroy(targetCanvas);
                    
                }
            }
        }
        
    }
}