/************************************************
 * Config class : Level-Difficult
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Difficult
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 奖励
        /// </summary>
        public int rewardType { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int reward { get; set; }

        /// <summary>
        /// 难度
        /// </summary>
        public int difficult { get; set; }
    }
}
