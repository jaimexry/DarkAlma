using System.Collections;
using System.Collections.Generic;
using JB;
using UnityEngine;

namespace JB
{
    [CreateAssetMenu(menuName = "Spells/Healing Spell")]
    public class HealingSpell : SpellItem
    {
        public int healAmount;

        public override void AttempToCastSpell(PlayerAnimatorManager playerAnimatorManager, PlayerStats playerStats)
        {
            base.AttempToCastSpell(playerAnimatorManager, playerStats);
            GameObject instantiatedWarmUpSpellFX = Instantiate(spellWarmUpFX, playerAnimatorManager.transform);
            playerAnimatorManager.PlayTargetAnimation(spellAnimation, true);
            Debug.Log("Attempting to cast spell...");
        }

        public override void SuccesfullyCastSpell(PlayerAnimatorManager playerAnimatorManager, PlayerStats playerStats)
        {
            base.SuccesfullyCastSpell(playerAnimatorManager, playerStats);
            GameObject instantiatedSpellFX = Instantiate(spellCastFX, playerAnimatorManager.transform);
            playerStats.HealPlayer(healAmount);
            Debug.Log("Spell cast succesfull");
        }
    }
}
