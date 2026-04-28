using System;
using System.Collections.Generic;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain.Widget;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.Activity.Pass
{
    [Window("Pass", WindowLayer.Popup)]
    public class PassView : UIWindow
    {
        [UIBinder("cell")] private Transform cell;
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Touch")] private GameObject bantouch;
        
        List<float> posx = new List<float>(){-251,251, 251,-251};
        private float offsetY = 554;

        private float initY = -275;

        private int ShowItemCount = 6;
        private PassInfo storage =
            StorageManager.Instance.GetStorage<ActivityInfo>().PassInfo;

        private List<GameConfig.Pass> config = GameConfigSys.pass;
        private List<PassItem> _items = new List<PassItem>();
        private List<Vector2> itemsPos = new();
        private int total = 0;

        private int itemindex;
        public override void OnCreate()
        {
            base.OnCreate();
            closeBtn.onClick.AddListener(() =>
            {
                Close();
                
                AudioModule.Instance.ClickAudio();
            });
            CreateCell();
            bantouch.SetActive(false);

        }

        public override void Close()
        {
            base.Close();
            LobbySequence.Instance.FinishTask("PassGuide");
        }

        private void CreateCell()
        {
            int start = storage.CurrentIndex;
            total = Math.Min(config.Count, storage.PassPhotoNames.Count);
            
            if (storage.CurrentIndex + 6 > total)
            {
                start = total - 6;
            } 
            int count = 0;
            for (int index = start; index < start+8; index++)
            {
                if (total <= index)
                {
                    break;
                }
                var obj = GameObject.Instantiate(cell, cell.parent);
                float x = posx[count % 4];
                float y = initY - offsetY * (int)(count / 2);
                var item = AddWidget<PassItem>(obj.gameObject, true, config[index], index,this);
                _items.Add(item);
                item.rectTransform.anchoredPosition = new Vector2(x, y);
                item.gameObject.name = count.ToString();
                itemsPos.Add(new Vector2(x,y));
                if (index == storage.CurrentIndex)
                {
                    itemindex = count;
                }
                count++;
            }
            cell.gameObject.SetActive(false);
        }

        public async void MoveCell()
        { 
            if (storage.CurrentIndex + 6 > total)
            {
                return;
            }
            bantouch.gameObject.SetActive(true);
            bool finalMove = false;
            int first = itemindex % _items.Count;
            for (int i = 0; i < _items.Count; i++)
            {
                int checkIndex = (itemindex + i) % _items.Count;
                await UniTaskMgr.Instance.WaitForSecond(0.15f);
                if (checkIndex == first)
                {
                    Vector2 endpos = new Vector2(-251*2 ,0) + _items[checkIndex].rectTransform.anchoredPosition;
                    finalMove = total > _items[checkIndex].index + 8;
                    _items[checkIndex].rectTransform.DOAnchorPos(endpos, 0.3f).OnComplete(() =>
                    {
                        var item = _items[checkIndex];
                        if (total > item.index + 8)
                        { 
                            item.UpdateContent(config[item.index+8], item.index+8);
                            item.rectTransform.anchoredPosition = itemsPos[^1] - new Vector2(0, offsetY);
                        }
                    });
                }
                else
                {
                    Debug.Log(checkIndex - 1);
                    var posindex = (i - 1 + _items.Count) % _items.Count;
                    Vector2 endpos = itemsPos[posindex];
                    _items[checkIndex].rectTransform.DOAnchorPos(endpos, 0.3f);
                }
            }

            if (finalMove)
            {
                _items[first].rectTransform.DOAnchorPos(itemsPos[^1],0.3f);
            }
            itemindex++;

            bantouch.gameObject.SetActive(false);
        }
    }

    public class PassItem : UIWidget
    {
        [UIBinder("current")] private GameObject current;
        [UIBinder("Photo")] private Image img;
        [UIBinder("lock")] private GameObject plock;
        [UIBinder("claim")] private Button claimBtn;
        [UIBinder("buy")] private Button buyBtn;
        [UIBinder("vip")] private Button vipBtn;
        [UIBinder("price")] private TextMeshProUGUI price;
        [UIBinder("finished")] private GameObject finished;

        private GameConfig.Pass config;
        public int index;
        private PassView _passView;
        private PassInfo storage =
            StorageManager.Instance.GetStorage<ActivityInfo>().PassInfo;

        private string showName;
        private bool vipFree => StorageManager.Instance.GetStorage<BaseInfo>().Buff.IsPermanent;
        public override void OnCreate()
        {
            base.OnCreate();
            config = (GameConfig.Pass)userDatas[0];
            index = (int)userDatas[1];
            _passView = (PassView)userDatas[2];
            claimBtn.onClick.AddListener(ClaimImg);
            buyBtn.onClick.AddListener(BuyFun);
            vipBtn.onClick.AddListener(VipFun);
            if (!config.isFree)
            {
                price.text = IAPManager.Instance.GetLocalizedPrice(config.BuyKey);
            }
            UpdateContent(config, index);
        }

        private void VipFun()
        {
            
            AudioModule.Instance.ClickAudio();
            if (index != storage.CurrentIndex)
            {
                Event.Instance.SendEvent(new ShowTips("can't claim"));
                return;
            }
            Claim();
        }

        private void BuyFun()
        {
            AudioModule.Instance.ClickAudio();
            if (index != storage.CurrentIndex)
            {
                Event.Instance.SendEvent(new ShowTips("can't claim"));
                return;
            }
            GameIAP.Purchase(config.BuyKey, Claim,"pass"); 
        }

        private void Claim()
        {
            storage.CurrentIndex++;
            finished.gameObject.SetActive(true);
            buyBtn.gameObject.SetActive(false);
            claimBtn.gameObject.SetActive(false);
            vipBtn.gameObject.SetActive(false);
            plock.gameObject.SetActive(false);
            _passView.MoveCell();
            Event.Instance.SendEvent(new AddPhoto((int)PhotoType.Pass, 1, new List<string>(){showName},showpop:false));
        }
        private void ClaimImg()
        {
            AudioModule.Instance.ClickAudio();
            if (index != storage.CurrentIndex)
            {
                Event.Instance.SendEvent(new ShowTips("can't claim"));
                return;
            }

            Claim();
        }

        
        private async void ShowImg()
        {
            showName = storage.PassPhotoNames[index];
            string name = GUtility.GetPhotoName(showName);
            string atlas =  GameConfigSys.GetPhotoAtlasName(name);
            var sp = AssetLoad.Instance.LoadSprite(name, atlas); 
            img.sprite = sp;
            GUtility.ApplyAspect(sp, img.transform.parent.GetComponent<RectTransform>(), img.GetComponent<RectTransform>());
        }

        public void UpdateContent(GameConfig.Pass pass, int itemIndex)
        {
            index = itemIndex;

            config = pass;
            
            plock.gameObject.SetActive(index >= storage.CurrentIndex);
            current.gameObject.SetActive(index == storage.CurrentIndex);
            claimBtn.gameObject.SetActive(config.isFree && index >= storage.CurrentIndex);
            finished.gameObject.SetActive(index < storage.CurrentIndex);
            price.text =  IAPManager.Instance.GetLocalizedPrice(config.BuyKey);
            buyBtn.gameObject.SetActive(!config.isFree && !vipFree  && index >= storage.CurrentIndex);
            vipBtn.gameObject.SetActive(!config.isFree && vipFree  && index >= storage.CurrentIndex);
            ShowImg();

        }
    }
}