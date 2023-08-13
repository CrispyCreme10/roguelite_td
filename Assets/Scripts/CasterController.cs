using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class CasterController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private List<ProjectileBundle> projectiles;

    [SerializeField] private GameObject spawnPos;
    
    [Header("Display")]
    [ReadOnly]
    [SerializeField] private List<ProjectileCooldown> cooldowns;
    
    private Dictionary<ProjectileSlot, ProjectileSO> _projectileMap;
    private Dictionary<ProjectileSlot, ProjectileCooldown> _cooldownMap;
    
    

    private void Start()
    {
        _projectileMap = projectiles.ToDictionary(p => p.Slot, p => p.Data);
        _cooldownMap = projectiles.ToDictionary(p => p.Slot, _ => new ProjectileCooldown());
        cooldowns = _cooldownMap.Keys.Select(k => _cooldownMap[k]).ToList();
    }

    void Update()
    {
        ManageCooldowns();
    }

    private void ManageCooldowns()
    {
        foreach (var key in _cooldownMap.Keys)
        {
            if (!_cooldownMap[key].CanCast)
            {
                _cooldownMap[key].DecreaseCooldown(Time.deltaTime);
            }
        }
    }

    public void CastProjectile(ProjectileSlot projectileSlot, Vector3 castDirection) {
        if (!_cooldownMap[projectileSlot].CanCast) return;
        try
        {
            var projectileSo = _projectileMap[projectileSlot];
            var projectile = Instantiate(projectileSo.Prefab,
                spawnPos.transform.position, Quaternion.identity);
            projectile.SetMoveTowardsDir(castDirection);
            projectile.SetCasterController(this);
            _cooldownMap[projectileSlot].StartCooldown(projectileSo.Cooldown);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

[Serializable]
public class ProjectileBundle
{
    [SerializeField] private ProjectileSlot slot;
    [SerializeField] private ProjectileSO data;
    
    public ProjectileSlot Slot => slot;
    public ProjectileSO Data => data;

}

[Serializable]
public class ProjectileCooldown
{
    [SerializeField] private float cooldown;
    public bool CanCast => cooldown <= 0;

    public void StartCooldown(float c)
    {
        cooldown = c;
    }

    public void DecreaseCooldown(float value)
    {
        cooldown -= value;
    }
}

public enum ProjectileSlot
{
    Primary,
    Secondary,
    Ability1,
    Ability2
}
