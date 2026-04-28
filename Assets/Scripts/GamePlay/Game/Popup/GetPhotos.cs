using System;
using System.Collections.Generic;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using GamePlay.Component;
using GamePlay.UIMain;
using GamePlay.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

namespace GamePlay.Game.Popup
{
    [Window("GetPhotos", WindowLayer.Popup)]
    public class GetPhotos: UIWindow
    {
        [UIBinder("Claim")] private Button closeBtn;
        [UIBinder("Photo")] private Transform content;
        [UIBinder("p1")] private Transform p1;
        private UICoverFlow flow;

        private Action endAction;
        private List<RectTransform> objs;
        public override void OnCreate()
        {
            base.OnCreate();
            List<string> names = (List<string>)userDatas[0];
            
            closeBtn.onClick.AddListener(() =>
            {
                AudioModule.Instance.ClickAudio();
                Close();
            });
            flow = content.TryGetOrAddComponent<UICoverFlow>();
            CreatePhoto(names);
            if (userDatas.Length >= 2)
            {
                endAction = (Action)userDatas[1];
            }
        }

        public override void Close()
        {
            base.Close();
            // FlyPhotos();
            endAction?.Invoke();
        }

        private void FlyPhotos()
        {
            if (GameFsm.Instance.InGame())
            {
                return;
            }

            int index = 0;
            foreach (var obj in objs)
            {
                obj.parent = FlyUtilty.FlyRoot;
                obj.DOKill();
                Sequence seq = DOTween.Sequence();
                
                seq.Insert(0f,obj.transform.DOScale(Vector3.one * 0.25f,0.2f));
                seq.Insert(0f ,obj.transform.DOMove(MainUI.CollectTrans.position, 0.5f)); 
                seq.onComplete = delegate()
                {
                    GameObject.Destroy(obj.gameObject);
                };
                 
                
            }
        }

        private async void CreatePhoto(List<string> names)
        {
            List<RectTransform> items = new();
            for (int index = 0; index < names.Count; index++)
            {
                bool isvideo = names[index].ToLower().Contains("mp4");
                var obj = GameObject.Instantiate(p1, p1.parent);
                obj.transform.localPosition = Vector3.zero;
                obj.name = index.ToString();
                var media = obj.Find("img").AddComponent<UguiMediaSource>();
                var str = GUtility.GetPhotoName(names[index]);
                media.SetSource(str, isvideo);

                items.Add(obj.GetComponent<RectTransform>()); 
            }

            objs = new List<RectTransform>(items);
            flow.SetContent(items);
            flow.SetLoop(false);
            flow.SetAutoScroll(false);
        }
    }
}