using System.Collections.Generic;

namespace GameConfig
{
    public partial  class GameConfigSys
    {
        public static Dictionary<PhotoType, bool> _activityGuides = new Dictionary<PhotoType, bool>();

        public static void SetGuide()
        {          
            _activityGuides[PhotoType.BeautyDraft] = _config.Beauty[0].HasGuide;
            _activityGuides[PhotoType.Sign] = _config.Sign.HasGuide;
            _activityGuides[PhotoType.LimitedGift] = _config.LimitPack.HasGuide;
            _activityGuides[PhotoType.LuckyWheel] = _config.Wheel.HasGuide;
            _activityGuides[PhotoType.SecretGift] = _config.SecretGift[0].HasGuide;
            _activityGuides[PhotoType.DailyChallenge] = true;
            _activityGuides[PhotoType.Pass] = _config.Pass[0].HasGuide;
            // _activityGuides[PhotoType.DraftPackage] = _config.DraftPackage.HasGuide;
            
        }
    }
}