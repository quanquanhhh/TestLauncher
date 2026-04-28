using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Foundation
{
    public static class CommonUtility
    {
        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);

                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
        public static List<int> GetWeightIndex(List<int> weight, int count = 1)
        {
            int allweight = 0;
            foreach (var i in weight)
            {
                allweight += i;
            }
            List<int> result = new List<int>();
            for (int i = 0; i < count; i++)
            {
                int rnd = Random.Range(0, allweight);
                
                for (int index = 0; index < weight.Count; index++)
                {
                    if (result.Contains(index))
                    {
                        continue;
                    }
                    rnd -= weight[index];
                    if (rnd <= 0)
                    {
                        allweight -= weight[index];
                        result.Add(index);
                        break;
                    }
                }
            }
            return result;
        }
    }
}