using System;
using System.Collections.Generic;
using Foundation.Fundamental; 

namespace Foundation.FSM
{
    public sealed class FsmManager: IFsmManager
    {
        private readonly Dictionary<TypeNamePair, FsmBase> m_Fsms;
        private readonly List<FsmBase> m_TempFsms;

        public FsmManager()
        {
            m_Fsms = new Dictionary<TypeNamePair, FsmBase>();
            m_TempFsms = new List<FsmBase>();
        }

        internal  int Priority
        {
            get
            {
                return 1;
            }
        }

        public int Count
        {
            get
            {
                return m_Fsms.Count;
            }
        }

        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_TempFsms.Clear();
            if (m_Fsms.Count <= 0)
            {
                return;
            }

            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in m_Fsms)
            {
                m_TempFsms.Add(fsm.Value);
            }

            foreach (FsmBase fsm in m_TempFsms)
            {
                if (fsm.IsDestroyed)
                {
                    continue;
                }

                fsm.Update(elapseSeconds, realElapseSeconds);
            }
        }

        public bool HasFsm<T>() where T : class
        {
            return InternalHasFsm(new TypeNamePair(typeof(T)));
        }
        public bool HasFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new Exception("Owner type is invalid.");
            }

            return InternalHasFsm(new TypeNamePair(ownerType));
        }

        public bool HasFsm<T>(string name) where T : class
        {
            return InternalHasFsm(new TypeNamePair(typeof(T), name));
        }

        public bool HasFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new Exception("Owner type is invalid.");
            }

            return InternalHasFsm(new TypeNamePair(ownerType, name));
        }

        public IFsm<T> GetFsm<T>() where T : class
        {
            return (IFsm<T>)InternalGetFsm(new TypeNamePair(typeof(T)));
        }
        public FsmBase GetFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new Exception("Owner type is invalid.");
            }

            return InternalGetFsm(new TypeNamePair(ownerType));
        }

        public IFsm<T> GetFsm<T>(string name) where T : class
        {
            return (IFsm<T>)InternalGetFsm(new TypeNamePair(typeof(T), name));
        }

        public FsmBase GetFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new Exception("Owner type is invalid.");
            }

            return InternalGetFsm(new TypeNamePair(ownerType, name));
        }
        public FsmBase[] GetAllFsms()
        {
            int index = 0;
            FsmBase[] results = new FsmBase[m_Fsms.Count];
            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in m_Fsms)
            {
                results[index++] = fsm.Value;
            }

            return results;
        }

        public void GetAllFsms(List<FsmBase> results)
        {
            if (results == null)
            {
                throw new Exception("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in m_Fsms)
            {
                results.Add(fsm.Value);
            }
        }
        public IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }
        public IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
        {
            TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name))
            {
                throw new Exception(FUtility.Text.Format("Already exist FSM '{0}'.", typeNamePair));
            }

            Fsm<T> fsm = Fsm<T>.Create(name, owner, states);
            m_Fsms.Add(typeNamePair, fsm);
            return fsm;
        }
        public IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class
        {
            return CreateFsm(string.Empty, owner, states);
        }

        public IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
        {
            TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasFsm<T>(name))
            {
                throw new Exception(FUtility.Text.Format("Already exist FSM '{0}'.", typeNamePair));
            }

            Fsm<T> fsm = Fsm<T>.Create(name, owner, states);
            m_Fsms.Add(typeNamePair, fsm);
            return fsm;
        }
        public bool DestroyFsm<T>() where T : class
        {
            return InternalDestroyFsm(new TypeNamePair(typeof(T)));
        }
        public bool DestroyFsm(Type ownerType)
        {
            if (ownerType == null)
            {
                throw new Exception("Owner type is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(ownerType));
        }
        public bool DestroyFsm<T>(string name) where T : class
        {
            return InternalDestroyFsm(new TypeNamePair(typeof(T), name));
        }
        public bool DestroyFsm(Type ownerType, string name)
        {
            if (ownerType == null)
            {
                throw new Exception("Owner type is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(ownerType, name));
        }
        public bool DestroyFsm<T>(IFsm<T> fsm) where T : class
        {
            if (fsm == null)
            {
                throw new Exception("FSM is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(typeof(T), fsm.Name));
        }
        public bool DestroyFsm(FsmBase fsm)
        {
            if (fsm == null)
            {
                throw new Exception("FSM is invalid.");
            }

            return InternalDestroyFsm(new TypeNamePair(fsm.OwnerType, fsm.Name));
        }

        private bool InternalHasFsm(TypeNamePair typeNamePair)
        {
            return m_Fsms.ContainsKey(typeNamePair);
        }

        private FsmBase InternalGetFsm(TypeNamePair typeNamePair)
        {
            FsmBase fsm = null;
            if (m_Fsms.TryGetValue(typeNamePair, out fsm))
            {
                return fsm;
            }

            return null;
        }

        private bool InternalDestroyFsm(TypeNamePair typeNamePair)
        {
            FsmBase fsm = null;
            if (m_Fsms.TryGetValue(typeNamePair, out fsm))
            {
                fsm.Shutdown();
                return m_Fsms.Remove(typeNamePair);
            }

            return false;
        }
    }
}