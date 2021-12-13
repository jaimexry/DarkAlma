using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace JB
{
    public class Controller : MonoBehaviour
    {
        public bool lockOn;
        public bool isOnAir;
        public bool isGrounded;
        public bool canRotate;
        public bool canMove;
        public bool isRolling;
        public bool isInteracting;
        public bool canDoCombo;
        public AnimationCurve rollCurve;
        public AnimationClip rollClip;

        [Header("Controller")] 
        public float movementSpeed = 3;
        public float rollSpeed = 1;
        public float adaptSpeed = 1;
        public float rotationSpeed = 10;
        public float attackRotationSpeed = 3;
        public float groundRayForDistance = 0.2f;
        public float groundDownDistanceOnAir = 0.4f;
        public float navMeshDetectDistance = 1;
        public float frontRayOffset = 0.5f;
        public float delta;

        private Animator anim;
        new Rigidbody rigidbody;
        public NavMeshAgent agent;

        private Transform mTransform;

        private LayerMask ignoreForGroundCheck;

        public List<ClothItem> startingCloth;
        public ItemActionContainer[] currentActions;
        public ItemActionContainer[] defaultActions;
        private WeaponHolderManager weaponHolderManager;
        private ClothManager clothManager;

        public void SetWeapons(Item rightHand, Item leftHand)
        {
            weaponHolderManager.Init();
            
            LoadWeapon(rightHand, false);
            LoadWeapon(leftHand, true);
        }
        
        public void Init()
        {
            mTransform = this.transform;
            rigidbody = GetComponentInChildren<Rigidbody>();
            agent = GetComponentInChildren<NavMeshAgent>();
            agent.updateRotation = false;
            anim = GetComponentInChildren<Animator>();
            weaponHolderManager = GetComponent<WeaponHolderManager>();

            clothManager = GetComponent<ClothManager>();
            clothManager.Init();
            clothManager.LoadListOfItems(startingCloth);
            
            ResetCurrentActions();

            ignoreForGroundCheck = ~(1 << 9 | 1 << 10);
        }

        private void Update()
        {
            isInteracting = anim.GetBool("isInteracting");
            if (canDoCombo)
            {
                if (!isInteracting)
                {
                    canDoCombo = false;
                }
            }
        }

        #region Movement

        public void MoveCharacter(float vertical, float horizontal, Vector3 rotateDirection)
        {
            delta = Time.deltaTime;
            float moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

            if (!isInteracting || canRotate)
            {
                //HANDLE ROTATION
                if (lockOn)
                {
                    //TODO: change rotateDirection
                }
                HandleRotation(moveAmount, rotateDirection);
            }
           
            if (isInteracting)
            {
                return;
            }
         
            Vector3 targetVelocity = Vector3.zero;

            if (lockOn)
            {
                targetVelocity = movementSpeed * vertical * mTransform.forward;
                targetVelocity += movementSpeed * horizontal * mTransform.right;
            }
            else
            {
                targetVelocity = movementSpeed * moveAmount * mTransform.forward;
            }
            
            CheckGround(ref targetVelocity);
            
            //HANDLE MOVEMENT
            if (isGrounded)
            {
                /*CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.LateUpdate;
               
                if (!rigidbody.isKinematic)
                {
                    rigidbody.isKinematic = true;
                }
                if (!agent.isActiveAndEnabled)
                {
                    agent.enabled = true;
                }*/
                
                agent.velocity = targetVelocity;
            }
            else
            {
               /* CinemachineBrainHook.singleton.brain.m_UpdateMethod = Cinemachine.CinemachineBrain.UpdateMethod.FixedUpdate;

                if (rigidbody.isKinematic)
                {
                    rigidbody.isKinematic = false;
                }

                if (agent.isActiveAndEnabled)
                {
                    agent.enabled = false;
                }
                rigidbody.drag = 0;*/
                MoveWithPhysics(targetVelocity, moveAmount);
            }

            HandleAnimations(vertical, horizontal, moveAmount);
        }
        
        private void MoveWithPhysics(Vector3 targetVelocity, float moveAmount)
        {
            float frontY = 0;
            RaycastHit hit;
            
            Vector3 origin = mTransform.position + (targetVelocity.normalized * frontRayOffset);
            origin.y += 0.5f;
            Debug.DrawRay(origin, -Vector3.up, Color.red, 0.01f, false);
            if (Physics.Raycast(origin, -Vector3.up, out hit, 1, ignoreForGroundCheck))
            {
                float y = hit.point.y;
                frontY = y - mTransform.position.y;
            }

            Vector3 currentVelocity = rigidbody.velocity;

            if (isGrounded)
            {
                if (moveAmount > 0.1f)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.drag = 0;
                    if (Mathf.Abs(frontY) > 0.02f)
                    {
                        targetVelocity.y = ((frontY > 0) ? frontY + 0.2f : frontY - 0.2f) * movementSpeed;
                    }
                }
                else
                {
                    float abs = Mathf.Abs(frontY);

                    if (abs > 0.02f)
                    {
                        rigidbody.isKinematic = true;
                        targetVelocity.y = 0;
                        rigidbody.drag = 4;
                    }
                }
            }
            else
            {
                rigidbody.isKinematic = false;
                rigidbody.drag = 0;
                targetVelocity.y = currentVelocity.y;
            }

            Debug.DrawRay((mTransform.position + Vector3.up * 0.2f), targetVelocity, Color.green, 0.01f, false);
            rigidbody.velocity = targetVelocity;
        }
        
        private void CheckGround(ref Vector3 v)
        {
            RaycastHit hit;
            Vector3 origin = mTransform.position;
            origin.y += 0.5f;
            
            Debug.DrawRay(origin, mTransform.forward * 0.4f, Color.red);
            if (Physics.Raycast(origin, mTransform.forward, out hit, 0.4f))
            {
                v = Vector3.zero;
            }

            Vector3 dir = v;
            dir.Normalize();
            origin += dir * groundRayForDistance;

            float dis = 1;
            if (isOnAir)
            {
                dis = groundDownDistanceOnAir;
            }
            
            Debug.DrawRay(origin, Vector3.down * dis, Color.red);
            if (Physics.Raycast(origin,  Vector3.down, out hit, dis, ignoreForGroundCheck))
            {
                Vector3 tp = hit.point;
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(tp, out navMeshHit, navMeshDetectDistance, NavMesh.AllAreas))
                {
                    isGrounded = true;
                }
                else
                {
                    isGrounded = false;
                }

                if (isOnAir)
                {
                    isOnAir = false;
                    PlayTargetAnimation("Empty", false);

                }
            }
            else
            {
                if (isGrounded)
                {
                    isGrounded = false;
                }

                if (!isOnAir)
                {
                    isOnAir = true;
                    PlayTargetAnimation("OnAir", true);
                }
            }
        }
        
        private void HandleRotation(float moveAmount, Vector3 targetDir)
        {
            float moveOverride = moveAmount;
            if (lockOn)
            {
               moveOverride = 1;
            }

            targetDir.Normalize();
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
            {
                targetDir = mTransform.forward;
            }

            float actualRotationSpeed = rotationSpeed;
            if (isInteracting)
            {
                actualRotationSpeed = attackRotationSpeed;
            }
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(mTransform.rotation, tr, 
                delta * moveOverride * actualRotationSpeed);

            mTransform.rotation = targetRotation;
        }

        private void HandleAnimations(float vertical, float horizontal, float moveAmount)
        {
            if (isGrounded)
            {
                if (lockOn)
                {
                    float v = Mathf.Abs(vertical);
                    float f = 0;
                    if (v > 0 && v < 0.5f)
                    {
                        f = 0.5f;
                    }else if (v > 0.5f)
                    {
                        f = 1;
                    }

                    if (vertical < 0)
                    {
                        f = -f;
                    }

                    anim.SetFloat("forward", f); // 0.02f, delta);
                    
                    float h = Mathf.Abs(horizontal);
                    float s = 0;
                    if (h > 0 && h < 0.5f)
                    {
                        s = 0.5f;
                    }else if (h > 0.5f)
                    {
                        s = 1;
                    }

                    if (horizontal < 0)
                    {
                        s = -1;
                    }

                    anim.SetFloat("sideways", s); // 0.2f, delta);
                }
                else
                {
                    float m = moveAmount;
                    float f = 0;
                    if (m > 0 && m < 0.5f)
                    {
                        f = 0.5f;
                    }else if (m > 0.5f)
                    {
                        f = 1;
                    }
                
                    anim.SetFloat("forward", f, 0.02f, delta);
                    anim.SetFloat("sideways", 0); // 0.2f, delta);
                }
            }
            else
            {
                
            }
        }
        
        #endregion

        #region Items & Actions
        private void ResetCurrentActions()
        {
            currentActions = new ItemActionContainer[defaultActions.Length];
            for (int i = 0; i < defaultActions.Length; i++)
            {
                currentActions[i] = new ItemActionContainer();
                currentActions[i].animName = defaultActions[i].animName;
                currentActions[i].attackInput = defaultActions[i].attackInput;
                currentActions[i].isMirrored = defaultActions[i].isMirrored;
                currentActions[i].itemAction = defaultActions[i].itemAction;
                currentActions[i].itemActual = defaultActions[i].itemActual;
            }
        }
        public void PlayTargetAnimation(string targetAnim, bool isInteracting, bool isMirrored = false)
        {
            anim.SetBool("isMirror", isMirrored);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
            this.isInteracting = isInteracting;
        }
        
        public void PlayTargetItemAction(AttackInputs attackInputs)
        {
            canRotate = false;
            ItemActionContainer itemActionContainer = GetItemActionContainer(attackInputs, currentActions);
            
            if (!string.IsNullOrEmpty(itemActionContainer.animName))
            {
                itemActionContainer.ExecuteItemAction(this);
            }
        }
        
        private ItemActionContainer GetItemActionContainer(AttackInputs attackInputs, ItemActionContainer[] itemActionContainer)
        {
            if (itemActionContainer == null)
            {
                return null;
            }
            for (int i = 0; i < itemActionContainer.Length; i++)
            {
                if (itemActionContainer[i].attackInput == attackInputs)
                {
                    return itemActionContainer[i];
                }
            }

            return null;
        }

        public void LoadWeapon(Item item, bool isLeft)
        {
            if (!(item is WeaponItem))
            {
                return;
            }

            WeaponItem weaponItem = (WeaponItem) item;
            weaponHolderManager.LoadWeaponOnHook(weaponItem, isLeft);

            if (weaponItem == null)
            {
                ItemActionContainer da = GetItemActionContainer(GetAttackInput(AttackInputs.rb, isLeft), defaultActions);
                ItemActionContainer ta = GetItemActionContainer(GetAttackInput(AttackInputs.rt, isLeft), currentActions);
                CopyItemActionContainer(da, ta);
                ta.isMirrored = isLeft;
                return;
            }

            for (int i = 0; i < weaponItem.itemActions.Length; i++)
            {
                ItemActionContainer wa = weaponItem.itemActions[i];
                ItemActionContainer ic = GetItemActionContainer(GetAttackInput(wa.attackInput, isLeft), currentActions);
                
                ic.isMirrored = (isLeft);
                CopyItemActionContainer(wa, ic);
            }
        }

        private void CopyItemActionContainer(ItemActionContainer from, ItemActionContainer to)
        {
            to.animName = from.animName;
            to.itemAction = from.itemAction;
            to.itemActual = from.itemActual;
        }

        private AttackInputs GetAttackInput(AttackInputs inp, bool isLeft)
        {
            if (!isLeft)
            {
                return inp;
            }
            else
            {
                return inp switch
                {
                    AttackInputs.rb => AttackInputs.rb,
                    AttackInputs.lb => AttackInputs.lb,
                    AttackInputs.rt => AttackInputs.rt,
                    AttackInputs.lt => AttackInputs.lt,
                    _ => inp
                };
            }
        }


        #region Combos

        private Combo[] combos;

        public void LoadCombos(Combo[] targetCombo)
        {
            combos = targetCombo;
        }
        
        public void DoCombo(AttackInputs inputs)
        {
            Combo c = GetComboFromInput(inputs);
            if (c == null)
            {
                return;
            }
            PlayTargetAnimation(c.animName, true);
            canDoCombo = false;
        }
        
        private Combo GetComboFromInput(AttackInputs inputs)
        {
            if (combos == null)
            {
                return null;
            }
            
            for (int i = 0; i < combos.Length; i++)
            {
                if (combos[i].attackInputs == inputs)
                {
                    return combos[i];
                }
            }

            return null;
        }

        #endregion
        

        #endregion
    }
}