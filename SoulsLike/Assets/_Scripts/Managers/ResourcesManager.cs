using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace JB
{
    [CreateAssetMenu(menuName = "JB/Resources Manager")]
    public class ResourcesManager : ScriptableObject
    {
        public Item[] allItems;
        [System.NonSerialized] private Dictionary<string, Item> itemsDict = new Dictionary<string, Item>();

        public void Init()
        {
            for (int i = 0; i < allItems.Length; i++)
            {
                itemsDict.Add(allItems[i].name, allItems[i]);
            }
        }

        public Item GetItem(string id)
        {
            Item retVal = null;
            itemsDict.TryGetValue(id, out retVal);
            return retVal;
        }
    }
}