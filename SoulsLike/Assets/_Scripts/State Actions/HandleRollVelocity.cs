using UnityEngine;

namespace JB
{
    public class HandleRollVelocity : StateAction
    {
        private PlayerStateManager states;

        private bool isInit;
        private float t;
        private float speed;
        
        public HandleRollVelocity(PlayerStateManager states)
        {
            this.states = states;
            speed = 1 / states.rollClip.length;
        }
        
        public override bool Execute()
        {
            if (!states.isRolling)
            {
                isInit = false;
            }
            if (!isInit)
            {
                t = 0;
                states.isRolling = true;
                isInit = true;
            }
            
            float frontY = 0;
            RaycastHit hit;
            Vector3 targetVelocity = states.rollDirection;

            Vector3 origin = states.mTransform.position + (targetVelocity.normalized * states.frontRayOffset);
            origin.y += 0.5f;
            Debug.DrawRay(origin, -Vector3.up, Color.red, 0.01f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, 1, states.ignoreForGroundCheck))
            {
                float y = hit.point.y;
                frontY = y - states.mTransform.position.y;
            }
            states.rigidbody.isKinematic = false;
            states.rigidbody.drag = 0;

            Vector3 currentVelocity = states.rigidbody.velocity;

            if (states.isGrounded)
            {
                if (Mathf.Abs(frontY) > 0.02f)
                {
                    targetVelocity.y = ((frontY > 0) ? frontY + 0.2f : frontY - 0.2f) * states.rollSpeed;
                }
            }
            else
            {
                targetVelocity.y = currentVelocity.y;
            }

            if (states.isRolling)
            {
                t += speed * states.delta;
                if (t > 1)
                {
                    t = 1;
                    states.isRolling = false;
                }
            }
            
            float e = states.rollCurve.Evaluate(t);
            
            states.rigidbody.velocity = targetVelocity * states.rollSpeed * e;
            
            return false;
        }
    }
}