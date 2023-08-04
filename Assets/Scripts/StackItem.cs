using System;
using UnityEngine;

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
        var newAmt = amount - amt;
        amount = newAmt <= 0 ? 0 : newAmt;
        return amount;
    }

    public int IncreaseAmount(int amt) {
        if (!CanStack(amt)) return amount;
        amount += amt;
        return amount;
    }

    public bool CanStack(int increaseAmt) {
        return amount + increaseAmt <= item.StackSize;
    }

    public void CopyStackItem(StackItem itemToCopy) {
        item = itemToCopy.item;
        amount = itemToCopy.amount;
    }
    
    public void Reset() {
        item = null;
        amount = 0;
    }

    public bool IsSameItem(StackItem stackItem) {
        if (IsEmpty || stackItem.IsEmpty) return true; // return true if one of the stackItems is empty
        return item.name == stackItem.item.name;
    }
}