using UnityEngine;

namespace JB
{
    [CreateAssetMenu(menuName = "JB/Item Actions/Attack Action")]
    public class AttackAction : ItemAction
    {
        public override void ExecuteAction(ItemActionContainer itemActionContainer, Controller controller)
        {
           // characterStateManager.AssignCurrentWeaponAndAction((WeaponItem)itemActionContainer.itemActual, itemActionContainer);
            controller.PlayTargetAnimation(itemActionContainer.animName, true, itemActionContainer.isMirrored);
        }
    }
}