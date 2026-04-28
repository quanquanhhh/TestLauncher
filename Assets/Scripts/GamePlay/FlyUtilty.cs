using System;
using System.Collections.Generic;
using DG.Tweening;
using Foundation;
using Foundation.AudioModule;
using Foundation.Pool;
using GameConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GamePlay
{
    public class FlyUtilty
    { 
        public static Transform FlyRoot;
        public static async  void FlyItem(int itemid, int amount, Vector3 startPos, Vector3 endPos,float time =  0.8f, Action firstFlyEndAction = null,Action lastFlyEndAction = null)
        {
            var item = GameConfigSys.GetItemConfig(itemid);
            string pic = item.name + "_icon";
            
            int count = Math.Min(amount, 10);
            float delay = 0;
            for (int i = 0; i < count; i++)
            { 
                var obj = PoolManager.Instance.GetPoolObj("FlyGameobject");
                obj.transform.SetParent(FlyRoot);
                
                Sprite img = AssetLoad.Instance.LoadSprite(pic);
                obj.transform.Find("icon").GetComponent<Image>().sprite = img;
                // obj.transform.Find("amount").GetComponent<TextMeshProUGUI>().text = "X"+amount.ToString();

                int temp = i;
                PlayAudioEffect(itemid);
                FlyObj(obj, startPos, endPos, delay,time, () =>
                {
                    if (temp == 0)
                    {
                        firstFlyEndAction?.Invoke();
                    }
                    else if (temp == count - 1)
                    {
                        lastFlyEndAction?.Invoke();
                    }
                });
                delay += 0.15f;
            }
        }

        private static void PlayAudioEffect(int itemid)
        {
            switch (itemid)
            {
                case (int)ItemType.Coin:
                    AudioModule.Instance.PlaySfx("coins");
                    break;
                case (int)ItemType.Diamond:
                    AudioModule.Instance.PlaySfx("Gem");
                    break;
            }
        }

        private static void FlyObj(GameObject obj, Vector3 startPos, Vector3 endPos,float delay, float time, Action endAction)
        {
            float z = RootManager.UIRoot.position.z;
            startPos.z = z;
            
            startPos.x += Random.Range(-0.5f, 0.5f);
            startPos.y += Random.Range(-0.2f, 0.2f);

            endPos.z = z;
            
            obj.transform.position = startPos;
            obj.transform.localScale = Vector3.zero;
            
            
            Vector3 start = startPos;
            start.z = z;
            
            Vector3 control = Vector3.zero;
            control.x = start.x + 0.3f;
            control.y = start.y - 0.3f;
            control.z = z;
            
            
            Vector3 control1 = Vector3.MoveTowards(control, endPos, 1);

            obj.SetActive(true);

            Sequence s = DOTween.Sequence();
            s.SetDelay(delay);
            s.Append(obj.transform.DOScale(new Vector3(1, 1, 1), 0.3f));
            s.Append(obj.transform.DOPath(new[] {startPos, control, control1, endPos}, time, PathType.CatmullRom)
            .SetEase(Ease.InQuad));
            // s.Append(obj.transform.DOMove(endPos, 0.8f).SetEase(Ease.Linear)
                // .OnUpdate(() => Debug.Log(Time.frameCount)));
            s.OnComplete(() =>
            { 
                PoolManager.Instance.BackToPool("FlyGameobject", obj); 
                endAction?.Invoke();
            });
            s.Play(); 
        }
    }
}