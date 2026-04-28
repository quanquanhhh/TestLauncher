using System.Collections.Generic;
using Foundation;
using GamePlay.Component;
using UnityEngine;

namespace GamePlay.UIMain
{
    [Window("TestPanel", WindowLayer.Popup)]
    public class TestPanel : UIWindow
    {
        [UIBinder("Flow")] private RectTransform flow;
        public override void OnCreate()
        {
            base.OnCreate();
            var fl = flow.TryGetOrAddComponent<UICoverFlow>();
            List<RectTransform> childs = new();
            foreach (RectTransform child in flow)
            {
                childs.Add(child);
            }
            fl.SetContent(childs);
        }
    }
}