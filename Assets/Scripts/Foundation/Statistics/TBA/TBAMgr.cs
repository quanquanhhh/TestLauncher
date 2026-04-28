using System;
using System.Collections.Generic;
using GameConfig;
using UnityEngine;
using XGame.Scripts.IAP;

namespace Foundation
{
    public class TBAMgr : SingletonComponent<TBAMgr>
    {
        public int isHit = -1; //- 0 命中  1 未命中
        public bool cloakBack = false;
        
        
        private static string UserIDFA = "";//Device.advertisingIdentifier;
        private static string UserIDFV = "";// Device.vendorIdentifier;
        private const string MessageReleaseUrl = "https://sabina.desiregirls.net/whimper/delete/jill";
 
        private const string gamename = "com.Desire.Girls.Mahjong.Match.Tile";
        private Dictionary<string, string> topDic = new Dictionary<string, string>();
 
        public void SendInstallEvent()
        {
            Dictionary<string, object> install  = new Dictionary<string, object>();
            install ["ambiance"] = "build /2022.3.61f1".EncodingText();
            install["entirety"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
            install["unkempt"] = "clique".EncodingText();
            install["waste"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            install["whinny"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            install["reuben"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            install["gauze"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            install["prow"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            install["gunfire"] = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            
             
            SendPostEvent(install, "Install", true, "for");
        }

        private void SendPostEvent(Dictionary<string, object> connect, string posId, bool IsAlone,string aloneName = "")
        { 
            string logId = Guid.NewGuid().EncodingText();

            Dictionary<string, object> ball = new Dictionary<string, object>();
            ball["shipmate"]   = gamename.EncodingText();
            ball["veridic"] = Application.version.EncodingText().EncodingText();
            ball["position"]  =  new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            ball["cacti"] = SystemInfo.operatingSystem.EncodingText();
            
            Dictionary<string, object> bill = new Dictionary<string, object>();
            bill["libretto"]  = "cameo".EncodingText();
            bill["huston"]  = SystemInfo.deviceUniqueIdentifier.EncodingText();; 
            bill["spheroid"]   = "mcc".EncodingText();
            bill["parquet"]   =  SystemInfo.deviceUniqueIdentifier.EncodingText(); 
            
            Dictionary<string, object> through = new Dictionary<string, object>();
            through["happen"]  = Guid.NewGuid().EncodingText();;
            through["vaduz"]  =  "gp".EncodingText(); 
            through["vegetate"]   = SystemInfo.deviceModel.EncodingText();
            through["hippo"]   =  "en_US".EncodingText(); 
             
            
            Dictionary<string, object> sendDic = new Dictionary<string, object>();
            
            sendDic.Add("ball",  ball);
            sendDic.Add("bill",  bill);
            sendDic.Add("through",  through);

            if (IsAlone)
            {
                sendDic.Add(aloneName,connect);
            }
            else
            {
                foreach (var item in connect)
                {
                    sendDic.Add(item.Key, item.Value);
                }
            }
            MessageManager.Instance.Post<GetData>(
                MessageReleaseUrl, logId, posId, topDic, sendDic, 60);
        }



        public void SendSessionEvent()
        {
            Dictionary<string, object> baseInfo = new Dictionary<string, object>();
            SendPostEvent(baseInfo, "Session", true, "forge");
        }

        private string RevenueStr = "";
        private double Revenue = 0;

        public void SendAdGetReward(MaxSdkBase.AdInfo adInfo)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            dic.Add("decree",  "mare");
            dic.Add("decease", (long)(adInfo.Revenue * 1e6));
            dic.Add("alto", "USD".EncodingText());
            dic.Add("toxic", adInfo.NetworkName.EncodingText());
            dic.Add("boogie", "max".EncodingText());
            dic.Add("dessert", adInfo.AdUnitIdentifier.EncodingText());
            dic.Add("ranch", adInfo.Placement.EncodingText());
            dic.Add("coronary", adInfo.AdFormat.EncodingText());

            SendPostEvent(dic, adInfo.Placement, false);
        }
 
        public void SendLogPurchase(double price, string productName, string pos, string c)
        {
            var config = IAPManager.Instance.getPriceConfig(productName);
            var dic = new Dictionary<string, object>();
            dic.Add("price", price.ToString());
            dic.Add("price2", price);
            dic.Add("price3", config);
            dic.Add("productId",productName);
            dic.Add("iso", c.ToString());
            dic.Add("pos",pos);
            SendLogEvent("purchase", dic);
            
            var dic2 = new Dictionary<string, object>();
            dic2.Add("price", price);
            dic2.Add("productId",productName);
            dic2.Add("pos",pos);
            SendLogEvent("buy", dic2);
            // SendLogEvent("_" + productName);
        }
        public void SendLogEvent(string eventName, Dictionary<string, object> arrValue = null)
        { 
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["decree"] = eventName; 
            dic[eventName] = arrValue;
            
            // if (arrValue != null)
            // {
            //     foreach (var item in arrValue)
            //     {
            //         dic.Add(item.Key, item.Value);
            //     }
            //     dic.Add(eventName,baseInfo);
            // }
            
            SendPostEvent(dic, eventName, false);
        } 
    }
}