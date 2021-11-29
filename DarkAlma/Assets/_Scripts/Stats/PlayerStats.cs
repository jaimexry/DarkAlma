using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class PlayerStats : CharacterStats
    {
        private PlayerManager playerManager;
        private HealthBar healthBar;
        private StaminaBar staminaBar;
        private FocusPointBar focusPointBar;

        private PlayerAnimatorManager _playerAnimatorManager;

        private void Awake()
        {
            playerManager = GetComponent<PlayerManager>();
            healthBar = FindObjectOfType<HealthBar>();
            staminaBar = FindObjectOfType<StaminaBar>();
            focusPointBar = FindObjectOfType<FocusPointBar>();
            _playerAnimatorManager = GetComponentInChildren<PlayerAnimatorManager>();
        }

        private void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
            healthBar.SetMaxHealth(maxHealth);

            maxStamina = SetMaxStaminaFromStaminaLevel();
            currentStamina = maxStamina;
            staminaBar.SetMaxStamina(maxStamina);

            maxFocusPoint = SetMaxFocusPointFromFocusPointLevel();
            currentFocusPoint = maxFocusPoint;
            focusPointBar.SetMaxFocusPoint(maxFocusPoint);
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }
        
        private float SetMaxStaminaFromStaminaLevel()
        {
            maxStamina = staminaLevel * 10;
            return maxStamina;
        }

        private float SetMaxFocusPointFromFocusPointLevel()
        {
            maxFocusPoint = focusPointLevel * 10;
            return maxFocusPoint;
        }
        
        public void TakeDamageNoAnimation(int damage)
        {
            currentHealth = currentHealth - damage;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;
            }
        }

        public void TakeDamage(int damage, string damageAnimation = "Damage_01")
        {
            if (playerManager.isInvulnerable)
            {
                return;
            }
            if (isDead)
            {
                return;
            }
            currentHealth = currentHealth - damage;
            healthBar.SetCurrentHealth(currentHealth);
            
            _playerAnimatorManager.PlayTargetAnimation(damageAnimation, true);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                _playerAnimatorManager.PlayTargetAnimation("Dead_01", true);
                isDead = true;
                //HANDLE PLAYER DEATH
            }
        }

        public void TakeStaminaDamage(int damage)
        {
            currentStamina = currentStamina - damage;
            staminaBar.SetCurrentStamina(currentStamina);

            if (currentStamina <= 0)
            {
                currentStamina = 0;
            }
        }

        public void RegenerateStamina()
        {
            if (!playerManager.isInteracting)
            {
                staminaRegenerationTimer += Time.deltaTime;
                if (currentStamina < maxStamina && staminaRegenerationTimer > 1f)
                {
                    currentStamina += staminaRegenerationAmount * Time.deltaTime;
                    if (currentStamina >= maxStamina)
                    {
                        currentStamina = maxStamina;
                    }
                    staminaBar.SetCurrentStamina(Mathf.RoundToInt(currentStamina));
                }
            }
            else
            {
                staminaRegenerationTimer = 0;
            }
        }

        public void HealPlayer(int amount)
        {
            currentHealth += amount;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
            
            healthBar.SetCurrentHealth(currentHealth);
        }

        public void DeductFocusPoint(int focusPoint)
        {
            currentFocusPoint -= focusPoint;

            if (currentFocusPoint < 0)
            {
                currentFocusPoint = 0;
            }
            focusPointBar.SetCurrentFocusPoint(currentFocusPoint);
        }

        public void AddSouls(int souls)
        {
            soulCount += souls;
        }
    }
}
