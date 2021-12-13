using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public abstract class ItemAction : ScriptableObject
    {
        public abstract void ExecuteAction(ItemActionContainer itemActionContainer, Controller controller);
    }
}
