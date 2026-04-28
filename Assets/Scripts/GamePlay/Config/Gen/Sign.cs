/************************************************
 * Config class : Activity-Sign
 * This file is can not be modify !!!
 ************************************************/

using System;
using System.Collections.Generic;

namespace GameConfig
{
    public class Sign
    {
        /// <summary>
        /// OpenLevel
        /// </summary>
        public int OpenLevel { get; set; }

        /// <summary>
        /// SignRewardType
        /// </summary>
        public List<int> SignRewardType { get; set; }

        /// <summary>
        /// SignRewardNum
        /// </summary>
        public List<int> SignRewardNum { get; set; }

        /// <summary>
        /// HasGuide
        /// </summary>
        public bool HasGuide { get; set; }
    }
}
