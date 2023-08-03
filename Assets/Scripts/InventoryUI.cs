using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour {
    private const float SlotSize = 115f;

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

    private void Awake() {
        SetupRoot();
        SetupDropzones();
        _playerInventorySlots = _root.Q<ScrollView>("InventorySlots");
    }

    private void SetupRoot() {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.RegisterCallback<MouseMoveEvent>(RootMouseMove);
        _root.RegisterCallback<MouseUpEvent>(ImageMouseUp);
    }

    private void SetupDropzones() {
        // equipment
        _equipmentLoadoutSlot1 = _root.Q<VisualElement>("EquipmentSlot1");
        StackItemDragDropHandler.RegisterDropzone(new StackItemDropzone(
            _equipmentLoadoutSlot1,
            new[] { ItemType.Equipment },
            newStackItem => {
                // attempt to add equipment to slot
                Debug.Log("Equipment 1 TARGET");
                var addedEquipmentToLoadout = playerPermDataSo.Loadout.TryAddEquipment(newStackItem, 0);
                return new TargetDropdownResult(addedEquipmentToLoadout, 0);
            },
            (selfStackItem, targetDropdownResult) => {
                if (targetDropdownResult.ItemAccepted) {
                    // remove item from Equipment Slot 1
                    playerPermDataSo.Loadout.TryRemoveEquipment(0);
                }
            }));
        // _equipmentLoadoutSlot2 = _root.Q<VisualElement>("EquipmentSlot2");
        // _dropzoneElements.Add(new DropzoneElement(_equipmentLoadoutSlot2, StackItemContainerType.Equipment, 1));
        // _equipmentLoadoutSlot3 = _root.Q<VisualElement>("EquipmentSlot3");
        // _dropzoneElements.Add(new DropzoneElement(_equipmentLoadoutSlot3, StackItemContainerType.Equipment, 2));
        // // modifiers
        // _modifierLoadoutSlot1 = _root.Q<VisualElement>("ModifierSlot1");
        // _dropzoneElements.Add(new DropzoneElement(_modifierLoadoutSlot1, StackItemContainerType.Modifier));
        // _modifierLoadoutSlot2 = _root.Q<VisualElement>("ModifierSlot2");
        // _dropzoneElements.Add(new DropzoneElement(_modifierLoadoutSlot2, StackItemContainerType.Modifier, 1));
        // _modifierLoadoutSlot3 = _root.Q<VisualElement>("ModifierSlot3");
        // _dropzoneElements.Add(new DropzoneElement(_modifierLoadoutSlot3, StackItemContainerType.Modifier, 2));
        // // towers
        // _towerLoadoutSlot1 = _root.Q<VisualElement>("TowerSlot1");
        // _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot1, StackItemContainerType.Tower));
        // _towerLoadoutSlot2 = _root.Q<VisualElement>("TowerSlot2");
        // _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot2, StackItemContainerType.Tower, 1));
        // _towerLoadoutSlot3 = _root.Q<VisualElement>("TowerSlot3");
        // _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot3, StackItemContainerType.Tower, 2));
        // _towerLoadoutSlot4 = _root.Q<VisualElement>("TowerSlot4");
        // _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot4, StackItemContainerType.Tower, 3));
        // // utility
        // _utilityLoadoutSlot1 = _root.Q<VisualElement>("UtilitySlot1");
        // _dropzoneElements.Add(new DropzoneElement(_utilityLoadoutSlot1, StackItemContainerType.Utility));
        // _utilityLoadoutSlot2 = _root.Q<VisualElement>("UtilitySlot2");
        // _dropzoneElements.Add(new DropzoneElement(_utilityLoadoutSlot2, StackItemContainerType.Utility, 1));
        // // backpack
        // _backpackLoadoutSlot = _root.Q<VisualElement>("BackpackSlot");
        // _dropzoneElements.Add(new DropzoneElement(_backpackLoadoutSlot, StackItemContainerType.Backpack));
        // // pouch
        // _pouchLoadoutSlot = _root.Q<VisualElement>("PouchSlot");
        // _dropzoneElements.Add(new DropzoneElement(_pouchLoadoutSlot, StackItemContainerType.Pouch));
    }

    private void Start() {
        BuildInventorySlots();
        BuildBackpackSlots();
        BuildPouchSlots();
    }

    private void BuildInventorySlots() {
        var container = new VisualElement {
            style = {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
            }
        };
        for (var i = 0; i < playerPermDataSo.Inventory.Size; i++) {
            var slot = MakeInventorySlot(SlotSize);
            var inventoryStackItem = playerPermDataSo.Inventory.GetItemAtIndex(i);
            var copyIndex = i;
            var slotDropzone = new StackItemDropzone(
                slot,
                new[] {
                    ItemType.Backpack, ItemType.Consumable, ItemType.Crafting, ItemType.Currency,
                    ItemType.Equipment,
                    ItemType.Modifier, ItemType.Pouch, ItemType.Tower, ItemType.Utility
                },
                newStackItem => {
                    // safe to assume new stackItem is valid to be placed here?
                    Debug.Log("Inventory Slot Target: " + newStackItem.Item.name);
                    var remainingAmount = playerPermDataSo.Inventory.TryAddItemAtIndex(newStackItem, copyIndex);
                    return new TargetDropdownResult(remainingAmount > -1, remainingAmount);
                },
                (selfStackItemAmount, targetDropdownResult) => {
                    Debug.Log("Inventory Slot SOURCE: " + targetDropdownResult);
                    if (!targetDropdownResult.ItemAccepted) return;
                    playerPermDataSo.Inventory.TryRemoveItem(copyIndex,
                        selfStackItemAmount - targetDropdownResult.RemainingAmount);
                },
                inventoryStackItem
            );
            StackItemDragDropHandler.RegisterDropzone(slotDropzone);
            if (!inventoryStackItem.IsEmpty) {
                var image = new Image {
                    sprite = inventoryStackItem.Item.Icon,
                    style = {
                        height = new Length(50, LengthUnit.Percent),
                        width = new Length(50, LengthUnit.Percent)
                    }
                };

                slot.Add(image);

                if (inventoryStackItem.Amount > 1) {
                    var label = new Label {
                        text = inventoryStackItem.Amount.ToString(),
                        style = {
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
                
                image.RegisterCallback<MouseDownEvent, StackItemDropzone>(ImageMouseDown, slotDropzone);
            }
            
            container.Add(slot);
        }

        _playerInventorySlots.Add(container);
    }

    private void BuildBackpackSlots() {
        if (playerPermDataSo.Loadout.Backpack == null) return;

        var container = new VisualElement {
            style = {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
            }
        };
    }

    private void BuildPouchSlots() {
        if (playerPermDataSo.Loadout.Pouch == null) return;

        var container = new VisualElement {
            style = {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
            }
        };
    }

    private VisualElement MakeInventorySlot(float size) {
        var newSlot = new VisualElement {
            name = "DropzoneSlot",
            style = {
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

    private void ImageMouseDown(MouseDownEvent evt, StackItemDropzone dropzone) {
        var dragImage = new Image {
            sprite = dropzone.StackItem.Item.Icon,
            style = {
                height = 115f * 0.5f,
                width = 115f * 0.5f
            }
        };
        StackItemDragDropHandler.DragElement = new VisualElement {
            name = "DragElement",
            style = {
                position = new StyleEnum<Position>(Position.Absolute),
                opacity = 0f
            }
        };
        
        StackItemDragDropHandler.SetSourceDropzone(dropzone);
        StackItemDragDropHandler.DragElement.Add(dragImage);
        _root.Add(StackItemDragDropHandler.DragElement);
    }

    private void RootMouseMove(MouseMoveEvent evt) {
        if (!StackItemDragDropHandler.IsDragging) return;

        // lower opacity of item in inventory that is being drug
        StackItemDragDropHandler.ApplySourceDragStyling();

        // put drag inventory item's image under the cursor with lowered opacity
        var mousePos = Input.mousePosition;
        var mousePosAdj = new Vector2(mousePos.x, Screen.height - mousePos.y);
        mousePosAdj = RuntimePanelUtils.ScreenToPanel(_root.panel, mousePosAdj);

        StackItemDragDropHandler.DragElement.style.top =
            mousePosAdj.y - StackItemDragDropHandler.DragElement.worldBound.height / 2;
        StackItemDragDropHandler.DragElement.style.left =
            mousePosAdj.x - StackItemDragDropHandler.DragElement.worldBound.width / 2;
        StackItemDragDropHandler.DragElement.style.opacity = 0.1f;
        StackItemDragDropHandler.DragElement.pickingMode = PickingMode.Ignore;

        // check dropzone overlaps
        var overlappedDropzones = StackItemDragDropHandler.Dropzones.Where(dropzone =>
            dropzone.VisualElement.worldBound.Overlaps(StackItemDragDropHandler.DragElement.worldBound) &&
            dropzone.VisualElement != StackItemDragDropHandler.SourceDropzone.VisualElement).ToList();
        if (overlappedDropzones.Count == 0) {
            LeaveDropzone();
            return;
        }

        foreach (var overlappedDropzone in overlappedDropzones) {
            Debug.Log("OVERLAP: " + overlappedDropzone.VisualElement.name);
            DragOverDropzone(overlappedDropzone);
        }
    }

    private void ImageMouseUp(MouseUpEvent evt) {
        if (!StackItemDragDropHandler.IsDragging) return;

        Debug.Log("IMAGE MOUSE UP: " + StackItemDragDropHandler.IsOverTargetDropzone);

        // if mouse is over a dropzone
        if (StackItemDragDropHandler.IsOverTargetDropzone && StackItemDragDropHandler.IsValidDrop) {
            StackItemDragDropHandler.Drop();
        }

        // cleanup
        _root.Remove(StackItemDragDropHandler.DragElement);
        StackItemDragDropHandler.CleanUp();
    }

    private void DragOverDropzone(StackItemDropzone targetDropzone) {
        if (StackItemDragDropHandler.TargetDropzone == targetDropzone) return;
        StackItemDragDropHandler.SetTargetDropzone(targetDropzone);
    }

    private void LeaveDropzone() {
        if (StackItemDragDropHandler.TargetDropzone == null) return;
        StackItemDragDropHandler.UnsetTargetDropzone();
    }

    // private class DropzoneElement {
    //     private readonly StackItemContainerType _containerType;
    //     private readonly VisualElement _dropzoneSlotVisualElement;
    //     private readonly StackItem _currentStackItem;
    //
    //     // colors
    //     private readonly StyleColor _defaultBgColor;
    //     private readonly Color _green;
    //     private readonly Color _red;
    //     public VisualElement Element { get; }
    //     public int ContainerIndex { get; }
    //
    //     public DropzoneElement(VisualElement element, StackItemContainerType stackItemContainerType, int containerIndex = 0, StackItem stackItem = null) {
    //         _containerType = stackItemContainerType;
    //         ContainerIndex = containerIndex;
    //         _currentStackItem = stackItem;
    //         Element = element;
    //
    //         _dropzoneSlotVisualElement = Element.Q<VisualElement>("DropzoneSlot");
    //         _defaultBgColor = _dropzoneSlotVisualElement.style.backgroundColor;
    //         
    //         _green = Color.green;
    //         _green.a = .05f;
    //         _red = Color.red;
    //         _red.a = .05f;
    //     }
    //
    //     public void HighlightDropzone(StackItem item) {
    //         Debug.Log("HIGHLIGHTING");
    //         _dropzoneSlotVisualElement.style.backgroundColor = IsValidItem(item) ? _green : _red;
    //     }
    //
    //     public void ResetDropzoneColor() {
    //         _dropzoneSlotVisualElement.style.backgroundColor = _defaultBgColor;
    //     }
    //
    //     private bool IsValidItem(StackItem stackItem) {
    //         if (!IsItemTypeAccepted(stackItem.Item.ItemType)) return false;
    //         if (_containerType != StackItemContainerType.Inventory) {
    //             return _currentStackItem == null;
    //         }
    //
    //         return !_currentStackItem?.IsMaxed ?? true;
    //     }
    //
    //     private bool IsItemTypeAccepted(ItemType dragItemType) {
    //         switch (dragItemType) {
    //             case ItemType.Currency:
    //             case ItemType.Crafting:
    //             case ItemType.Consumable:
    //                 return _containerType == StackItemContainerType.Inventory;
    //             case ItemType.Equipment:
    //                 return _containerType is StackItemContainerType.Inventory or StackItemContainerType.Equipment;
    //             case ItemType.Modifier:
    //                 return _containerType is StackItemContainerType.Inventory or StackItemContainerType.Modifier;
    //             case ItemType.Utility:
    //                 return _containerType is StackItemContainerType.Inventory or StackItemContainerType.Utility;
    //             case ItemType.Tower:
    //                 return _containerType is StackItemContainerType.Inventory or StackItemContainerType.Tower;
    //             case ItemType.Backpack:
    //                 return _containerType is StackItemContainerType.Inventory or StackItemContainerType.Backpack;
    //             case ItemType.Pouch:
    //                 return _containerType is StackItemContainerType.Inventory or StackItemContainerType.Pouch;
    //             default:
    //                 return _containerType == StackItemContainerType.Inventory;
    //         }
    //     }
    //
    //     public void DropComplete(StackItem stackItem, PlayerPermDataSO playerPermDataSo, int targetIndex) {
    //         switch (_containerType) {
    //             case StackItemContainerType.Equipment:
    //                 playerPermDataSo.Loadout.TryAddEquipment(stackItem, targetIndex);
    //                 break; // add to equipment list
    //             case StackItemContainerType.Modifier:
    //                 break; // add to modifier list
    //             case StackItemContainerType.Tower:
    //                 break; // add to tower list
    //             case StackItemContainerType.Utility:
    //                 break; // add to utility list
    //             case StackItemContainerType.Inventory:
    //                 playerPermDataSo.Inventory.TryAddItemAtIndex(stackItem, targetIndex);
    //                 break; // add to player inventory
    //             case StackItemContainerType.Backpack:
    //                 break; // add to backpack
    //             case StackItemContainerType.Pouch:
    //                 break; // add to pouch
    //             default:
    //                 throw new ArgumentOutOfRangeException();
    //         }
    //     }
    // }
}

public class StackItemDropzone {
    private readonly List<ItemType> _allowedTypes;
    private readonly VisualElement _dropzoneSlotVisualElement;
    private readonly StyleColor _defaultBgColor;
    private readonly Color _green;
    private readonly Color _red;

    public StackItem StackItem { get; }
    public int StackItemContainerIndex { get; }
    public VisualElement VisualElement { get; } // container element that shows item image and potentially stack size
    public Func<StackItem, TargetDropdownResult> TargetEvent { get; }
    public Action<int, TargetDropdownResult> SourceEvent { get; }

    public StackItemDropzone(VisualElement visualElement, IEnumerable<ItemType> allowedTypes,
        Func<StackItem, TargetDropdownResult> targetEvent, Action<int, TargetDropdownResult> sourceEvent,
        StackItem stackItem = null, int stackItemContainerIndex = -1) {
        VisualElement = visualElement;
        _allowedTypes = allowedTypes.ToList();
        StackItem = stackItem;
        StackItemContainerIndex = stackItemContainerIndex;
        TargetEvent = targetEvent;
        SourceEvent = sourceEvent;
        
        _dropzoneSlotVisualElement = VisualElement.Q<VisualElement>("DropzoneSlot");
        _defaultBgColor = _dropzoneSlotVisualElement.style.backgroundColor;
        _green = Color.green;
        _green.a = .05f;
        _red = Color.red;
        _red.a = .05f;
    }

    public void UpdateItemImage() {
        // update UI Image based on the StackItem associated with this dropzone
    }
    
    public bool IsValidItem(StackItem stackItem) {
        if (!IsItemTypeAccepted(stackItem.Item.ItemType)) return false;
    
        return !StackItem?.IsMaxed ?? true;
    }
    
    private bool IsItemTypeAccepted(ItemType dragItemType) {
        return _allowedTypes.Contains(dragItemType);
    }

    public void HighlightDropzone(bool isValid) {
        Debug.Log("HIGHLIGHTING");
        _dropzoneSlotVisualElement.style.backgroundColor = isValid ? _green : _red;
    }
    
    public void ResetDropzoneColor() {
        _dropzoneSlotVisualElement.style.backgroundColor = _defaultBgColor;
    }
}

public static class StackItemDragDropHandler {
    public static List<StackItemDropzone> Dropzones { get; }
    public static StackItemDropzone SourceDropzone { get; private set; }
    public static StackItemDropzone TargetDropzone { get; private set; }
    public static VisualElement DragElement { get; set; }
    public static bool IsValidDrop { get; private set; }
    public static bool IsDragging => SourceDropzone != null;
    public static bool IsOverTargetDropzone => TargetDropzone != null;

    static StackItemDragDropHandler() {
        Dropzones = new List<StackItemDropzone>();
    }

    public static void RegisterDropzone(StackItemDropzone dropzone) {
        Dropzones.Add(dropzone);
    }

    public static void SetSourceDropzone(StackItemDropzone sourceDropzone) {
        SourceDropzone = sourceDropzone;
    }
    
    public static void SetTargetDropzone(StackItemDropzone targetDropzone) {
        if (TargetDropzone != null) {
            // unset previous target
            UnsetTargetDropzone();
        }
        
        TargetDropzone = targetDropzone;
        IsValidDrop = TargetDropzone.IsValidItem(SourceDropzone.StackItem);
        TargetDropzone?.HighlightDropzone(IsValidDrop);
    }

    public static void UnsetTargetDropzone() {
        TargetDropzone.ResetDropzoneColor();
        TargetDropzone = null;
    }

    public static void CreateDragElement() { }

    public static void Drop() {
        var result = TargetDropzone.TargetEvent(SourceDropzone.StackItem);
        SourceDropzone.SourceEvent(SourceDropzone.StackItem.Amount, result);

        if (!result.ItemAccepted) return;

        SourceDropzone.UpdateItemImage();
        TargetDropzone.UpdateItemImage();
    }

    public static void CleanUp() {
        ApplySourceDragCompleteStyling();
        TargetDropzone?.ResetDropzoneColor();
        DragElement = null;
        SourceDropzone = null;
        TargetDropzone = null;
    }

    public static void ApplySourceDragStyling() {
        SourceDropzone.VisualElement.style.opacity = 0.1f;
    }

    private static void ApplySourceDragCompleteStyling() {
        SourceDropzone.VisualElement.style.opacity = 1f;
    }
}

public class TargetDropdownResult {
    public bool ItemAccepted { get; private set; }
    public int RemainingAmount { get; private set; }

    public TargetDropdownResult(bool itemAccepted, int remainingAmount) {
        ItemAccepted = itemAccepted;
        RemainingAmount = remainingAmount;
    }

    public override string ToString() {
        return $"ItemAccepted: {ItemAccepted}, RemainingAmount: {RemainingAmount}";
    }
}