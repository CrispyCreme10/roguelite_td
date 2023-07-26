using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ProjectileSO : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private string description;
    [SerializeField] private float cooldown;
    [SerializeField] private int damage;
    [SerializeField] private ProjectileController prefab;

    public ProjectileController Prefab => prefab;

    public int Damage => damage;

    public float Cooldown => cooldown;

    public string Description => description;

    public string Name => name;
}
