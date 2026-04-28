using UnityEngine;

namespace Foundation
{
    public class RootManager
    {
        public static GameObject MgrRoot;
        public static Transform UIRoot;
        
        public static Camera UICamera => Camera.main;

    }
}