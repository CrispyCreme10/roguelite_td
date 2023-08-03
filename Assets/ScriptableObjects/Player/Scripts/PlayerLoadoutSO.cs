using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Loadout")]
public class PlayerLoadoutSO : ScriptableObject {
    [SerializeField] private List<ItemSO> equipment = new List<ItemSO>(3);
    [SerializeField] private List<ItemSO> modifiers = new List<ItemSO>(3);
    [SerializeField] private List<ItemSO> towers = new List<ItemSO>(4);
    [SerializeField] private List<ItemSO> utilities = new List<ItemSO>(2);
    [SerializeField] private ItemSO backpack;
    [SerializeField] private ItemSO pouch;
    [SerializeField] private InventorySO backpackSlots;
    [SerializeField] private InventorySO pouchSlots;

    public InventorySO PouchSlots => pouchSlots;

    public InventorySO BackpackSlots => backpackSlots;

    public ItemSO Pouch => pouch;

    public ItemSO Backpack => backpack;

    public List<ItemSO> Utilities => utilities;

    public List<ItemSO> Towers => towers;

    public List<ItemSO> Modifiers => modifiers;

    public List<ItemSO> Equipment => equipment;

    public bool IsEquipmentFull => equipment.Count == equipment.Capacity;
    public bool IsModifiersFull => modifiers.Count == modifiers.Capacity;
    public bool IsTowersFull => towers.Count == towers.Capacity;
    public bool IsUtilitiesFull => utilities.Count == utilities.Capacity;

    public bool TryAddEquipment(StackItem equipmentItem, int index) {
        if (equipmentItem.Item.ItemType != ItemType.Equipment || index < 0 || index >= equipment.Capacity ||
            IsEquipmentFull || equipment[index] != null) return false;

        equipment[index] = equipmentItem.Item;
        return true;
    }

    public bool TryRemoveEquipment(int index) {
        if (index < 0 || index >= equipment.Capacity || equipment[index] == null) return false;

        equipment[index] = null;
        return true;
    }
}