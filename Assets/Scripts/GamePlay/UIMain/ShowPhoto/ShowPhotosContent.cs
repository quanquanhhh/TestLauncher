using System;
using System.Collections.Generic;
using Foundation;
using Foundation.Storage;
using GamePlay.Component;
using GamePlay.Storage;
using Unity.VisualScripting;
using UnityEngine;

namespace GamePlay.UIMain
{
    public class ShowPhotosContent : UIWidget
    {
        [UIBinder("")] private GameObject _gameObject;
        [UIBinder("Photo")] private RectTransform PhotoModel;

        private int startIndex = 0;
        UICoverFlow2 flow;
        List<string> names = new List<string>();
        public override void OnCreate()
        {
            base.OnCreate();
            names = (List<string>)userDatas[0];
            startIndex = (int)userDatas[1];
            flow = _gameObject.TryGetOrAddComponent<UICoverFlow2>();
            PhotoModel.anchorMin = new Vector2(0.5f, 0.5f);
            PhotoModel.anchorMax = new Vector2(0.5f, 0.5f);
            PhotoModel.sizeDelta = rectTransform.rect.size;
            
            PhotoModel.Find("p1").GetComponent<RectTransform>().sizeDelta =  rectTransform.rect.size;
            CreateCell();

        }
 
        private void CreateCell()
        {
            flow.OnBindItem = (item, data, index) =>
            {
                string name = (string)data;
                bool isVideo = name.ToLower().Contains("mp4");

                var p1 = item.Find("p1");
                var media = p1.TryGetOrAddComponent<UguiMediaSource>();
                media.Init();

                string sourceName = GUtility.GetPhotoName(name);
                media.SetSource(sourceName, isVideo);
            };
            flow.OnMoveUpdate = (data, index) =>
            {
                string name = (string)data;
                if (UIModule.Instance.Get<ShowPhoto>() != null)
                {
                    UIModule.Instance.Get<ShowPhoto>().UpdatePhotoIndex(index,name); 
                } 
            };
            flow.SideScale = 1.0f;
            flow.SideYOffset = 0f;
            flow.SideAlpha = 1.0f; 
            flow.SetData(names, PhotoModel); 
            flow.SetLoop(true);
            // flow.SetLoop(true);
            // flow.SetAutoScroll(true); 
            flow.JumpTo(startIndex, true);   
        }

        private bool isplay = false;
        public void Play()
        { 
            isplay = true;
            flow.SetAutoScroll(true); 
        }

        public void Pause()
        {
            CancelPlay();
            
        }

        private void CancelPlay()
        {
            isplay = false;
            flow.SetAutoScroll(false);
        }
    }
}