using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRangeController : MonoBehaviour
{
    [SerializeField] private DamageController damageController;
    [SerializeField] private TowerController towerController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out DamageController dController) &&
            dController.CanBeAttackedByTag(damageController.DamageTag))
        {
            towerController.AddTarget(dController);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out DamageController dController) &&
            towerController.Targets.Contains(dController))
        {
            towerController.RemoveTarget(dController);
        }
    }
}
