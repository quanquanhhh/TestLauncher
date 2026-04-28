using System;
using Cysharp.Threading.Tasks;
using Foundation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GamePlay.UIMain
{
    public class GuideMask: UIWidget
    {
        [UIBinder("CircleImage")] private Image _showImg;
        public Transform[] _targets;

        private Material _material;
        private float _current = 0f;
        private float _yVelocity = 0f; 

        private float _width;
        private float _height;
        private Vector2 _center;
        private Vector2 _centerScreenPos;
        private bool _targetMoving;


        private Vector3[] _tempWorldCorners = new Vector3[4];

        private Canvas _canvas;
        private Camera _camera; 

        public Action<PointerEventData> OnMaskClick;
        private double _updateTargetDelta;
        public override void OnCreate()
        {
            base.OnCreate();
            _canvas = GameObject.Find("UICanvas").GetComponent<Canvas>();
            _camera = RootManager.UICamera;
            _material = _showImg.material; 
        } 
        public void SetTarget(Transform target)
        {  
            _updateTargetDelta = 1f;
  

            _targetMoving = true;

            UniTaskMgr.Instance.WaitForSecond(5, () => { _targetMoving = false; }).Forget();

            _targets = new Transform[1];
            _targets[0] = target; 

            _material.color = Color.white;
            _showImg.gameObject.SetActive(false); 

            updateMaterialSettings();
        }
        private void updateMaterialSettings()
        {
            updateTargetSizeAndPos();

            _material.SetVector("_Center", _center);
            _material.SetFloat("_width", _current * _width);
            _material.SetFloat("_height", _current * _height);
        }
        private void updateTargetSizeAndPos()
        { 

            var worldCenter = Vector3.zero;
            if (_targets.Length > 0 && _targets[0] != null)
            {
                var target = _targets[0] as RectTransform;
                     
                _width = target.sizeDelta.x / 2f;
                _height = target.sizeDelta.y / 2f; 
                
                target.GetWorldCorners(_tempWorldCorners);
                worldCenter = (_tempWorldCorners[0] + _tempWorldCorners[2]) / 2f;
                _centerScreenPos = _camera.WorldToScreenPoint(new Vector3(worldCenter.x, worldCenter.y, 0)); 

                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform,
                    _centerScreenPos, _camera, out var v3);
                _center = v3;

                _current = 1.2f;
            }

            _showImg.rectTransform.sizeDelta = new Vector2(_width * 2, _height * 2);
            _showImg.transform.position = worldCenter;
            _showImg.transform.localScale = Vector3.one * _current; 
        }
        // public void OnPointerDown(PointerEventData eventData)
        // {
        //     Debug.Log("123123");
        // }
        //
        // public void OnPointerClick(PointerEventData eventData)
        // {
        //     
        //     Debug.Log("456456");
        // }
        //
        // public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        // {
        //     foreach (var target in _targets)
        //     {
        //         var clickInCircle = false;
        //         if (target is RectTransform)
        //         {
        //             clickInCircle =
        //                 RectTransformUtility.RectangleContainsScreenPoint((target as RectTransform), sp, _camera);
        //         }
        //         else
        //         {
        //             clickInCircle = Vector2.Distance(sp, _centerScreenPos) < _height ;
        //         }
        //
        //         if (clickInCircle)
        //         {
        //             //false表示没有点中遮罩，不响应点击，透过
        //             return false;
        //         }
        //     }
        //
        //     return true;
        // }
    }
}