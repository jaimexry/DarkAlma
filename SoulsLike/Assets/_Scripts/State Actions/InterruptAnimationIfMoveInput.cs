using UnityEngine;

namespace JB
{
    public class InterruptAnimationIfMoveInput : StateAction
    {
        private PlayerStateManager states;

        public InterruptAnimationIfMoveInput(PlayerStateManager states)
        {
            this.states = states;
        }
        
        public override bool Execute()
        {
            if (states.canMove)
            {
                if (states.horizontal != 0 || states.vertical != 0)
                {
                    states.anim.Play("Empty");
                    states.ChangeState(states.locomotionId);
                    return true;
                }
            }

            return false;
        }
    }
}