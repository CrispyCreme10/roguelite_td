using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [SerializeField] private Slider HealthSlider;
    [SerializeField] private HealthController HealthController;

    void OnEnable()
    {
        HealthController.OnHealthChange += OnHealthChange;
    }

    void OnDisable()
    {
        HealthController.OnHealthChange -= OnHealthChange;
    }

    void OnHealthChange(int activeHealth, float healthPerc)
    {
        HealthSlider.value = healthPerc;
    }
}
