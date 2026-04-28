using System;
using Foundation.Storage;
using UnityEngine;

namespace Foundation
{
    public class ScheduleDriver : MonoBehaviour
    {        
        private ScheduleModule _schedule;

        internal void Bind(ScheduleModule schedule)
        {
            _schedule = schedule;
        }
        
        private void Update()
        {
            
            if (_schedule != null)
            {
                _schedule.InternalUpdate(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_schedule != null)
            {
                _schedule.InternalDriverDestroyed(this);
            }
        }

        private void OnApplicationQuit()
        {
            StorageManager.Instance.SaveDataAppExit();
        }
    }
}