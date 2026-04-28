using UnityEngine;

namespace Foundation
{
    
    public class SingletonScript<T> where T :  new()
    {
        private static T _instance;
         

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
                return _instance;
            }
        } 
    }
    
    public class Singleton<T> where T : SingletonComponent<T>, new()
    {
        private static T _instance;
        
        private static GameObject _gameObject;
        public static GameObject SingleObj => _gameObject;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();

                    string name =_instance.GetRootName();
                    _gameObject = new GameObject(name);
                    _gameObject.transform.parent = RootManager.MgrRoot.transform;
                    _instance.OnInit();
                }
                return _instance;
            }
        } 
    }
    
    public class SingletonComponent<T> : Singleton<T> where T : SingletonComponent<T>, new()
    {
        public virtual string GetRootName()
        {
            return typeof(T).Name;
        }
        public virtual void OnInit()
        {
        }
        public virtual void OnDestroy(){
        }
    } 
     
}