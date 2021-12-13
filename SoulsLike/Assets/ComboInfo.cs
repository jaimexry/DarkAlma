using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class ComboInfo : StateMachineBehaviour
    {
        public Combo[] combos;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            Controller controller = animator.GetComponentInParent<Controller>();
            controller.LoadCombos(combos);
        }
    }
}
