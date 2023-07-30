using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerPermDataSO playerPermDataSo;
    
    private VisualElement _root;
    private VisualElement _equipmentLoadoutSlot1;
    private VisualElement _equipmentLoadoutSlot2;
    private VisualElement _equipmentLoadoutSlot3;
    private VisualElement _modifierLoadoutSlot1;
    private VisualElement _modifierLoadoutSlot2;
    private VisualElement _modifierLoadoutSlot3;
    private VisualElement _towerLoadoutSlot1;
    private VisualElement _towerLoadoutSlot2;
    private VisualElement _towerLoadoutSlot3;
    private VisualElement _towerLoadoutSlot4;
    private VisualElement _utilityLoadoutSlot1;
    private VisualElement _utilityLoadoutSlot2;
    private VisualElement _backpackLoadoutSlot;
    private VisualElement _backpackLoadoutSlots;
    private VisualElement _pouchLoadoutSlot;
    private VisualElement _pouchLoadoutSlots;
    private ScrollView _playerInventorySlots;
    
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _playerInventorySlots = _root.Q<ScrollView>("InventorySlots");
    }

    private void Start()
    {
        BuildInventorySlots();
    }

    private void BuildInventorySlots()
    {
        float size = 115f;
        var container = new VisualElement
        {
            style =
            {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
            }
        };
        for (int i = 0; i < playerPermDataSo.Inventory.Size; i++)
        {
            var slot = MakeInventorySlot(size);

            var inventoryItem = playerPermDataSo.Inventory.GetItemAtIndex(i);
            if (inventoryItem != null)
            {
                var image = new Image
                {
                    sprite = inventoryItem.Item.Icon,
                    style =
                    {
                        height = new Length(50, LengthUnit.Percent),
                        width = new Length(50, LengthUnit.Percent)
                    }
                };
                slot.Add(image);

                if (inventoryItem.Amount > 1)
                {
                    var label = new Label
                    {
                        text = inventoryItem.Amount.ToString(),
                        style =
                        {
                            color = Color.white,
                            fontSize = 20f,
                            position = new StyleEnum<Position>(Position.Absolute),
                            top = 0f,
                            right = 0f,
                            paddingTop = 2f,
                            paddingRight = 5f
                        }
                    };
                    label.AddToClassList("label-unset");
                    slot.Add(label);
                }
            }
            
            container.Add(slot);
        }
        
        _playerInventorySlots.Add(container);
    }

    private VisualElement MakeInventorySlot(float size)
    {
        var newSlot = new VisualElement
        {
            style =
            {
                width = size,
                minWidth = size,
                maxWidth = size,
                height = size,
                minHeight = size,
                maxHeight = size,
                position = new StyleEnum<Position>(Position.Relative),
                justifyContent = new StyleEnum<Justify>(Justify.Center),
                alignItems = new StyleEnum<Align>(Align.Center)
            }
        };
        newSlot.AddToClassList("loadout-slot");
        newSlot.AddToClassList("inventory-slot-border");
        
        return newSlot;
    }
}
