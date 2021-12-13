using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JB
{
    public class ClothItemHook : MonoBehaviour
    {
        public ClothItemType clothItemType;
        public SkinnedMeshRenderer meshRenderer;
        public Mesh defaultMesh;
        public Material defaultMaterial;
        
        public void Init()
        {
            ClothManager clothManager = GetComponentInParent<ClothManager>();
            clothManager.RegisterClothHook(this);
        }

        public void LoadClothItem(ClothItem clothItem)
        {
            meshRenderer.sharedMesh = clothItem.mesh;
            meshRenderer.material = clothItem.clothMaterial;
            meshRenderer.enabled = true;
        }

        public void UnloadItem()
        {
            if (clothItemType.isDisableWhenNoItem)
            {
                meshRenderer.enabled = false;
            }
            else
            {
                meshRenderer.sharedMesh = defaultMesh;
                meshRenderer.material = defaultMaterial;
            }
        }
    }
}
