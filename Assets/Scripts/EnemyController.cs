using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] GameObject _targetSurvivor;
    [SerializeField] GameObject _crystal;
    [SerializeField] private bool TargetCrystal = true;
    [SerializeField] private float ChanceToTargetPlayer = 0f;

    public GameObject TargetSurvivor => _targetSurvivor;
    public GameObject Crystal => _crystal;
    public bool IsTargetCrystal => TargetCrystal;
    public bool IsTargetSurvivor => _isTargetSurvivor;

    bool _isTargetSurvivor;

    void Start()
    {
        var rnd = Random.Range(0f, 1f);
        if (rnd <= ChanceToTargetPlayer)
        {
            _isTargetSurvivor = true;
        }
    }

    public void SetTargetSurvivor(GameObject target)
    {
        _targetSurvivor = target;
    }

    public void SetCrystalRef(GameObject crystal)
    {
        _crystal = crystal;
    }
}
