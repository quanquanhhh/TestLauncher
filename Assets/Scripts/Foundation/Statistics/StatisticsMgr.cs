using Foundation.Statistics.Facebook;

namespace Foundation.Statistics
{
    public class StatisticsMgr : SingletonScript<StatisticsMgr>
    {
        public void StatisticsGameOpen(bool first)
        {
            FacebookMgr.Instance.StatrPlayGame();
            if (first)
            {
                TBAMgr.Instance.SendLogEvent("first_play");
            }
        }

        public void StatisticsAdRevenue(  MaxSdkBase.AdInfo adInfo)
        {
            
            TBAMgr.Instance.SendAdGetReward(adInfo);
            FirebaseMgr.FirebaseMgr.ReportAdRevenue(adInfo);
            AdjustManager.Instance.LogAdjustRevenue(adInfo,"USD"); 
        }
        
    }
}