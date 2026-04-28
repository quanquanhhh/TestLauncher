using System.Collections.Generic;
using System.Linq;
using Foundation;
using Foundation.AudioModule;
using Foundation.Storage;
using GameConfig;
using GamePlay.Storage;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
 
    public class AlbumsPanel : UIWidget
    {
        public AlbumType _type;
        private PhotoItem photoItem;
        private GridLoop gloop;
        private Dictionary<string, PhotoItem> storageData;
        private List<PhotoItem> photos;
        public override void OnCreate()
        {
            base.OnCreate();
            // gameObject.transform.localScale = ViewUtility.GetEnoughXScale() * Vector3.one;
            _type = (AlbumType)userDatas[0];
            storageData = StorageManager.Instance.GetStorage<PhotoInfo>().PhotoState;
            photos = GetDiffTypePhoto();
            gloop = AddWidget<GridLoop>(gameObject,true, photos, this);
            gloop.SetCount(photos);
        }

        public void RefreshData()
        {
            photos = GetDiffTypePhoto();
            gloop.SetCount(photos);
        }

        public List<string> GetPhotosName()
        {
            var strs = new List<string>();
            foreach (var photo in photos)
            {
                strs.Add(photo.Name);
            }

            return strs;
            
        }
        private List<PhotoItem> GetDiffTypePhoto()
        {
            List<PhotoItem> photoStates = new(); 
            
            foreach (var item in storageData)
            {
                if (string.IsNullOrEmpty(item.Value.Name))
                {
                    item.Value.Name =  item.Key;
                }
                switch (_type)
                {
                    case AlbumType.Level :
                        if (item.Value.State == (int)PhotoState.Unlock &&
                            (item.Value.from == (int)PhotoType.Level1 ||
                             item.Value.from == (int)PhotoType.Level2 ||
                             item.Value.from == (int)PhotoType.Level3))
                        {
                            photoStates.Add(item.Value);
                        }
                        break;
                    case AlbumType.Like:
                        if (item.Value.isLike)
                        {
                            photoStates.Add(item.Value);
                        }
                        break;
                    case AlbumType.Albums:
                        if (item.Value.from != (int)PhotoType.Level1 && 
                            item.Value.from != (int)PhotoType.Level2 && 
                            item.Value.from != (int)PhotoType.Level3 )
                        {
                            photoStates.Add(item.Value);
                        }
                        break;
                }
            }

            if (_type == AlbumType.Like)
            {
                photoStates.Sort((a, b) => b.likeindex.CompareTo(a.likeindex));
            }
            else
            {
                photoStates.Sort((a, b) => b.index.CompareTo(a.index));
            }

            return photoStates;
        }
    }

    public class AlbumsItem
    {
        public AlbumsPanel _panelType;
        public Image image;
        public Transform viptag;
        public Transform videoTag;
        public Transform lockTag;
        public Button likeBtn;
        public Transform likeTag;
        public Transform likeParent;
        public Button playBtn;
        public Button showBtn;
        public bool isLike;

        private RectTransform mask;
        
        public int index;
        private PhotoItem _photoItem;
        public AlbumsItem(GameObject item, AlbumsPanel _panel)
        {
            _panelType = _panel;
            mask = item.transform.Find("mask").GetComponent<RectTransform>();
            image = item.transform.Find("mask/Img").GetComponent<Image>();
            viptag = item.transform.Find("VIP");
            videoTag = item.transform.Find("Video");
            lockTag = item.transform.Find("Lock");
            likeBtn = item.transform.Find("Like/likeBtn").GetComponent<Button>();
            likeTag = item.transform.Find("Like/light");
            likeParent = item.transform.Find("Like");
            playBtn = item.transform.Find("Play").GetComponent<Button>();
            showBtn = item.transform.Find("Show").GetComponent<Button>();
            likeBtn.onClick.AddListener(OnLikeImage);
            playBtn.onClick.AddListener(ToUnlockImg);
            showBtn.onClick.AddListener(ToShowImg);
        }

        private void ToShowImg()
        {
            AudioModule.Instance.ClickAudio();
            var names = _panelType.GetPhotosName();
            UIModule.Instance.ShowAsync<ShowPhoto>(_photoItem, names);
        }

        private void ToUnlockImg()
        {
            AudioModule.Instance.ClickAudio();
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.PhotoName = _photoItem.Name ;
            StorageManager.Instance.GetStorage<BaseInfo>().CurrentLevel.Level = StorageManager.Instance.GetStorage<BaseInfo>().Level;
             
            GameFsm.Instance.ToState<GameStatePlay>(); 
            UIModule.Instance.Close<AlbumsView>();
        }

        private void OnLikeImage()
        {
            AudioModule.Instance.ClickAudio();
            isLike = !isLike;
            _photoItem.isLike = isLike;
            likeTag.gameObject.SetActive(isLike);
            StorageManager.Instance.GetStorage<PhotoInfo>().LikeCount += (isLike ? 1 : -1);
            if (isLike)
            {
                _photoItem.likeindex = StorageManager.Instance.GetStorage<PhotoInfo>().LikeCount;
                
                var dic = new Dictionary<string, object>();
                dic["name"] = _photoItem.Name; 
                TBAMgr.Instance.SendLogEvent("like", dic);
            }
            else
            {
                _photoItem.likeindex = 0;
            }

            if (_panelType._type == AlbumType.Like)
            {
                _panelType.RefreshData(); 
            }
        }

        public async void UpdateContent(int i, PhotoItem photoItem)
        {
            index = i;
            _photoItem =  photoItem;
            isLike = photoItem.isLike;
            
            
            var str = GUtility.GetPhotoName(photoItem.Name);
            // Texture texture = await AssetLoad.Instance.LoadAsset<Texture>(str);

            var atlas = GameConfigSys.GetPhotoAtlasName(str);
            Sprite sp = AssetLoad.Instance.LoadSprite(str, atlas);
            
            image.sprite =  sp;
            
            GUtility.ApplyAspect(sp, mask, image.GetComponent<RectTransform>());
            viptag.gameObject.SetActive(false);
            var video = photoItem.Name.ToLower().Contains(".mp4");
            videoTag.gameObject.SetActive(video);
            lockTag.gameObject.SetActive(_photoItem.State == (int)PhotoState.Lock);
            playBtn.gameObject.SetActive(_photoItem.State == (int)PhotoState.Lock);
            showBtn.gameObject.SetActive(_photoItem.State != (int)PhotoState.Lock && photoItem.State != (int)PhotoState.None);
            likeParent.gameObject.SetActive(_photoItem.State != (int)PhotoState.Lock);
            likeTag.gameObject.SetActive(isLike);
        }
    }
}