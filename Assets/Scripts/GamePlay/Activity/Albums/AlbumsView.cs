using Foundation;
using GamePlay.UIMain.Widget;
using UnityEngine;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    public enum AlbumType
    {
        Level,
        Albums,
        Like
        
    }
    [Window("Albums", WindowLayer.Popup)]
    public class AlbumsView : UIWindow
    {
        [UIBinder("CloseBtn")] private Button closeBtn;
        [UIBinder("Content")] private  RectTransform _content;
        [UIBinder("Btn","Levels")] private Button levels;
        [UIBinder("Btn", "Albums")] private Button albums;
        [UIBinder("Btn", "Likes")] private Button likes;

        [UIBinder("Levels")] private Transform levelTop;
        [UIBinder("Albums")] private Transform albumTop;
        [UIBinder("Likes")] private Transform likesTop;
        [UIBinder("LevelsPanel")] private GameObject levelPanel;
        [UIBinder("AlbumsPanel")] private GameObject albumPanel;
        [UIBinder("LikePanel")]   private GameObject likePanel;

        [UIBinder("Coin")] private GameObject coin;
        [UIBinder("Diamond")] private GameObject diamond;
        private AlbumsPanel likepanel;
        private AlbumsPanel levelpanel;
        private AlbumsPanel albumspanel;

        private AlbumsPanel current;
        public override void OnCreate()
        {
            base.OnCreate();
            _content.offsetMax -= new Vector2(0, ViewUtility.AdjustTopHeight);
            closeBtn.onClick.AddListener(Close);
            
            levels.onClick.AddListener(ShowLevelPanel);
            albums.onClick.AddListener(ShowAlbumsPanel);
            likes.onClick.AddListener(ShowLikesPanel);

            
            levelpanel =  AddWidget<AlbumsPanel>(levelPanel.gameObject, true, AlbumType.Level);
            albumspanel = AddWidget<AlbumsPanel>(albumPanel.gameObject, true, AlbumType.Albums);
            likepanel = AddWidget<AlbumsPanel>(likePanel.gameObject, true, AlbumType.Like);
            ShowPanel(AlbumType.Level);

            AddWidget<CoinWidget>(coin);
            AddWidget<DiamondWidget>(diamond);
        }

        private void ChangeTopState(AlbumType albumType)
        {
            levelTop.Find("dark").gameObject.SetActive(albumType != AlbumType.Level);
            albumTop.Find("dark").gameObject.SetActive(albumType != AlbumType.Albums);
            likesTop.Find("dark").gameObject.SetActive(albumType != AlbumType.Like);
            
            levelTop.Find("select").gameObject.SetActive(albumType == AlbumType.Level);
            albumTop.Find("select").gameObject.SetActive(albumType == AlbumType.Albums);
            likesTop.Find("select").gameObject.SetActive(albumType == AlbumType.Like);
            
        }
        public void ShowPanel(AlbumType albumType)
        { 
            ChangeTopState(albumType);
            levelPanel.SetActive(albumType ==  AlbumType.Level);
            albumPanel.SetActive(albumType ==  AlbumType.Albums);
            likePanel.SetActive(albumType == AlbumType.Like);
            switch (albumType)
            {
                case AlbumType.Like:
                    likepanel.RefreshData();
                    current = likepanel;
                    break;
                case AlbumType.Level:
                    levelpanel.RefreshData();
                    current = levelpanel;
                    break;
                case AlbumType.Albums :
                    albumspanel.RefreshData();
                    current = albumspanel;
                    break;
            }
        }

        public void UpdateAllPanel()
        { 
            current.RefreshData();
        }
        
        private void ShowLikesPanel()
        {
            ShowPanel(AlbumType.Like); 
        }
        private void ShowAlbumsPanel()
        {
            ShowPanel(AlbumType.Albums);  
        }
        private void ShowLevelPanel()
        {
            ShowPanel(AlbumType.Level);
        }
        
    }
}