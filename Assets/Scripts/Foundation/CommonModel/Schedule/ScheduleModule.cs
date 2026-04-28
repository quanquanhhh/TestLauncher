using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Foundation
{
    public class ScheduleModule: SingletonComponent<ScheduleModule>
    {
        private ScheduleDriver _scheduleDriver;
        
        private static UnityAction updateAction;
        private static UnityAction secondUpdateAction;
        
        
        private float _timer;
        public override string GetRootName()
        {
            return "ScheduleDriver";
        }

        public override void OnInit()
        {
            base.OnInit();

            SingleObj.AddComponent<ScheduleProcess>();
            _scheduleDriver = SingleObj.AddComponent<ScheduleDriver>();
            _scheduleDriver.Bind(this);
        }

        public Coroutine StartCoroutine(IEnumerator action)
        {
            if (action ==  null)
            {
                return null;
            }

            return _scheduleDriver.StartCoroutine(action);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            updateAction = null;
            secondUpdateAction = null;
            _timer = 0f;
            if (_scheduleDriver != null)
            {
                GameObject.Destroy(_scheduleDriver.gameObject);
                _scheduleDriver = null;
            }
        }

        public void RegisterUpdate(UnityAction handler)
        {
            updateAction += handler; 
        }
        public void UnregisterUpdate(UnityAction handler)
        {
            updateAction -= handler;
            secondUpdateAction -= handler;
        }
        public void RegisterUpdatePerSecond(UnityAction handler)
        {
            secondUpdateAction += handler;
        }

        public void UnregisterUpdatePerSecond(UnityAction handler)
        {
            secondUpdateAction -= handler;
        }
        internal void InternalUpdate(float deltaTime)
        {
          
            if (updateAction != null)
            {
                updateAction.Invoke();
            }
            _timer += deltaTime;
            if (_timer >= 1f)
            {
                _timer = 0;
                if (secondUpdateAction != null)
                {
                    secondUpdateAction.Invoke();
                } 
            }
        }
        internal void InternalDriverDestroyed(ScheduleDriver driver)
        {
            if (driver == _scheduleDriver)
            {
                _scheduleDriver = null; //加个保护
            }
        }
        
    }
}