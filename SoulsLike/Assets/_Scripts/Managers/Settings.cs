using UnityEngine;

namespace JB
{
    public static class Settings
    {
        private static ResourcesManager _resourcesManager;

        public static ResourcesManager ResourcesManager
        {
            get
            {
                if (_resourcesManager == null)
                {
                    _resourcesManager = Resources.Load("ResourcesManager") as ResourcesManager;
                    _resourcesManager.Init();
                }

                return _resourcesManager;
            }
        }
    }
}