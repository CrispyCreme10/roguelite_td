using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Player Loadout")]
public class PlayerLoadoutSO : ScriptableObject {
    [SerializeField] private List<StackItem> equipment = new List<StackItem>(3);
    [SerializeField] private List<StackItem> modifiers = new List<StackItem>(3);
    [SerializeField] private List<StackItem> towers = new List<StackItem>(4);
    [SerializeField] private List<StackItem> utilities = new List<StackItem>(2);
    [SerializeField] private StackItem backpack;
    [SerializeField] private StackItem pouch;
    [SerializeField] private InventorySO backpackSlots;
    [SerializeField] private InventorySO pouchSlots;

    public InventorySO PouchSlots => pouchSlots;

    public InventorySO BackpackSlots => backpackSlots;

    public StackItem Pouch => pouch;

    public StackItem Backpack => backpack;

    public List<StackItem> Utilities => utilities;

    public List<StackItem> Towers => towers;

    public List<StackItem> Modifiers => modifiers;

    public List<StackItem> Equipment => equipment;

    public bool IsEquipmentFull => equipment.Count(e => e.IsMaxed) == equipment.Capacity;
    public bool IsModifiersFull => modifiers.Count == modifiers.Capacity;
    public bool IsTowersFull => towers.Count == towers.Capacity;
    public bool IsUtilitiesFull => utilities.Count == utilities.Capacity;

    public StackItem GetEquipmentAtIndex(int index) {
        if (index < 0 || index >= equipment.Count) return null;
        return equipment[index];
    }
    
    public bool TryAddEquipment(StackItem equipmentItem, int index) {
        if (equipmentItem.Item.ItemType != ItemType.Equipment || index < 0 || index >= equipment.Capacity ||
            IsEquipmentFull || equipment[index].IsMaxed) return false;

        equipment[index].CopyStackItem(equipmentItem);
        return true;
    }

    public bool TryRemoveEquipment(int index) {
        if (index < 0 || index >= equipment.Capacity || equipment[index] == null) return false;

        equipment[index].Reset();
        return true;
    }
}