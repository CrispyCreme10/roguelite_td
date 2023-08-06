using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item")]
public class ItemSO : ScriptableObject
{
    [SerializeField] [TextArea] private string description;
    [SerializeField] private ItemType itemType;
    [SerializeField] private Sprite icon;
    [SerializeField] private int stackSize;

    public int StackSize => stackSize;

    public Sprite Icon => icon;

    public ItemType ItemType => itemType;

    public string Description => description;
}

[CreateAssetMenu(menuName = "Container Item")]
public class ContainerItemSO : ItemSO {
    [SerializeField] private InventorySO containerInventory;
}

public enum ItemType
{
    Currency,
    Crafting,
    Consumable,
    Equipment,
    Ammo,
    Modifier,
    Utility,
    Tower,
    Backpack,
    Pouch
}