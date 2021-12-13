using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class AnimatorHook : MonoBehaviour
    {
        private Controller controller;
        private Animator animator;

        private void Start()
        {
            animator = GetComponent<Animator>();
            controller = GetComponentInParent<Controller>();
        }

        public void OnAnimatorMove()
        {
            OnAnimatorMoveOverride();
        }

        protected virtual void OnAnimatorMoveOverride()
        {
            if (controller == null)
            {
                return;
            }
            if (!controller.isInteracting)
            {
                return;
            }
            
            if (controller.isGrounded && Time.deltaTime > 0)
            {
                Vector3 v = (animator.deltaPosition) / controller.delta;
                //v.y = states.rigidbody.velocity.y;
                //states.rigidbody.velocity = v;
                controller.agent.velocity = v;
            }
        }

        public void OpenCanMove()
        {
            controller.canMove = true;
        }

        public void OpenDamageCollider()
        {
            //controller.HandleDamageCollider(true);
        }

        public void CloseDamageCollider()
        {
           // controller.HandleDamageCollider(false);
        }

        public void EnableCombo()
        {
            controller.canDoCombo = true;
        }

        public void EnableRotation()
        {
            controller.canRotate = true;
        }
        
        public void DisableRotation()
        {
            controller.canRotate = false;
        }
    }
}
