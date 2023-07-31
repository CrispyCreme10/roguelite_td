using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    
    // drag drop
    private bool IsDragging => _dragInventoryItem != null;
    private bool ActiveDropzone => _dropzoneElement != null;
    private InventoryItem _dragInventoryItem;
    private Image _dragImage;
    private DropzoneElement _dropzoneElement;
    
    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.RegisterCallback<MouseMoveEvent>(RootMouseMove);
        _playerInventorySlots = _root.Q<ScrollView>("InventorySlots");
        
        // setup dropzones
        _equipmentLoadoutSlot1 = _root.Q<VisualElement>("EquipmentSlot1");
        var equipment1Dropzone = new DropzoneElement(ItemType.Equipment, _equipmentLoadoutSlot1);
        // equipment1Dropzone.Element.RegisterCallback();
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
                
                image.RegisterCallback<MouseDownEvent, InventoryItem>(ImageMouseDown, inventoryItem);
                image.RegisterCallback<MouseUpEvent>(ImageMouseUp);
                
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

    private void ImageMouseDown(MouseDownEvent evt, InventoryItem inventoryItem)
    {
        _dragInventoryItem = inventoryItem;
        _dragImage = new Image
        {
            sprite = inventoryItem.Item.Icon
        };
    }

    private void RootMouseMove(MouseMoveEvent evt)
    {
        if (!IsDragging) return;
        
        // put drag inventory item's image under the cursor with lowered opacity
        
    }

    private void ImageMouseUp(MouseUpEvent evt)
    {
        // if mouse is over a dropzone
        // then send item info to dropzone & remove item from inventory

    }

    private class DropzoneElement
    {
        public ItemType AcceptedItemType { get; }
        public VisualElement Element { get; }
        
        public DropzoneElement(ItemType acceptedItemType, VisualElement element)
        {
            AcceptedItemType = acceptedItemType;
            Element = element;
            
            element.RegisterCallback<MouseEnterEvent>(ElementMouseEnter);
        }

        private void ElementMouseEnter(MouseEnterEvent evt)
        {
            
        }
    }
}