using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    [CreateAssetMenu(menuName = "JB/Items/Cloth Item")]
    public class ClothItem : Item
    {
        public ClothItemType clothType;
        public Mesh mesh;
        public Material clothMaterial;
    }
}
