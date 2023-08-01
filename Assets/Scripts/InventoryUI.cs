using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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
                var addedEquipmentToLoadout = playerPermDataSo.Loadout.TryAddEquipment(newStackItem, 0);
                return new TargetDropdownResult(addedEquipmentToLoadout, 0);
            },
            targetDropdownResult => { }));
        _equipmentLoadoutSlot2 = _root.Q<VisualElement>("EquipmentSlot2");
        _dropzoneElements.Add(new DropzoneElement(_equipmentLoadoutSlot2, StackItemContainerType.Equipment, 1));
        _equipmentLoadoutSlot3 = _root.Q<VisualElement>("EquipmentSlot3");
        _dropzoneElements.Add(new DropzoneElement(_equipmentLoadoutSlot3, StackItemContainerType.Equipment, 2));
        // modifiers
        _modifierLoadoutSlot1 = _root.Q<VisualElement>("ModifierSlot1");
        _dropzoneElements.Add(new DropzoneElement(_modifierLoadoutSlot1, StackItemContainerType.Modifier));
        _modifierLoadoutSlot2 = _root.Q<VisualElement>("ModifierSlot2");
        _dropzoneElements.Add(new DropzoneElement(_modifierLoadoutSlot2, StackItemContainerType.Modifier, 1));
        _modifierLoadoutSlot3 = _root.Q<VisualElement>("ModifierSlot3");
        _dropzoneElements.Add(new DropzoneElement(_modifierLoadoutSlot3, StackItemContainerType.Modifier, 2));
        // towers
        _towerLoadoutSlot1 = _root.Q<VisualElement>("TowerSlot1");
        _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot1, StackItemContainerType.Tower));
        _towerLoadoutSlot2 = _root.Q<VisualElement>("TowerSlot2");
        _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot2, StackItemContainerType.Tower, 1));
        _towerLoadoutSlot3 = _root.Q<VisualElement>("TowerSlot3");
        _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot3, StackItemContainerType.Tower, 2));
        _towerLoadoutSlot4 = _root.Q<VisualElement>("TowerSlot4");
        _dropzoneElements.Add(new DropzoneElement(_towerLoadoutSlot4, StackItemContainerType.Tower, 3));
        // utility
        _utilityLoadoutSlot1 = _root.Q<VisualElement>("UtilitySlot1");
        _dropzoneElements.Add(new DropzoneElement(_utilityLoadoutSlot1, StackItemContainerType.Utility));
        _utilityLoadoutSlot2 = _root.Q<VisualElement>("UtilitySlot2");
        _dropzoneElements.Add(new DropzoneElement(_utilityLoadoutSlot2, StackItemContainerType.Utility, 1));
        // backpack
        _backpackLoadoutSlot = _root.Q<VisualElement>("BackpackSlot");
        _dropzoneElements.Add(new DropzoneElement(_backpackLoadoutSlot, StackItemContainerType.Backpack));
        // pouch
        _pouchLoadoutSlot = _root.Q<VisualElement>("PouchSlot");
        _dropzoneElements.Add(new DropzoneElement(_pouchLoadoutSlot, StackItemContainerType.Pouch));
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
        for (int i = 0; i < playerPermDataSo.Inventory.Size; i++) {
            var slot = MakeInventorySlot(SlotSize);
            DropzoneElement slotDropzone = null;
            var inventoryItem = playerPermDataSo.Inventory.GetItemAtIndex(i);
            if (!inventoryItem.IsEmpty) {
                var image = new Image {
                    sprite = inventoryItem.Item.Icon,
                    style = {
                        height = new Length(50, LengthUnit.Percent),
                        width = new Length(50, LengthUnit.Percent)
                    }
                };

                var bundle = new DragDropEventBundle(inventoryItem, slot);
                image.RegisterCallback<MouseDownEvent, DragDropEventBundle>(ImageMouseDown, bundle);

                slot.Add(image);

                if (inventoryItem.Amount > 1) {
                    var label = new Label {
                        text = inventoryItem.Amount.ToString(),
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

                slotDropzone = new DropzoneElement(slot, StackItemContainerType.Inventory, i, inventoryItem);
            }

            // add inventory slot to dropzone elements
            slotDropzone ??= new DropzoneElement(slot, StackItemContainerType.Inventory);
            _dropzoneElements.Add(slotDropzone);
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

    private void ImageMouseDown(MouseDownEvent evt, DragDropEventBundle bundle) {
        _dragDropEventBundle = bundle;
        var dragImage = new Image {
            sprite = _dragDropEventBundle.StackItem.Item.Icon,
            style = {
                height = 115f * 0.5f,
                width = 115f * 0.5f
            }
        };
        _dragElement = new VisualElement {
            name = "DragElement",
            style = {
                position = new StyleEnum<Position>(Position.Absolute),
                opacity = 0f
            }
        };
        _dragElement.Add(dragImage);
        _root.Add(_dragElement);
    }

    private void RootMouseMove(MouseMoveEvent evt) {
        if (!IsDragging) return;

        // lower opacity of item in inventory that is being drug
        _dragDropEventBundle.SourceVisualElement.style.opacity = 0.1f;

        // put drag inventory item's image under the cursor with lowered opacity
        var mousePos = Input.mousePosition;
        var mousePosAdj = new Vector2(mousePos.x, Screen.height - mousePos.y);
        mousePosAdj = RuntimePanelUtils.ScreenToPanel(_root.panel, mousePosAdj);

        _dragElement.style.top = mousePosAdj.y - _dragElement.worldBound.height / 2;
        _dragElement.style.left = mousePosAdj.x - _dragElement.worldBound.width / 2;
        _dragElement.style.opacity = 0.1f;
        _dragElement.pickingMode = PickingMode.Ignore;

        // check dropzone overlaps
        var overlappedDropzones = _dropzoneElements.Where(dropzoneElement =>
            dropzoneElement.Element.worldBound.Overlaps(_dragElement.worldBound) &&
            dropzoneElement.Element != _dragDropEventBundle.SourceVisualElement).ToList();
        if (overlappedDropzones.Count == 0) {
            LeaveDropzone();
            return;
        }

        foreach (var dropzoneElement in overlappedDropzones) {
            Debug.Log("OVERLAP: " + dropzoneElement.Element.name);
            DragOverDropzone(dropzoneElement);
        }
    }

    private void ImageMouseUp(MouseUpEvent evt) {
        if (!IsDragging) return;

        Debug.Log("IMAGE MOUSE UP: " + ActiveDropzone);

        // if mouse is over a dropzone
        // then send item info to dropzone & remove item from inventory
        if (ActiveDropzone) {
            _dropzone.DropComplete(_dragDropEventBundle.StackItem, playerPermDataSo, _dropzone.ContainerIndex);
        }

        // cleanup
        _dragDropEventBundle.SourceVisualElement.style.opacity = 1f;
        _dragDropEventBundle = null;
        _root.Remove(_dragElement);
        _dragElement = null;
        _dropzone = null;
    }

    private void DragOverDropzone(DropzoneElement dropzoneElement) {
        if (dropzoneElement != null) {
            LeaveDropzone();
        }

        _dropzone = dropzoneElement;
        _dropzone?.HighlightDropzone(_dragDropEventBundle.StackItem);
    }

    private void LeaveDropzone() {
        if (_dropzone == null) return;

        _dropzone.ResetDropzoneColor();
        _dropzone = null;
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
    private StackItem _stackItem;
    private VisualElement _visualElement; // container element that shows item image and potentially stack size
    private List<ItemType> _allowedTypes;

    public Func<StackItem, TargetDropdownResult> TargetFunc { get; }
    public Action<TargetDropdownResult> SourceFunc { get; }

    public StackItemDropzone(VisualElement visualElement, IEnumerable<ItemType> allowedTypes,
        Func<StackItem, TargetDropdownResult> targetFunc, Action<TargetDropdownResult> sourceFunc,
        StackItem stackItem = null) {
        _visualElement = visualElement;
        _allowedTypes = allowedTypes.ToList();
        _stackItem = stackItem;
        TargetFunc = targetFunc;
        SourceFunc = sourceFunc;
    }
}

public static class StackItemDragDropHandler {
    private static List<StackItemDropzone> _dropzones;
    private static StackItemDropzone _sourceDropzone;
    private static StackItemDropzone _targetDropzone;

    static StackItemDragDropHandler() {
        _dropzones = new List<StackItemDropzone>();
    }

    public static bool IsDragging => _sourceDropzone != null;
    public static bool IsOverATargetDropzone => _targetDropzone != null;

    public static void RegisterDropzone(StackItemDropzone dropzone) {
        _dropzones.Add(dropzone);
    }

    public static void Drop() {
        var result = _targetDropzone.TargetFunc();
        _sourceDropzone.SourceFunc(result);
    }
}

public class TargetDropdownResult {
    public bool ItemAccepted { get; private set; }
    public int RemainingAmount { get; private set; }

    public TargetDropdownResult(bool itemAccepted, int remainingAmount) {
        ItemAccepted = itemAccepted;
        RemainingAmount = remainingAmount;
    }
}

// what happens on drop?
// dropzone defines what happens when it RECIEVES a new StackItem AND when it successfully SENDS it's StackItem
// FLOW:
// Pick-up item from _sourceDropzone
// Drag-overs form potential _targetDropzones
// Place item on _targetDropzone
// _targetDropzone runs it's custom func that handles what to do with the request to accept a new item...
//  and returns information to _sourceDropzone based on the result
// _sourceDropzone will then run custom func that handles what to do based on the _targetDropzone's returned results