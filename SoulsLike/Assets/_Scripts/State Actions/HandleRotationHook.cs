using UnityEngine;

namespace JB
{
    public class HandleRotationHook : StateAction
    {
        private PlayerStateManager states;
        private MovePlayerCharacter move;
        public HandleRotationHook(PlayerStateManager states, MovePlayerCharacter move)
        {
            this.states = states;
            this.move = move;
        }
        
        public override bool Execute()
        {

            if (states.canRotate)
            {
                move.HandleRotation();
            }

            return false;
        }
    }
}