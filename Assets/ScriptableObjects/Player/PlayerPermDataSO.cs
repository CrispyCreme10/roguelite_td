using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Game Data")]
public class PlayerPermDataSO : ScriptableObject
{
    [SerializeField] private InventorySO inventory;

    public InventorySO Inventory => inventory;
}
