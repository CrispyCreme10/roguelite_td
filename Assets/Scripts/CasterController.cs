using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CasterController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private GameObject ProjectileSpawnPoint;

    [Header("Settings")]
    [SerializeField] private float FireRate = 1f;

    bool _canCast = true;
    float _timeRemaining;

    void Update()
    {
        if (!_canCast)
        {
            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0)
            {
                _canCast = true;
            }
        }
    }

    public void CastProjectile(Vector3 CastDirection)
    {
        if (_canCast)
        {
            var projectile = Instantiate(ProjectilePrefab, ProjectileSpawnPoint.transform.position, Quaternion.identity);
            projectile.GetComponent<ProjectileController>().SetMoveTowardsDir(CastDirection);
            _canCast = false;
            _timeRemaining = FireRate;
        }
    }
}
