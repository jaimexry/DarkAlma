using UnityEngine;

namespace JB
{
    [CreateAssetMenu]
    public class PlayerProfile : ScriptableObject
    {
        public string[] startingClothes;
        public string rightHandWeapon;
        public string leftHandWeapon;
    }
}