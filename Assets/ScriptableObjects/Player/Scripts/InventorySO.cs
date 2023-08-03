using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Inventory")]
public class InventorySO : ScriptableObject {
    public Action OnInventoryUpdate;

    [SerializeField] private int size;

    [ListDrawerSettings(OnTitleBarGUI = "DrawItemsButtons")] [SerializeField]
    private List<StackItem> items;

    public List<StackItem> Items => items;
    public int Size => size;
    public bool IsFull => items.Count == size;
    public bool IsEmpty => items.Count == 0;
    public int AvailableSlots => items.Count(i => i.IsEmpty);

    public StackItem GetItemAtIndex(int index) {
        return index < items.Count ? items[index] : null;
    }

    /// <summary>
    /// Attempt to add stack(s) of item to beginning of list
    /// </summary>
    /// <param name="stackItem"></param>
    /// <returns></returns>
    public bool TryAddItem(StackItem stackItem) {
        // default validation
        if (IsFull || stackItem.Amount == 0) return false;

        // stack init & stack validation
        var stacks = (int)Math.Floor((decimal)stackItem.Item.StackSize / stackItem.Amount);
        if (stacks + 1 > AvailableSlots) return false; // +1 here since stacks at 0 still means we adding item
        var emptySlotIndex = items.FindIndex(i => i.IsEmpty);
        if (stacks == 0) {
            items[emptySlotIndex] = stackItem;
            InvokeInventoryUpdate();
            return true;
        }

        var newItems = new List<StackItem>();
        for (var i = 0; i < stacks; i++) {
            newItems.Add(new StackItem(stackItem.Item, stackItem.Item.StackSize));
        }

        var remainingCount = stackItem.Amount % stackItem.Item.StackSize;
        if (remainingCount > 0) {
            newItems.Add(new StackItem(stackItem.Item, remainingCount));
        }

        // can be loose with logic here since we validated available slots previously
        foreach (var newItem in newItems) {
            Debug.Log("New Item at index: " + emptySlotIndex);
            items[emptySlotIndex] = newItem;
            emptySlotIndex = items.FindIndex(emptySlotIndex + 1, i => i.IsEmpty);
        }

        InvokeInventoryUpdate();
        return true;
    }

    public int TryAddItemAtIndex(StackItem stackItem, int targetIndex) {
        if (stackItem.Amount == 0 || targetIndex < 0 || targetIndex >= items.Count) return -1;

        var stackItemAtIndex = items[targetIndex];
        if (stackItemAtIndex != null) {
            // return if mismatch types
            if (stackItemAtIndex.Item.name != stackItem.Item.name) return -1;
            
            // potential merge
            var mergeTotal = stackItem.Amount + stackItemAtIndex.Amount;
            var overflow = mergeTotal - stackItemAtIndex.Item.StackSize;
            if (overflow > 0) {
                items[targetIndex].IncreaseAmount(stackItem.Amount - overflow);
                return overflow;
            }

            items[targetIndex].IncreaseAmount(stackItem.Amount);
            return 0;
        }

        items[targetIndex] = stackItem;
        return 0;
    }

    public bool TryRemoveItem(int index, int decreaseAmount) {
        if (index >= items.Count) return false;

        var item = items[index];
        if (item.DecreaseAmount(decreaseAmount) == 0) {
            items.RemoveAt(index);
        }

        InvokeInventoryUpdate();
        return true;
    }

    public bool TrySwapItems(int fromIndex, int toIndex) {
        if (fromIndex < 0 || fromIndex >= size || toIndex < 0 || toIndex >= size) return false;

        (items[fromIndex], items[toIndex]) = (items[toIndex], items[fromIndex]);
        return true;
    }

    private void InvokeInventoryUpdate() {
        OnInventoryUpdate?.Invoke();
    }

    private void DrawItemsButtons() {
        if (!SirenixEditorGUI.ToolbarButton(EditorIcons.ArrowDown)) return;
        for (var i = 0; i < size - items.Count; i++) {
            items.Add(new StackItem());
        }
    }
}

[Serializable]
public class StackItem {
    [SerializeField] private ItemSO item;
    [SerializeField] private int amount;

    public int Amount => amount;
    public ItemSO Item => item;
    public bool IsEmpty => amount == 0;
    public bool IsMaxed => item != null && amount == item.StackSize;

    public StackItem() {
        item = null;
        amount = 0;
    }

    public StackItem(ItemSO item, int amount) {
        this.item = item;
        this.amount = amount;
    }

    public int DecreaseAmount(int amt) {
        int newAmt = amount - amt;
        amount = newAmt <= 0 ? 0 : newAmt;
        return amount;
    }

    public int IncreaseAmount(int amt) {
        if (!CanStack(amount)) return amount;
        amount += amt;
        return amount;
    }

    public bool CanStack(int increaseAmt) {
        return amount + increaseAmt <= item.StackSize;
    }
}

public enum StackItemContainerType {
    Equipment,
    Modifier,
    Tower,
    Utility,
    Inventory,
    Backpack,
    Pouch
}