using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace JB
{
    public class LockableHook : MonoBehaviour, ILockable
    {
        public Transform GetLockOnTarget(Transform from)
        {
            return transform;
        }
    }
}