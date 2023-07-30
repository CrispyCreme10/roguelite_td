using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Inventory")]
public class InventorySO : ScriptableObject
{
    public Action OnInventoryUpdate;
    
    [SerializeField] private List<InventoryItem> items;
    [SerializeField] private int size;
    
    public List<InventoryItem> Items => items;
    public int Size => size;
    public bool IsFull => items.Count == size;
    public bool IsEmpty => items.Count == 0;

    public InventoryItem GetItemAtIndex(int index)
    {
        return index < items.Count ? items[index] : null;
    }
    
    public bool TryAddItem(ItemSO item, int amount)
    {
        if (IsFull) return false;
        int stacks = (int)Math.Floor((decimal)item.StackSize / amount);

        if (stacks == 0)
        {
            items.Add(new InventoryItem(item, amount));
            InvokeInventoryUpdate();
            return true;
        }
        
        for (int i = 0; i < stacks; i++)
        {
            items.Add(new InventoryItem(item, item.StackSize));
        }

        int remainingCount = amount % item.StackSize;
        if (remainingCount > 0) items.Add(new InventoryItem(item, remainingCount));

        InvokeInventoryUpdate();
        return true;
    }

    public bool TryRemoveItem(int index, int amount)
    {
        if (index >= items.Count) return false;

        InventoryItem item = items[index];
        if (item.DecreaseAmount(amount) == 0)
        {
            items.RemoveAt(index);
        }

        InvokeInventoryUpdate();
        return true;
    }

    private void InvokeInventoryUpdate()
    {
        OnInventoryUpdate?.Invoke();
    }
}

[Serializable]
public class InventoryItem
{
    [SerializeField] private ItemSO item;
    [SerializeField] private int amount;

    public int Amount => amount;

    public ItemSO Item => item;
    
    public InventoryItem(ItemSO item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }

    public int DecreaseAmount(int amt)
    {
        int newAmt = amount - amt;
        amount = newAmt <= 0 ? 0 : newAmt;
        return amount;
    }
}