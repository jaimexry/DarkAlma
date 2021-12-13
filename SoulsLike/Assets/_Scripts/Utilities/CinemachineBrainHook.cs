using System;
using UnityEngine;

namespace JB
{
    public class CinemachineBrainHook : MonoBehaviour
    {
        public Cinemachine.CinemachineBrain brain;

        public static CinemachineBrainHook singleton;
        
        private void Awake()
        {
            singleton = this;
            brain = GetComponent<Cinemachine.CinemachineBrain>();
        }
    }
}