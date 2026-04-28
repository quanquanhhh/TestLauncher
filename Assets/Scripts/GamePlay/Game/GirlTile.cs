using System.Collections.Generic;
using Foundation;
using Foundation.Pool;
using GamePlay.Game.Data;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Event = Foundation.Event;

namespace GamePlay.Game
{
    public class GirlTile : UIWidget
    {
        [UIBinder("Button")] private Button btnSelf;
        [UIBinder("Icon")] private Image ImgIcon;
        [UIBinder("Cover")] private GameObject ImgCover;
        [UIBinder("Spine")] private SkeletonGraphic spine;


        public bool moving = false;
        private bool shown = true;
        public int index = -1;
        public List<string> debugtest = new();
        private GirlTileData _data;
        public bool readyRemove;
        public override void OnCreate()
        {
            base.OnCreate();
            spine.gameObject.SetActive(false);
            btnSelf.onClick.AddListener(OnClickBtnTile); 
        }

        public override void OnDestroy()
        {
            Release();
            PoolManager.Instance.BackToPool("TileItem", gameObject); 
            base.OnDestroy();
            
        }

        public async void InitTile(GirlTileData data, int index)
        {
            debugtest.Add(data.Icon.ToString()); 
            _data = data;
            string name = TileManager.Instance.GetTileName(data.Icon);
            var sp = AssetLoad.Instance.LoadSprite(name,"TileGame");
            this.index = index; 
            ImgIcon.sprite = sp;
            rectTransform.name = index.ToString();
            gameObject.SetActive(true);
        }

        public void InitIcon(int nType)
        {
            _data.Icon = nType;
            string name = TileManager.Instance.GetTileName(_data.Icon);
            var sp = AssetLoad.Instance.LoadSprite(name,"TileGame");
            ImgIcon.sprite = sp;
            debugtest.Add("name2" + _data.Icon.ToString());
        }
        private void OnClickBtnTile()
        {
            if (moving || !TileManager.Instance.TouchEnable|| _data == null) return;
            
            Event.Instance.SendEvent(new ClickTileEvent(this));
        }
 
        public void SetTileShown(bool isshow)
        {
            shown = isshow;
            SetBtnEnable(isshow);
            ImgCover.SetActive(!isshow);
        }

        public void SetBtnEnable(bool isshow)
        {
            btnSelf.enabled = isshow;
        }

        public bool IsTileShown()
        {
            return shown;
        }

        public GirlTileData GetTileData()
        {
            return _data;
        }

        public void SetRemoveing(bool isRemove)
        {
            readyRemove = isRemove;
            
        }
        public bool IsAround(GirlTile tile)
        {
            GirlTileData data = tile.GetTileData();

            int nDeltaX1 = _data.X - data.X - TileManager.TileWidth;
            if (nDeltaX1 > -5) return false;

            int nDeltaX2 = data.X - _data.X - TileManager.TileWidth;
            if (nDeltaX2 > -5) return false;

            int nDeltaY1 = _data.Y - data.Y - TileManager.TileHeight;
            if (nDeltaY1 > -5) return false;

            int nDeltaY2 = data.Y - _data.Y - TileManager.TileHeight;
            if (nDeltaY2 > -5) return false;

            return true;
        }

        public void PlayClickEffect()
        {
            spine.gameObject.SetActive(true);
            spine.PlayAsync("animation", false, () =>
            {
                spine.gameObject.SetActive(false);
            });
        }
        public void SetStatus(bool m)
        {
            moving = m;
            if(m)
            {
                Event.Instance.SendEvent(new UpdateThirdAreaEvent()); 
            }
        }

        public void Release()
        {
            _data = null;
            index = -1;
        }
    }
}