// using System;
// using System.Collections.Generic;
// using System.Threading;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
//
// namespace Foundation
// {
//     public enum InitAfStatus
//     {
//         Wait = -1,
//         Skip = 0,
//         Init = 1,
//     }
//     public class ShavingManager : SingletonComponent<ShavingManager>
//     {
//         const string InitAFStatusKey = "InitAFStatus";
//         const string InitAFNeedAdKey = "InitAFNeedAd";
//
//         // const string ProjectID = "Rabbit";        
//         const string InitAFAdTimeKey = "InitAFAdTime";
//         const string WaitAFInitADRevenueKey = "WaitAFInitADRevenue";
//         public float WaitAFInitADRevenueValue
//         {
//             get
//             {
//                 string sKey = GameCommon.GameName + WaitAFInitADRevenueKey;
//                 return PlayerPrefs.GetFloat(sKey, 0f);
//             }
//             set
//             {
//                 string sKey =  GameCommon.GameName + WaitAFInitADRevenueKey;
//                 PlayerPrefs.SetFloat(sKey, value);
//             }
//         }
//
//         
//         public string InitAFAdTimeValue
//         {
//             get
//             {
//                 string sKey =  GameCommon.GameName + InitAFAdTimeKey;
//                 return PlayerPrefs.GetString(sKey, "");
//             }
//             set
//             {
//                 string sKey =  GameCommon.GameName + InitAFAdTimeKey;
//                 PlayerPrefs.SetString(sKey, value);
//             }
//         }
//         public int InitAFNeedAdValue
//         {
//             get
//             {
//                 string sKey =  GameCommon.GameName + InitAFNeedAdKey;
//                 return PlayerPrefs.GetInt(sKey, -1);
//             }
//             set
//             {
//                 string sKey =  GameCommon.GameName + InitAFNeedAdKey;
//                 PlayerPrefs.SetInt(sKey, value);
//             }
//         }
//         public InitAfStatus InitAFStatusValue//-1 wait，0 skip，1 init
//         {
//             get
//             {
//                 string sKey =  GameCommon.GameName + InitAFStatusKey;
//                 int nValue = PlayerPrefs.GetInt(sKey, -1);
//                 return (InitAfStatus)nValue;
//             }
//             set
//             {
//                 int nValue = (int)value;
//                 string sKey =  GameCommon.GameName + InitAFStatusKey;
//                 PlayerPrefs.SetInt(sKey, nValue);
//             }
//         }
//         private CancellationTokenSource _countCts;
//
//         private List<string> UserFrom;
//         private List<int> UserNeedRv;
//         private List<int> UserWeight;
//
//         public void SetUserInfo(List<string> from, List<int> needrv, List<int> weight)
//         {
//             UserFrom = from;
//             UserNeedRv = needrv;
//             UserWeight = weight;
//         }
//
//         public override void OnDestroy()
//         {
//             base.OnDestroy();
//             StopCountTime();
//         }
//
//         public void RepeatCountTime()
//         {
//             StopCountTime();
//             _countCts = new CancellationTokenSource();
//             CountLoopScaledAsync(_countCts.Token).Forget();
//         }
//         public void StopCountTime()
//         {
//             if (_countCts != null)
//             {
//                 _countCts.Cancel();
//                 _countCts.Dispose();
//                 _countCts = null;
//             }
//         }
//
//         private async UniTask CountLoopScaledAsync(CancellationToken token)
//         {
//             CountTime(); // 立刻执行一次
//
//             try
//             {
//                 while (!token.IsCancellationRequested)
//                 {
//                     await UniTask.Delay(1000, DelayType.DeltaTime, cancellationToken: token);
//                     CountTime();
//                 }
//             }
//             catch (OperationCanceledException) { }
//         }
//         
//         public void OnInitialization()
//         { 
//             switch (InitAFStatusValue)
//             {
//                 case InitAfStatus.Skip:
//                     return;//skip
//                 case InitAfStatus.Init:
//                     ToInitAdjust();
//                     return;//init
//             } 
//             string sEndTime = InitAFAdTimeValue;
//             if (!string.IsNullOrEmpty(sEndTime))
//             {
//                 // InvokeRepeating("CountTime", 0, 1);
//                 RepeatCountTime();
//                 return;
//             } 
//             CheckShaving();
//         }
//         
//         public bool CheckShaving()
//         {
//             //获取url
//             string sUrl = ""; 
//             sUrl = MonitorDataUtil.GetReferrer_url(); 
//             //当url为 空 或 无意义
//             if (string.IsNullOrEmpty(sUrl) || sUrl.Length <= 3)
//             {
//                 InitAFStatusValue = InitAfStatus.Init;
//                 ToInitAdjust();
//                 return false;
//             }
//             
//             //当不在渠道里，去初始化AF
//             int nIndex = GetTargetIndex(sUrl);
//             if (nIndex == -1)
//             {
//                 InitAFStatusValue = InitAfStatus.Init;
//                 ToInitAdjust();
//                 return false;
//             }
//             //当在渠道里
//             //当RV和Weight都为-1时，去初始化AF
//             int nNeedRV = GetUserNeedRv(nIndex);
//             int nWeight = GetUserWeight(nIndex);
//             if (nNeedRV == -1 && nWeight == -1)
//             {
//                 InitAFStatusValue = InitAfStatus.Init;
//                 ToInitAdjust();
//                 return false;
//             }
//             //当Weight大于0时
//             if (nWeight >= 0)
//             {
//                 int nRandom = UnityEngine.Random.Range(0, 100);
//                 if (nRandom < nWeight)
//                 {
//                     InitAFStatusValue = InitAfStatus.Init;//概率内，初始化
//                     ToInitAdjust();
//                     return true;
//                 }
//                 InitAFStatusValue = InitAfStatus.Skip;
//                 return false;
//             }
//             //当RV大于0时,设置好时间和次数，开始计时检测
//             if (nNeedRV > 0)
//             {
//                 InitAFAdTimeValue = DateTime.Now.AddDays(1).ToString();
//                 InitAFNeedAdValue = nNeedRV;
//                 
//                 RepeatCountTime();
//                 // InvokeRepeating("CountTime", 0, 1);
//                 return false;
//             }
//             //常理不会进入
//             InitAFStatusValue = InitAfStatus.Init;
//             ToInitAdjust();
//             return false;
//         }
//         private int GetTargetIndex(string mtarget)
//         {
//             var UserFrom = GetUserFromList();
//             for (int i = 0; i < UserFrom.Count; i++)
//             {
//                 int nIndex = i;
//                 var target = UserFrom[nIndex];
//                 if (mtarget.Contains(target))
//                 {
//                     return nIndex;
//                 }
//             }
//             return -1;
//         }
//         private void CountTime()
//         {
//             string sEndTime = InitAFAdTimeValue;
//             DateTime endTime = DateTime.Parse(sEndTime);
//             if (DateTime.Now > endTime)//超出24小时,不初始化AF
//             {
//                 InitAFStatusValue = InitAfStatus.Skip;
//                 InitAFAdTimeValue = "";
//                 InitAFNeedAdValue = -1;
//                 // CancelInvoke("CountTime");
//                 StopCountTime();
//                 return;
//             }
//             if (InitAFNeedAdValue < 1)//看够所需RV，去初始化AF
//             {
//                 InitAFStatusValue = InitAfStatus.Init;
//                 InitAFAdTimeValue = "";
//                 InitAFNeedAdValue = -1;
//                 ToInitAdjust();
//                 
//                 StopCountTime();
//                 // CancelInvoke("CountTime");
//                 return;
//             }
//             //继续等待
//         }
//         private void LogWaitAfAdRevanue()
//         {
//             AdjustManager.Instance.LogAdjustRevenue2(WaitAFInitADRevenueValue);
//         }
//  
//         //--------------------------------------------------------------------
//         public void ShownAD()
//         {
//             string sEndTime = InitAFAdTimeValue;
//             if (string.IsNullOrEmpty(sEndTime))
//             {
//                 return;
//             }
//
//             InitAFNeedAdValue--;
//             if (InitAFNeedAdValue <= 0)
//             {
//                 InitAFStatusValue = InitAfStatus.Init;
//                 InitAFAdTimeValue = "";
//                 ToInitAdjust();
//                 StopCountTime();
//                 // CancelInvoke("CountTime");
//             }
//         }
//         //所有的af上报都走这个位置
//         public void LogAfAdRevenue(MaxSdkBase.AdInfo adInfo)
//         {
//             switch (InitAFStatusValue)
//             {
//                 case InitAfStatus.Wait:
//                     WaitAFInitADRevenueValue += (float)adInfo.Revenue;
//                     break;
//                 case InitAfStatus.Init:
//                     AdjustManager.Instance.LogAdjustRevenue(adInfo,"USD"); 
//                     break;
//             }
//         }
//
//         private void ToInitAdjust()
//         {
//             AdjustManager.Instance.InitAdjust();
//
//             UniTaskMgr.Instance.WaitForSecond(5f, LogWaitAfAdRevanue, SingleObj.GetCancellationTokenOnDestroy()).Forget(); 
//         }
//         private List<string> GetUserFromList()
//         {
//             return UserFrom; 
//         }
//         private int GetUserNeedRv(int nIndex)
//         {
//             return UserNeedRv[nIndex];
//         }
//         private int GetUserWeight(int nIndex)
//         {
//             return UserWeight[nIndex];
//         }
//         
//     }
// }