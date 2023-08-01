using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Game Data")]
public class PlayerPermDataSO : ScriptableObject {
    [SerializeField] private PlayerLoadoutSO loadout;
    [SerializeField] private InventorySO inventory;

    public PlayerLoadoutSO Loadout => loadout;
    public InventorySO Inventory => inventory;
}
