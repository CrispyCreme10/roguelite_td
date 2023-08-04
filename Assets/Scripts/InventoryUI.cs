using System.Linq;
using CustomVisualElements;
using DragDrop;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour {
    private const float SlotSize = 115f;

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
        StackItemDragDropHandler.SetRoot(_root);
    }

    private void SetupDropzones() {
        // equipment
        _equipmentLoadoutSlot1 = _root.Q<VisualElement>("EquipmentSlot1");
        StackItemDragDropHandler.RegisterDropzone(new StackItemDropzone(
            new DropzoneVisualElement(_equipmentLoadoutSlot1),
            new[] { ItemType.Equipment },
            retrievalIndex => Singleton.Instance.PlayerManager.PlayerData.Loadout.GetEquipmentAtIndex(retrievalIndex),
            newStackItem => {
                // attempt to add equipment to slot
                Debug.Log("Equipment 1 TARGET");
                var addedEquipmentToLoadout = Singleton.Instance.PlayerManager.PlayerData.Loadout.TryAddEquipment(newStackItem, 0);
                return new TargetDropdownResult(addedEquipmentToLoadout, 0);
            },
            (selfStackItem, targetDropdownResult) => {
                if (targetDropdownResult.ItemAccepted) {
                    // remove item from Equipment Slot 1
                    Singleton.Instance.PlayerManager.PlayerData.Loadout.TryRemoveEquipment(0);
                }
            },0));
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
        for (var i = 0; i < Singleton.Instance.PlayerManager.PlayerData.Inventory.Size; i++) {
            var dropzoneVisualElement = MakeInventorySlot(SlotSize);
            var copyIndex = i;
            var slotDropzone = new StackItemDropzone(
                dropzoneVisualElement,
                new[] {
                    ItemType.Backpack, ItemType.Consumable, ItemType.Crafting, ItemType.Currency,
                    ItemType.Equipment,
                    ItemType.Modifier, ItemType.Pouch, ItemType.Tower, ItemType.Utility
                },
                retrievalIndex => Singleton.Instance.PlayerManager.PlayerData.Inventory.GetItemAtIndex(retrievalIndex),
                newStackItem => {
                    // safe to assume new stackItem is valid to be placed here?
                    Debug.Log("Inventory Slot Target: " + newStackItem.Item.name);
                    var remainingAmount = Singleton.Instance.PlayerManager.PlayerData.Inventory.TryAddItemAtIndex(newStackItem, copyIndex);
                    return new TargetDropdownResult(remainingAmount > -1, remainingAmount);
                },
                (selfStackItemAmount, targetDropdownResult) => {
                    Debug.Log("Inventory Slot SOURCE: " + targetDropdownResult);
                    if (!targetDropdownResult.ItemAccepted) return;
                    Singleton.Instance.PlayerManager.PlayerData.Inventory.TryRemoveItem(copyIndex,
                        selfStackItemAmount - targetDropdownResult.RemainingAmount);
                },
                copyIndex
            );
            StackItemDragDropHandler.RegisterDropzone(slotDropzone);
            container.Add(dropzoneVisualElement.MainVisualElement);
        }

        _playerInventorySlots.Add(container);
    }

    private static void BuildBackpackSlots() {
        if (Singleton.Instance.PlayerManager.PlayerData.Loadout.Backpack == null) return;

        var container = new VisualElement {
            style = {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
            }
        };
    }

    private static void BuildPouchSlots() {
        if (Singleton.Instance.PlayerManager.PlayerData.Loadout.Pouch == null) return;

        var container = new VisualElement {
            style = {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row),
                flexWrap = new StyleEnum<Wrap>(Wrap.Wrap)
            }
        };
    }

    private static DropzoneVisualElement MakeInventorySlot(float size) {
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

        return new DropzoneVisualElement(newSlot);
    }
}