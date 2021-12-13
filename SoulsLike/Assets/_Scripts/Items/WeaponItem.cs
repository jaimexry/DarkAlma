using System;
using UnityEngine;
using System.Collections;

namespace JB
{
    [CreateAssetMenu(menuName = "JB/Items/Weapon Item")]
    public class WeaponItem : Item
    {
        public GameObject modelPrefab;
        public string oneHandedAnim = "One Handed";
        public string twoHandedAnim = "Two Handed";
        public ItemActionContainer[] itemActions;

        [NonSerialized] 
        public WeaponHook weaponHook;
    }
}