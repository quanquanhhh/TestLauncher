using Foundation;
using GamePlay.Component;
using UnityEngine;

namespace GamePlay.UIMain
{
    public class ShowPhotoCell
    {
        private GameObject _gameObject;
        private UguiMediaSource ugui;
        
        public ShowPhotoCell(GameObject obj)
        {
            _gameObject = obj;
            ugui = _gameObject.TryGetOrAddComponent<UguiMediaSource>();
        }

        public void UpdateContent(string name)
        {
            
            var isvideo = name.ToLower().Contains("mp4");
            name = GUtility.GetPhotoName(name);
            ugui.SetSource(name,isvideo);

        }
    }
}