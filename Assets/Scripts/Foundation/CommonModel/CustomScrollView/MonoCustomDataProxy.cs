using System.Collections.Generic;
using UnityEngine;

namespace Foundation
{

    public class MonoCustomDataProxy : MonoBehaviour
    {
        private object customData;
        private Dictionary<string, object> customDataDict;
   
        public void SetCustomData(object inCustomData)
        {
            customData = inCustomData;
        }

        public object GetCustomData()
        {
            return customData;
        }
    
        public bool HasCustomData(string key)
        {
            return customDataDict != null && customDataDict.ContainsKey(key);
        }

        public void SetCustomData(string key, object value)
        {
            if (customDataDict == null)
            {
                customDataDict = new Dictionary<string, object>();
            }

            customDataDict[key] = value;
        }
    
        public T GetCustomData<T>(string key)
        {
            if (customDataDict != null && customDataDict.ContainsKey(key))
                return (T) customDataDict[key];
            return default(T);
        }

        public T GetCustomData<T>() where T : class
        {
            return customData as T;
        }
    }
}