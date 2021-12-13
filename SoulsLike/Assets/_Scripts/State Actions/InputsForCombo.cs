using UnityEngine;

namespace JB
{
    public class InputsForCombo : StateAction
    {
        private bool Rb;
        private bool Rt;
        private bool Lb;
        private bool Lt;
        private bool isAttacking;

        private PlayerStateManager states;

        public InputsForCombo(PlayerStateManager playerStateManager)
        {
            states = playerStateManager;
        }
        
        public override bool Execute()
        {
            states.horizontal = Input.GetAxis("Horizontal");
            states.vertical = Input.GetAxis("Vertical");
            
            if (!states.canDoCombo)
            {
                return false;
            }
            
            Rb = Input.GetButton("RB");
            Rt = Input.GetButton("RT");
            Lb = Input.GetButton("LB");
            Lt = Input.GetButton("LT");
            
            AttackInputs attackInput = AttackInputs.none;
            
            if (Rb || Rt || Lb || Lt)
            {
                isAttacking = true;

                if (Rb)
                {
                    attackInput = AttackInputs.rb;
                }
                if (Rt)
                {
                    attackInput = AttackInputs.rt;
                }
                if (Lb)
                {
                    attackInput = AttackInputs.lb;
                }
                if (Lt)
                {
                    attackInput = AttackInputs.lt;
                }
            }

            
            if (attackInput != AttackInputs.none)
            {
                states.DoCombo(attackInput);
                isAttacking = false;
            }
            
            return false;
        }
    }
}