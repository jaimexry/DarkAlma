using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace JB
{
    public class MovePlayerCharacter : StateAction
    {
        private PlayerStateManager states;


        public MovePlayerCharacter(PlayerStateManager playerStateManager)
        {
            states = playerStateManager;
        }
        
        public override bool Execute()
        {
            Vector3 targetVelocity = Vector3.zero;
            
            if (states.lockOn)
            {
                targetVelocity = states.movementSpeed * states.vertical * states.mTransform.forward;
                targetVelocity += states.movementSpeed * states.horizontal * states.mTransform.right;
            }
            else
            {
                targetVelocity = states.movementSpeed * states.moveAmount * states.mTransform.forward;
            }
            
            CheckGround(ref targetVelocity);

            if (states.isGrounded)
            {
                CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.LateUpdate;
                
                if (!states.rigidbody.isKinematic)
                {
                    states.rigidbody.isKinematic = true;
                }
                if (!states.agent.isActiveAndEnabled)
                {
                    states.agent.enabled = true;
                }
                HandleRotation();
                HandleAnimations();
                states.agent.velocity = targetVelocity;
            }
            else
            {
                CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.FixedUpdate;

                if (states.rigidbody.isKinematic)
                {
                    states.rigidbody.isKinematic = false;
                }

                if (states.agent.isActiveAndEnabled)
                {
                    states.agent.enabled = false;
                }
                states.rigidbody.drag = 0;
                targetVelocity.y = states.rigidbody.velocity.y;
                states.rigidbody.velocity = targetVelocity;
            }
            
            return false;
        }

        private void CheckGround(ref Vector3 v)
        {
            RaycastHit hit;
            Vector3 origin = states.mTransform.position;
            origin.y += 0.5f;
            
            Debug.DrawRay(origin, states.mTransform.forward * 0.4f, Color.red);
            if (Physics.Raycast(origin, states.mTransform.forward, out hit, 0.4f))
            {
                v = Vector3.zero;
            }

            Vector3 dir = v;
            dir.Normalize();
            origin += dir * states.groundRayForDistance;

            float dis = 1;
            if (states.isOnAir)
            {
                dis = states.groundDownDistanceOnAir;
            }
            
            Debug.DrawRay(origin, Vector3.down * dis, Color.red);
            if (Physics.Raycast(origin,  Vector3.down, out hit, dis, states.ignoreForGroundCheck))
            {
                Vector3 tp = hit.point;
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(tp, out navMeshHit, states.navMeshDetectDistance, NavMesh.AllAreas))
                {
                    states.isGrounded = true;
                }
                else
                {
                    states.isGrounded = false;
                }

                if (states.isOnAir)
                {
                    states.isOnAir = false;
                    states.PlayTargetAnimation("Empty", false);

                }
            }
            else
            {
                if (states.isGrounded)
                {
                    states.isGrounded = false;
                }

                if (!states.isOnAir)
                {
                    states.isOnAir = true;
                    states.PlayTargetAnimation("OnAir", true);
                }
            }
        }

        private void MoveWithPhysics()
        {
            float frontY = 0;
            RaycastHit hit;
            Vector3 targetVelocity = Vector3.zero;

            if (states.lockOn)
            {
                targetVelocity = states.movementSpeed * states.vertical * states.mTransform.forward;
                targetVelocity += states.movementSpeed * states.horizontal * states.mTransform.right;
            }
            else
            {
                targetVelocity = states.movementSpeed * states.moveAmount * states.mTransform.forward;
            }
            
            Vector3 origin = states.mTransform.position + (targetVelocity.normalized * states.frontRayOffset);
            origin.y += 0.5f;
            Debug.DrawRay(origin, -Vector3.up, Color.red, 0.01f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, 1, states.ignoreForGroundCheck))
            {
                float y = hit.point.y;
                frontY = y - states.mTransform.position.y;
            }

            Vector3 currentVelocity = states.rigidbody.velocity;

            
            
            /*if (states.isLockingOn)
            {
                targetVelocity = states.rotateDirection * states.moveAmount * movementSpeed;
            }*/

            if (states.isGrounded)
            {
                float moveAmount = states.moveAmount;

                if (moveAmount > 0.1f)
                {
                    states.rigidbody.isKinematic = false;
                    states.rigidbody.drag = 0;
                    if (Mathf.Abs(frontY) > 0.02f)
                    {
                        targetVelocity.y = ((frontY > 0) ? frontY + 0.2f : frontY - 0.2f) * states.movementSpeed;
                    }
                }
                else
                {
                    float abs = Mathf.Abs(frontY);

                    if (abs > 0.02f)
                    {
                        states.rigidbody.isKinematic = true;
                        targetVelocity.y = 0;
                        states.rigidbody.drag = 4;
                    }
                }
                
                HandleRotation();
            }
            else
            {
                //states.collider.height = colStartHeight;
                states.rigidbody.isKinematic = false;
                states.rigidbody.drag = 0;
                targetVelocity.y = currentVelocity.y;
            }
            
            HandleAnimations();
            
            Debug.DrawRay((states.mTransform.position + Vector3.up * 0.2f), targetVelocity, Color.green, 0.01f, false);
            states.rigidbody.velocity = targetVelocity;
            
            //states.rigidbody.velocity = Vector3.Lerp(currentVelocity, targetVelocity, states.delta * states.adaptSpeed);
            
        }
        
        public void HandleRotation()
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = states.moveAmount;
            if (states.lockOn)
            {
               targetDir = states.target.position - states.mTransform.position;
               moveOverride = 1;
            }
            else
            {
                float h = states.horizontal;
                float v = states.vertical;
                
                targetDir = states.camera.transform.forward * v;
                targetDir += states.camera.transform.right * h;
            }
            
            targetDir.Normalize();
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
            {
                targetDir = states.mTransform.forward;
            }

            float rotationSpeed = states.rotationSpeed;
            if (states.isInteracting)
            {
                rotationSpeed = states.attackRotationSpeed;
            }
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(states.mTransform.rotation, tr, 
                states.delta * moveOverride * rotationSpeed);

            states.mTransform.rotation = targetRotation;
        }

        private void HandleAnimations()
        {
            if (states.isGrounded)
            {
                if (states.lockOn)
                {
                    float v = Mathf.Abs(states.vertical);
                    float f = 0;
                    if (v > 0 && v < 0.5f)
                    {
                        f = 0.5f;
                    }else if (v > 0.5f)
                    {
                        f = 1;
                    }

                    if (states.vertical < 0)
                    {
                        f = -f;
                    }

                    states.anim.SetFloat("forward", f); // 0.02f, states.delta);
                    
                    float h = Mathf.Abs(states.horizontal);
                    float s = 0;
                    if (h > 0 && h < 0.5f)
                    {
                        s = 0.5f;
                    }else if (h > 0.5f)
                    {
                        s = 1;
                    }

                    if (states.horizontal < 0)
                    {
                        s = -1;
                    }

                    states.anim.SetFloat("sideways", s); // 0.2f, states.delta);
                }
                else
                {
                    float m = states.moveAmount;
                    float f = 0;
                    if (m > 0 && m < 0.5f)
                    {
                        f = 0.5f;
                    }else if (m > 0.5f)
                    {
                        f = 1;
                    }
                
                    states.anim.SetFloat("forward", f, 0.02f, states.delta);
                    states.anim.SetFloat("sideways", 0); // 0.2f, states.delta);
                }
            }
            else
            {
                
            }
        }
    }
}
