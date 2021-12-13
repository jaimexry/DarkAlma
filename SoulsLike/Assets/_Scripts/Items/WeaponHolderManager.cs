using UnityEngine;

namespace JB
{
    public class WeaponHolderManager : MonoBehaviour
    {
        private WeaponHolderHook leftHook;
        public WeaponItem leftItem;
        private WeaponHolderHook rightHook;
        public WeaponItem rightItem;

        public void Init()
        {
            WeaponHolderHook[] weaponHolderHooks = GetComponentsInChildren<WeaponHolderHook>();

            foreach (WeaponHolderHook hook in weaponHolderHooks)
            {
                if (hook.isLeftHook)
                {
                    leftHook = hook;
                }
                else
                {
                    rightHook = hook;
                }
            }
        }

        public void LoadWeaponOnHook(WeaponItem weaponItem, bool isLeft)
        {
            if (isLeft)
            {
                leftHook.LoadWeaponModel(weaponItem);
                leftItem = weaponItem;
            }
            else
            {
                rightHook.LoadWeaponModel(weaponItem);
                rightItem = weaponItem;
            }
        }
    }
}