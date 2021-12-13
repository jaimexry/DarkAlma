using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class ClothManager : MonoBehaviour
    {
        private Dictionary<ClothItemType, ClothItemHook> clothHooks = new Dictionary<ClothItemType, ClothItemHook>();
        
        public void Init()
        {
            ClothItemHook[] clothItemHooks = GetComponentsInChildren<ClothItemHook>();
            foreach (ClothItemHook hook in clothItemHooks)
            {
                hook.Init();
            }
        }

        public void RegisterClothHook(ClothItemHook clothItemHook)
        {
            if (!clothHooks.ContainsKey(clothItemHook.clothItemType))
            {
                clothHooks.Add(clothItemHook.clothItemType, clothItemHook);
            }
        }

        public void LoadListOfItems(List<ClothItem> clothItems)
        {
            UnloadAllItems();

            for (int i = 0; i < clothItems.Count; i++)
            {
                LoadItem(clothItems[i]);
            }
        }
        
        public void UnloadAllItems()
        {
            foreach (ClothItemHook hook in clothHooks.Values)
            {
                hook.UnloadItem();
            }
        }

        private ClothItemHook GetClothHook(ClothItemType target)
        {
            clothHooks.TryGetValue(target, out ClothItemHook retVal);
            return retVal;
        }

        public void LoadItem(ClothItem clothItem)
        {
            ClothItemHook itemHook = null;
            
            if (clothItem == null)
            {
               return;
            }

            itemHook = GetClothHook(clothItem.clothType);
            itemHook.LoadClothItem(clothItem);
        }
    }
}
