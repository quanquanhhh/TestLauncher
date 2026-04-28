/************************************************
 * Config class : BaseGame-AdConfig
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class AdConfig
    {
        /// <summary>
        /// ivImgRandom
        /// </summary>
        public List<int> ivImgRandom { get; set; }

        /// <summary>
        /// iv概率
        /// </summary>
        public List<int> ivRate { get; set; }

        /// <summary>
        /// 前X天的保护次数
        /// </summary>
        public List<int> ivProtectNum { get; set; }

        /// <summary>
        /// 看n个RV，保护m次IV
        /// </summary>
        public List<int> rvProtectIv { get; set; }

        /// <summary>
        /// 看n个iV，间隔m次IV
        /// </summary>
        public List<int> ivInterval { get; set; }

        /// <summary>
        /// 是否开启iv
        /// </summary>
        public bool ivOpen { get; set; }

        /// <summary>
        /// 是否开启开屏广告
        /// </summary>
        public bool openAd { get; set; }
    }
}
