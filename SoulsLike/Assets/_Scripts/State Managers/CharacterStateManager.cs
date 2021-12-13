using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace JB
{
    public abstract class CharacterStateManager : StateManager, ILockable
    {
        [Header("References")]
        public Animator anim;
        public new Rigidbody rigidbody;
        public AnimatorHook animHook;
        public ClothManager clothManager;
        public WeaponHolderManager weaponHolderManager;
        public NavMeshAgent agent;
        
        [Header("States")] 
        public bool isGrounded;
        public bool isOnAir;
        public bool isInteracting;
        public bool useRootMotion;
        public bool lockOn;
        public bool isTwoHanded;
        public bool canDoCombo;
        public bool canRotate;
        public bool canMove;
        public float groundRayForDistance = 0.2f;
        public float groundDownDistanceOnAir = 0.4f;
        public Transform target;

        [Header("Controller Values")] 
        public float vertical;
        public float horizontal;
        public float delta;
        public Vector3 rootMovement;

        [Header("Item Actions")] 
        protected ItemActionContainer[] itemActions;
        public ItemActionContainer[] defaultItemActions = new ItemActionContainer[4];

        [Header("Runtime References")]
        public List<ClothItem> startingsCloths;
        public WeaponItem rightWeapon;
        public WeaponItem leftWeapon;

        protected WeaponItem currentWeaponInUse;
        protected ItemActionContainer currentItemAction;

        public Combo[] currentCombo;

        public override void Init()
        {
            anim = GetComponentInChildren<Animator>();
            animHook = GetComponentInChildren<AnimatorHook>();
            rigidbody = GetComponentInChildren<Rigidbody>();
            clothManager = GetComponentInChildren<ClothManager>();
            weaponHolderManager = GetComponentInChildren<WeaponHolderManager>();
            agent = GetComponentInChildren<NavMeshAgent>();
            agent.updateRotation = false;
            anim.applyRootMotion = false;
            
            itemActions = new ItemActionContainer[4];
            PopulateListWithDefaultItemActions();
        }

        private void PopulateListWithDefaultItemActions()
        {
            for (int i = 0; i < defaultItemActions.Length; i++)
            {
                itemActions[i] = defaultItemActions[i];
            }
        }

        private ItemActionContainer GetItemActionContainer(AttackInputs attackInputs,
            ItemActionContainer[] itemActionContainers)
        {
            for (int i = 0; i < itemActionContainers.Length; i++)
            {
                if (itemActionContainers[i].attackInput == attackInputs)
                {
                    return itemActionContainers[i];
                }
            }

            return null;
        }
        
        protected ItemActionContainer GetItemActionContainer(AttackInputs attackInputs,
            ItemActionContainer[] itemActionContainers, bool isTwoHanded)
        {
            for (int i = 0; i < itemActionContainers.Length; i++)
            {
                if (itemActionContainers[i].attackInput == attackInputs)
                {
                    if (isTwoHanded)
                    {
                        if (itemActionContainers[i].isTwoHanded)
                        {
                            return itemActionContainers[i];
                        }
                    }
                    else
                    {
                        if (!itemActionContainers[i].isTwoHanded)
                        {
                            return itemActionContainers[i];
                        }
                    }
                }
            }
            return null;
        }

        public void PlayTargetAnimation(string targetAnim, bool isInteracting, bool isMirrored = false)
        {
            anim.SetBool("isMirror", isMirrored);
            anim.SetBool("isInteracting", isInteracting);
            anim.CrossFade(targetAnim, 0.2f);
        }

        public virtual bool PlayTargetItemAction(AttackInputs attackInputs)
        {
            return true;
        }

        public virtual void OnAssignLookOverride(Transform target)
        {
            this.target = target;
            if (target != null)
            {
                lockOn = true;
            }
        }

        public virtual void OnClearLookOverride()
        {
            lockOn = false;
        }

        public virtual void UpdateItemActionWithCurrent()
        {
            ItemActionContainer[] itemActionContainers = new ItemActionContainer[4];

            for (int i = 0; i < itemActionContainers.Length; i++)
            {
                itemActionContainers[i] = new ItemActionContainer();
                itemActionContainers[i].animName = defaultItemActions[i].animName;
                itemActionContainers[i].attackInput = defaultItemActions[i].attackInput;
                itemActionContainers[i].itemAction = defaultItemActions[i].itemAction;
                itemActionContainers[i].isMirrored = defaultItemActions[i].isMirrored;
                itemActionContainers[i].itemActual = defaultItemActions[i].itemActual;
            }

            bool canLoadLeftHandActions = !isTwoHanded;

            if (weaponHolderManager.rightItem != null)
            {
                if (isTwoHanded)
                {
                    anim.CrossFade(weaponHolderManager.rightItem.twoHandedAnim, 0.2f);
                    anim.SetBool("isMirror", false);
                }
                else
                {
                    anim.CrossFade("R_" + weaponHolderManager.rightItem.oneHandedAnim, 0.2f);
                }
                
                for (int i = 0; i < weaponHolderManager.rightItem.itemActions.Length; i++)
                {
                    if (isTwoHanded)
                    {
                        if (!weaponHolderManager.rightItem.itemActions[i].isTwoHanded)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (weaponHolderManager.rightItem.itemActions[i].isTwoHanded)
                        {
                            continue;
                        }
                    }
                    ItemActionContainer iac = GetItemActionContainer(
                        weaponHolderManager.rightItem.itemActions[i].attackInput,
                        itemActionContainers);
                    
                    iac.animName = weaponHolderManager.rightItem.itemActions[i].animName;
                    iac.itemAction = weaponHolderManager.rightItem.itemActions[i].itemAction;
                    iac.isTwoHanded = weaponHolderManager.rightItem.itemActions[i].isTwoHanded;
                    iac.itemActual = weaponHolderManager.rightItem;
                }
            }
            else
            {
                anim.CrossFade("R_Empty", 0.2f);
                canLoadLeftHandActions = true;
            }

            if (!canLoadLeftHandActions)
            {
                itemActions = itemActionContainers;
                return;
            }
            
            if (weaponHolderManager.leftItem != null)
            {
                if (isTwoHanded)
                {
                    anim.CrossFade(weaponHolderManager.leftItem.twoHandedAnim, 0.2f);
                    anim.SetBool("isMirror", true);
                }
                else
                {               
                    anim.CrossFade("L_" + weaponHolderManager.leftItem.oneHandedAnim, 0.2f);
                }

                for (int i = 0; i < weaponHolderManager.leftItem.itemActions.Length; i++)
                {
                    if (isTwoHanded)
                    {
                        if (!weaponHolderManager.leftItem.itemActions[i].isTwoHanded)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (weaponHolderManager.leftItem.itemActions[i].isTwoHanded)
                        {
                            continue;
                        }
                    }
                    ItemActionContainer weaponAction = weaponHolderManager.leftItem.itemActions[i];

                    if (isTwoHanded)
                    {
                        ItemActionContainer iac = GetItemActionContainer(weaponHolderManager.leftItem.itemActions[i].attackInput, itemActionContainers);
                    
                        iac.animName = weaponAction.animName;
                        iac.itemAction = weaponAction.itemAction;
                        iac.isTwoHanded = weaponHolderManager.leftItem.itemActions[i].isTwoHanded;
                        iac.itemActual = weaponHolderManager.leftItem;
                    }
                    else
                    {
                        AttackInputs ai = AttackInputs.lb;
                        if (weaponAction.attackInput == AttackInputs.rb)
                        {
                            ai = AttackInputs.lb;
                        }
                        if (weaponAction.attackInput == AttackInputs.rt)
                        {
                            ai = AttackInputs.lt;
                        }
                        
                        ItemActionContainer iac = GetItemActionContainer(ai, itemActionContainers);
                    
                        iac.animName = weaponAction.animName;
                        iac.itemAction = weaponAction.itemAction;
                        iac.isTwoHanded = weaponHolderManager.leftItem.itemActions[i].isTwoHanded;
                        iac.itemActual = weaponHolderManager.leftItem;
                    }
                }
            }
            else
            {
                anim.CrossFade("L_Empty", 0.2f);
            }

            itemActions = itemActionContainers;
        }

        public void HandleTwoHanded()
        {
            if (isTwoHanded)
            {
                isTwoHanded = false;
                anim.Play("B_Empty");
            }
            else
            {
                isTwoHanded = true;
            }
            UpdateItemActionWithCurrent();
        }

        public void AssignCurrentWeaponAndAction(WeaponItem weaponItem, ItemActionContainer iac)
        {
            currentWeaponInUse = weaponItem;
            currentItemAction = iac;
        }

        public void HandleDamageCollider(bool status)
        {
            if (currentWeaponInUse == null)
            {
                return;
            }
            currentWeaponInUse.weaponHook.DamageColliderStatus(status);
        }

        public virtual void DoCombo(AttackInputs inputs)
        {
            
        }

        public void DisableCombo()
        {
            canDoCombo = false;
        }

        public Transform FindLockableTarget()
        {
            List<Transform> lockableList = new List<Transform>();
            
            LayerMask lm = 1 << 9;
            Collider[] colliders = Physics.OverlapSphere(mTransform.position, 20, lm);
            foreach (Collider collider in colliders)
            {
                ILockable iLock = collider.GetComponentInChildren<ILockable>();
                if (iLock != null)
                {
                    Transform t = iLock.GetLockOnTarget(mTransform);

                    if (t != null)
                    {
                        lockableList.Add(t);
                    }
                }
            }

            float minDis = float.MaxValue;
            Transform target = null;

            for (int i = 0; i < lockableList.Count; i++)
            {
                float tempDis = Vector3.Distance(lockableList[i].position, mTransform.position);
                if (tempDis < minDis)
                {
                    minDis = tempDis;
                    target = lockableList[i];
                }
            }
            return target;
        }

        public Transform GetLockOnTarget(Transform from)
        {
            if (from == this.transform)
            {
                return null;
            }

            return mTransform;
        }
    }

    [Serializable]
    public class Combo
    {
        public string animName;
        public AttackInputs attackInputs;
    }
}
