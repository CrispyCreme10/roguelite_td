using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "Merchant")]
public class MerchantSO : SerializedScriptableObject {
    [SerializeField] private int nextRestockEpoch;
    [SerializeField] private int reputation;
    [SerializeField] private Dictionary<ItemSO, ShopDef> shopItems;
}

[Serializable]
public struct ShopDef {
    public float chanceToRestockSingleItem;
    public int restockAmount;
    public int maxStackSize;
    public int buyPrice;
    public int sellPrice;
}