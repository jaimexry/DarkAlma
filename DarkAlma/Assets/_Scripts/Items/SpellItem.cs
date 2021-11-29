using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class SpellItem : Item
    {
        public GameObject spellWarmUpFX;
        public GameObject spellCastFX;
        public string spellAnimation;

        [Header("Spell Cost")]
        public int focusPointCost;

        [Header("Spell Type")] 
        public bool isFaithSpell;
        public bool isMagicSpell;
        public bool isPyroSpell;

        [Header("Spell Description")] [TextArea]
        public string spellDescription;

        public virtual void AttempToCastSpell(PlayerAnimatorManager playerAnimatorManager, PlayerStats playerStats)
        {
            Debug.Log("You attempt to cast a spell!");
        }

        public virtual void SuccesfullyCastSpell(PlayerAnimatorManager playerAnimatorManager, PlayerStats playerStats)
        {
            Debug.Log("You succesfully casted a spell!");
            playerStats.DeductFocusPoint(focusPointCost);
        }
    }
}
