namespace Foundation
{
    public enum LimitedType
    {
        NONE,REGION,VPN,SIM
    }
    public class MonitorManager : SingletonScript<MonitorManager>
    {
        public LimitedType CheckLimited()
        {
            var isNo = MonitorDataUtil.IsGAIDInWhitelist();
            if (isNo)
            {
                return LimitedType.NONE;
            }

            bool ip = MonitorDataUtil.IsChineseIp();
            if (ip)
            {
                return LimitedType.REGION;
            }

            var country = MonitorDataUtil.GetCountry();
            var language = MonitorDataUtil.GetLanguage();
            if (language=="zh")
            {
                if (string.IsNullOrEmpty(country) || country == "CN")
                {
                    return  LimitedType.REGION;
                }
            }

            var proxy = MonitorDataUtil.IsProxy();
            var vpn = MonitorDataUtil.IsVpn();
            if (proxy || vpn)
            {
                return LimitedType.VPN;
            }

            var hasSim = MonitorDataUtil.HasSimCard();
            if (!hasSim)
            {
                return LimitedType.SIM;
            }

            var simMCC = MonitorDataUtil.GetSimMcc();
            if (simMCC == 460)
            {
                return LimitedType.SIM;
            }

            return LimitedType.NONE;
        }
    }
}