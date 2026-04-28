/************************************************
 * Config class : Photo-Photo
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Photo
    {
        /// <summary>
        /// 图片名字
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 来源（
// 0 三选一 -1  100
// 1 三选一 -2  100
// 2 三选一 -3  100
// 3 签到       50
// 4 限时礼包   24
// 5 商城       50 
// 6 选秀       
// 7 VIP
// 8 幸运转盘
// 9 神秘礼包 
// 10每日挑战
// 11pass）
        /// </summary>
        public int sourceFrom { get; set; }

        /// <summary>
        /// BundleTag
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 其他信息
        /// </summary>
        public string other { get; set; }
    }
}
