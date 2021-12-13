using System;
using UnityEngine;

namespace JB
{
    [Serializable]
    public class ItemActionContainer
    {
        public string animName;
        public ItemAction itemAction;
        public AttackInputs attackInput;
        public bool isMirrored;
        public bool isTwoHanded;
        public Item itemActual;
       
        public void ExecuteItemAction(Controller controller)
        {
            itemAction.ExecuteAction(this, controller);
        }
    }
}