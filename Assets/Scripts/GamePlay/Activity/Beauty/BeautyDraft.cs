using System.Collections.Generic;
using System.Linq;
using Foundation;
using Foundation.AudioModule;
using Foundation.GridViewLoop;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using GamePlay.UIMain;
using GamePlay.UIMain.Widget; 
using TMPro; 
using UnityEngine;
using UnityEngine.UI;
using XGame.Scripts.IAP;
using Event = Foundation.Event;

namespace GamePlay.Activity
{
    [Window("Beauty", WindowLayer.Popup)]
    public class BeautyDraft : UIWindow
    {
        [UIBinder("Scroll View")] private GridView _gridView;

        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Coin")] private GameObject coin;
        [UIBinder("Diamond")] private GameObject Diamond;
        [UIBinder("Content")] private RectTransform _content;
        [UIBinder("Scroll View")] private GameObject scrollView;
        [UIBinder("Item")] private GameObject model;
        // private TableView _tableView;
        public int mTotalDataCount = 100;//total item count in the GridView  
        int mCurrentSelectIndex = -1;

        Dictionary<string, BeautyItemPhotos> datas = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.BeautyItemPhotos;
        private Dictionary<string, Beauty> configs = new();

        private List<string> order = StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.ShowOrder;
        public override void OnCreate()
        {
            base.OnCreate();

            _content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            closeBtn.onClick.AddListener(CloseFun);
            var config = GameConfigSys.GetBeautyInfo();
            for (int i = 0; i < config.Count; i++)
            {
                configs[config[i].id.ToString()] = config[i];
            }

            var d = GameConfigSys.GetNoShowPhotos(PhotoType.BeautyDraft);
            _gridView.InitGridView(mTotalDataCount, OnGetItemByRowColumn);
            SetCount(datas.Count);
            SubScribeEvent<UpdateBeautyDraftOrder>(OnUpdateBeautyDraftOrder);

            AddWidget<CoinWidget>(coin);
            AddWidget<DiamondWidget>(Diamond); 
        }
         
        private void CloseFun()
        {
            LobbySequence.Instance.FinishTask("BeautyEncountersGuide");
            AudioModule.Instance.ClickAudio();
            Close();
        }

        private void OnUpdateBeautyDraftOrder(UpdateBeautyDraftOrder obj)
        {
            _gridView.RefreshAllShownItem();
            // _gridView.MovePanelToItemByIndex(0);
        }

        public void SetCount(int count)
        {
            if (count <= mCurrentSelectIndex)
            {
                mCurrentSelectIndex = -1;
            }
 
            mTotalDataCount = count;
            _gridView.SetListItemCount(count, false); 
        } 
        LoopGridViewItem OnGetItemByRowColumn(GridView gridView, int index, int row, int column)
        {
            if (index < 0)
            {
                return null;
            } 
            LoopGridViewItem item = gridView.NewListViewItem("Item");
            var proxy = item.TryGetOrAddComponent<MonoCustomDataProxy>();
            var pitem = proxy.GetCustomData<BeautyCell>();
            if (pitem == null)
            {
                pitem = new BeautyCell(item.gameObject);
                proxy.SetCustomData(pitem);
            }

            string name = order[index];
            pitem.UpdateContent(index,datas[(name).ToString()], configs[name.ToString()]);
            return item;
        }
    }

    public class BeautyCell
    {
        private TextMeshProUGUI nameText;
        private string name;
        private List<Image> imgs = new();
        private List<Button> buttons = new();
        private Transform button;
        private RectTransform bg;
        private RectTransform photo;

        private BeautyItemPhotos _data;
        private Beauty _config;
        private int _index;
        
        public BeautyCell(GameObject obj)
        {
            
            nameText = obj.transform.Find("bg/name").GetComponent<TextMeshProUGUI>();
            for (int i = 1; i < 5; i++)
            {
                int temp = 0;
                temp = i - 1;
                var img = obj.transform.Find("photos/img" + i+"/photo").GetComponent<Image>();
                var btn = obj.transform.Find("photos/img" + i + "/openPhoto").GetComponent<Button>();
                imgs.Add(img);
                btn.onClick.AddListener(() =>
                {
                    OpenPhoto(temp);
                });
                buttons.Add(btn);
            }

            bg = obj.transform.Find("bg").GetComponent<RectTransform>();
            photo = obj.transform.Find("photos").GetComponent<RectTransform>();
            button = obj.transform.Find("Button");
            button.Find("ad").GetComponent<Button>().onClick.AddListener(AdBuy);
            button.Find("buy").GetComponent<Button>().onClick.AddListener(CurrencyBuy);
            button.Find("play").GetComponent<Button>().onClick.AddListener(PlayGame);
        }

        private void OpenPhoto(int index)
        {
            if(_data.finishedCount != 4) return;
            string name = _data.photoNames[index];
            PhotoItem photo = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState[name];
            UIModule.Instance.ShowAsync<ShowPhoto>(photo);
        }

        private void PlayGame()
        {
            AudioModule.Instance.ClickAudio();
            UIModule.Instance.Close<BeautyDraft>();
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.Level =
                StorageManager.Instance.GetStorage<BaseInfo>().Level;
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName = _data.photoNames[0];
            GameFsm.Instance.ToState<GameStatePlay>(nameText.text);
        }

        private void CurrencyBuy()
        {
            AudioModule.Instance.ClickAudio();
            if (_config.buyType == 0)
            {
                GameIAP.Purchase(_config.buykey, ()=>
                {
                    
                    var dic = new Dictionary<string, object>();
                    dic["name"] = nameText.text;
                    dic["type"] = _config.buyType;
                    TBAMgr.Instance.SendLogEvent("drift",dic);
                    
                    Claim();
                },"beauty");  
            }
            else if (GUtility.IsEnoughItem((ItemType)_config.buyType, _config.price))
            {
                Event.Instance.SendEvent(new SubItem(_config.buyType, _config.price));
                
                var dic = new Dictionary<string, object>();
                dic["name"] = nameText.text;
                dic["type"] = _config.buyType;
                TBAMgr.Instance.SendLogEvent("drift",dic);

                
                Claim();
            }
        }

        private void AdBuy()
        {
            AudioModule.Instance.ClickAudio();
            AdMgr.Instance.PlayRV(() =>
            {
                var dic = new Dictionary<string, object>();
                dic["name"] = nameText.text;
                dic["type"] = _config.buyType;
                TBAMgr.Instance.SendLogEvent("drift",dic);
                Claim();
            },"BeautyAd");
        }

        private void Claim()
        {
            _data.unlock = true;

            StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.ShowOrder.Remove(_config.id.ToString());
            StorageManager.Instance.GetStorage<ActivityInfo>().BeautyInfo.ShowOrder.Insert(0, _config.id.ToString());
            UpdateContent(_index,_data,_config);
            
            GUtility.CheckPhotoState(_data.photoNames[0], (int)PhotoType.BeautyDraft, false, PhotoState.Lock);
            
            Event.Instance.SendEvent(new UpdateBeautyDraftOrder());
        }

        public void UpdateContent(int index, BeautyItemPhotos data, Beauty config)
        {
            _data = data;
            _config = config;
            _index = index;
            button.gameObject.SetActive(!data.unlock || data.finishedCount < 4);

            button.Find("play").gameObject.SetActive(data.unlock && data.finishedCount < 4);
            
            button.Find("ad").gameObject.SetActive(!data.unlock && config.buyType == 3);
            button.Find("buy").gameObject.SetActive(!data.unlock && config.buyType != 3);
            nameText.text = config.name;
            if (!data.unlock)
            {
                photo.SetAsFirstSibling();
                if (config.buyType != 3)
                {
                    button.Find("buy/coin").gameObject.SetActive(config.buyType == (int)ItemType.Coin);
                    button.Find("buy/diamond").gameObject.SetActive(config.buyType == (int)ItemType.Diamond);
                    string str = "";
                    if (config.buyType == 0)
                    {
                        str = IAPManager.Instance.GetLocalizedPrice(config.buykey);
                    }
                    else
                    {
                        str = config.price.ToString();
                    }
                    button.Find("buy/price").GetComponent<TextMeshProUGUI>().text = str.ToString();
                }
            }
            else
            {
                bg.SetAsFirstSibling();
            }

            ShowImg();
            
        }

        private async void ShowImg()
        {
            int showCount = _data.finishedCount == 4 ? 4 : 1;
            for (int i = 0; i < 4; i++)
            {
                if (i < showCount)
                {
                    
                    string name = _data.photoNames[i];
                    name = GUtility.GetPhotoName(name); 
                    string atlas =  GameConfigSys.GetPhotoAtlasName(name);
                    var sp = AssetLoad.Instance.LoadSprite(name, atlas);
                    imgs[i].sprite = sp;
                    GUtility.ApplyAspect(sp,228,278 ,imgs[i].GetComponent<RectTransform>());
                }
                else
                {
                    var sp = GUtility.GetItemIcon(ItemType.Photo, "single");
                    imgs[i].sprite = sp;
                    GUtility.ApplyAspect(sp,228,278 ,imgs[i].GetComponent<RectTransform>());
                }
                
            } 
        }
    }
}