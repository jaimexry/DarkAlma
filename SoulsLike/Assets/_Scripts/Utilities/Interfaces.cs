using UnityEngine;

namespace JB
{
    public interface ILockable
    {
        Transform GetLockOnTarget(Transform from);
    }
}