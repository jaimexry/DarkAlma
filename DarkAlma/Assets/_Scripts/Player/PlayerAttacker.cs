using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class PlayerAttacker : MonoBehaviour
    {
        private PlayerEquipmentManager playerEquipmentManager;
        private PlayerAnimatorManager _playerAnimatorManager;
        private PlayerManager playerManager;
        private PlayerStats playerStats;
        private PlayerInventory playerInventory;
        private InputHandler inputHandler;
        private WeaponSlotManager weaponSlotManager;
        public string lastAttack;
        private Vector3 handOverridePosition;
        private Transform handOverride;
        private Quaternion handOverrideRotation;

        private LayerMask backStabLayer = 1 << 13;
        private LayerMask riposteLayer = 1 << 14;

        private void Awake()
        {
            handOverride = GameObject.Find("Left Hand Override").transform;
            handOverridePosition = handOverride.localPosition;
            handOverrideRotation = handOverride.localRotation;
            _playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
            playerStats = GetComponentInParent<PlayerStats>();
            weaponSlotManager = GetComponent<WeaponSlotManager>();
            inputHandler = GetComponentInParent<InputHandler>();
            playerManager = GetComponentInParent<PlayerManager>();
            playerInventory = GetComponentInParent<PlayerInventory>();
        }

        public void HandleWeaponCombo(WeaponItem weapon)
        {
            if (playerStats.currentStamina <= 0)
            {
                return;
            }

            if (inputHandler.comboFlag)
            {
                _playerAnimatorManager.anim.SetBool("canDoCombo", false);
                if (lastAttack == weapon.OH_Light_Attack_1)
                {
                    _playerAnimatorManager.PlayTargetAnimation(weapon.OH_Light_Attack_2, true);
                }else if (lastAttack == weapon.TH_Attack_01)
                {
                    _playerAnimatorManager.PlayTargetAnimation(weapon.TH_Attack_02, true);
                }
            }
        }
        public void HandleLightAttack(WeaponItem weapon)
        {
            if (playerStats.currentStamina <= 0)
            {
                return;
            }

            weaponSlotManager.attackingWeapon = weapon;
            if (inputHandler.twoHandFlag)
            {
                _playerAnimatorManager.PlayTargetAnimation(weapon.TH_Attack_01, true);
                lastAttack = weapon.TH_Attack_01;
            }
            else
            {
                _playerAnimatorManager.PlayTargetAnimation(weapon.OH_Light_Attack_1, true);
                lastAttack = weapon.OH_Light_Attack_1;
            }
        }

        public void HandleHeavyAttack(WeaponItem weapon)
        {
            if (playerStats.currentStamina <= 0)
            {
                return;
            }

            weaponSlotManager.attackingWeapon = weapon;
            if (inputHandler.twoHandFlag)
            {
                _playerAnimatorManager.PlayTargetAnimation(weapon.TH_Attack_01, true);
                lastAttack = weapon.TH_Attack_01;
            }
            else
            {
                _playerAnimatorManager.PlayTargetAnimation(weapon.OH_Heavy_Attack_1, true);
                lastAttack = weapon.OH_Heavy_Attack_1;
            }
        }

        #region Input Actions

        public void HandleRBAction()
        {
            if (playerInventory.rightWeapon.isMeleeWeapon)
            {
                PerformRBMeleeAction();
            }else if (playerInventory.rightWeapon.isSpellCaster || playerInventory.rightWeapon.isPyroCaster || playerInventory.rightWeapon.isFaithCaster)
            {
                PerformRBMagicAction(playerInventory.rightWeapon);
            }
        }

        public void HandleLBAction()
        {
            PerformLBBlockingAction();
        }

        public void ReleaseLBAction()
        {
            handOverride.localPosition = handOverridePosition;
            handOverride.localRotation = handOverrideRotation;
        }

        public void HandleLTAction()
        {
            if (playerInventory.leftWeapon.isShieldWeapon)
            {
                PerformLTWeaponArt(inputHandler.twoHandFlag);
            }else if (playerInventory.leftWeapon.isMeleeWeapon)
            {
                
            }
        }

        #endregion

        #region Attack Actions

        private void PerformRBMeleeAction()
        {
            if (playerManager.canDoCombo)
            {
                inputHandler.comboFlag = true;
                HandleWeaponCombo(playerInventory.rightWeapon);
                inputHandler.comboFlag = false;
            }
            else
            {
                if (playerManager.isInteracting)
                {
                    return;
                }
                if (playerManager.canDoCombo)
                {
                    return;
                }
                _playerAnimatorManager.anim.SetBool("isUsingRightHand", true);
                HandleLightAttack(playerInventory.rightWeapon);
            }
        }

        private void PerformRBMagicAction(WeaponItem weapon)
        {
            if (playerManager.isInteracting)
            {
                return;
            }
            if (weapon.isFaithCaster)
            {
                if (playerInventory.currentSpell != null && playerInventory.currentSpell.isFaithSpell)
                {
                    if (playerStats.currentFocusPoint >= playerInventory.currentSpell.focusPointCost)
                    {
                        playerInventory.currentSpell.AttempToCastSpell(_playerAnimatorManager, playerStats);
                    }
                    else
                    {
                        _playerAnimatorManager.PlayTargetAnimation("Shrug", true);
                    }
                }
            }
        }

        private void PerformLTWeaponArt(bool isTwoHanding)
        {
            if (playerManager.isInteracting)
            {
                return;
            }

            if (isTwoHanding)
            {
                
            }
            else
            {
                _playerAnimatorManager.PlayTargetAnimation(playerInventory.leftWeapon.weapon_art, true);
            }
        }

        private void SuccesfullyCastSpell()
        {
            playerInventory.currentSpell.SuccesfullyCastSpell(_playerAnimatorManager, playerStats);
        }
        
        #endregion

        #region Defense Actions

        private void PerformLBBlockingAction()
        {
            if (playerManager.isInteracting)
            {
                return;
            }
            if (playerManager.isBlocking)
            {
                handOverride.localPosition = new Vector3(-0.144f, -0.078f, -0.013f);
                handOverride.localRotation = Quaternion.Euler(-59.736f, -65f, 67f);
                return;
            }

            _playerAnimatorManager.PlayTargetAnimation("Block Start", false, true);
            playerEquipmentManager.OpenBlockingCollider();
            playerManager.isBlocking = true;
        }

        #endregion
        
        public void AttemptBackStabOrRiposte()
        {
            if (playerStats.currentStamina <= 0)
            {
                return;
            }

            RaycastHit hit;

            if (Physics.Raycast(inputHandler.criticalAttackRayCastStartPoint.position,
                transform.TransformDirection(Vector3.forward), out hit, 0.5f, backStabLayer))
            {
                CharacterManager enemyCharacterManager =
                    hit.transform.gameObject.GetComponentInParent<CharacterManager>();
                DamageCollider rightWeapon = weaponSlotManager.rightHandDamageCollider;
                
                if (enemyCharacterManager != null)
                {
                    playerManager.transform.position = enemyCharacterManager.backStabCollider.criticalDamagerStandPosition.position;
                    Vector3 rotationDirection = playerManager.transform.root.eulerAngles;
                    rotationDirection = hit.transform.position - playerManager.transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation =
                        Quaternion.Slerp(playerManager.transform.rotation, tr, 500 * Time.deltaTime);
                    playerManager.transform.rotation = targetRotation;

                    int criticalDamage = playerInventory.rightWeapon.criticalDamageMultiplier *
                                         rightWeapon.currentWeaponDamage;
                    enemyCharacterManager.pendingCriticalDamage = criticalDamage;
                    
                    _playerAnimatorManager.PlayTargetAnimation("Back Stab", true);
                    enemyCharacterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Back Stabbed", true);
                }
            }else if (Physics.Raycast(inputHandler.criticalAttackRayCastStartPoint.position,
                transform.TransformDirection(Vector3.forward), out hit, 0.5f, riposteLayer))
            {
                CharacterManager enemyCharacterManager =
                    hit.transform.gameObject.GetComponentInParent<CharacterManager>();
                DamageCollider rightWeapon = weaponSlotManager.rightHandDamageCollider;
                
                if (enemyCharacterManager != null && enemyCharacterManager.canBeRiposted)
                {
                    playerManager.transform.position =
                        enemyCharacterManager.riposteCollider.criticalDamagerStandPosition.position;

                    Vector3 rotationDirection = playerManager.transform.root.eulerAngles;
                    rotationDirection = hit.transform.position - playerManager.transform.position;
                    rotationDirection.y = 0;
                    rotationDirection.Normalize();
                
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation =
                        Quaternion.Slerp(playerManager.transform.rotation, tr, 500 * Time.deltaTime);
                    playerManager.transform.rotation = targetRotation;
                
                    int criticalDamage = playerInventory.rightWeapon.criticalDamageMultiplier *
                                         rightWeapon.currentWeaponDamage;
                    enemyCharacterManager.pendingCriticalDamage = criticalDamage;
                    
                    _playerAnimatorManager.PlayTargetAnimation("Riposte", true);
                    enemyCharacterManager.GetComponentInChildren<AnimatorManager>().PlayTargetAnimation("Riposted", true);
                }
            }
        }
    }
}
