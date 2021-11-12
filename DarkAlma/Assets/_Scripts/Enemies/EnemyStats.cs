using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class EnemyStats : MonoBehaviour
    {
        public int healthLevel = 10;
        public int maxHealth;
        public int currentHealth;

        private Animator animatorHandler;

        private void Awake()
        {
            animatorHandler = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            maxHealth = SetMaxHealthFromHealthLevel();
            currentHealth = maxHealth;
        }

        private int SetMaxHealthFromHealthLevel()
        {
            maxHealth = healthLevel * 10;
            return maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth = currentHealth - damage;
            
            animatorHandler.Play("Damage_01");

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                animatorHandler.Play("Dead_01");
                //HANDLE PLAYER DEATH
            }
        }
    }
}
