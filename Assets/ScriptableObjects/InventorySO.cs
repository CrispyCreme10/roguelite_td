using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySO : ScriptableObject
{
    [SerializeField] private List<InventoryItem> items;

    public List<InventoryItem> Items => items;
}

[Serializable]
public class InventoryItem
{
    [SerializeField] private ItemSO item;
    [SerializeField] private int count;
}