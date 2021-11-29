using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JB
{
    public class FocusPointBar : MonoBehaviour
    {
        private Slider slider;

        private void Awake()
        {
            slider = GetComponent<Slider>();
        }
        
        public void SetMaxFocusPoint(float maxFocusPoint)
        {
            slider.maxValue = maxFocusPoint;
            slider.value = maxFocusPoint;
        }

        public void SetCurrentFocusPoint(float currentFocusPoint)
        {
            slider.value = currentFocusPoint;
        }
    }
}
