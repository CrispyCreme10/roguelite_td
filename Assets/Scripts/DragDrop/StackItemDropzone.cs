using System;
using System.Collections.Generic;
using System.Linq;
using CustomVisualElements;
using UnityEngine;

namespace DragDrop {
    public class StackItemDropzone {
        private readonly List<ItemType> _allowedTypes;
    
        public StackItem StackItem => GetStackItemEvent(StackItemContainerIndex);
        private int StackItemContainerIndex { get; }
        public DropzoneVisualElement DropzoneVisualElement { get; }
        private Func<int, StackItem> GetStackItemEvent { get; }
        public Func<StackItem, TargetDropdownResult> TargetEvent { get; }
        public Action<int, TargetDropdownResult> SourceEvent { get; }

        public StackItemDropzone(DropzoneVisualElement dropzoneVisualElement, IEnumerable<ItemType> allowedTypes,
            Func<int, StackItem> getStackItemEvent, Func<StackItem, TargetDropdownResult> targetEvent, 
            Action<int, TargetDropdownResult> sourceEvent, int stackItemContainerIndex = -1) {
            DropzoneVisualElement = dropzoneVisualElement;
            _allowedTypes = allowedTypes.ToList();
            StackItemContainerIndex = stackItemContainerIndex;
            GetStackItemEvent = getStackItemEvent;
            TargetEvent = targetEvent;
            SourceEvent = sourceEvent;
        }

        public void UpdateItemImage() {
            DropzoneVisualElement.UpdateElements(this);
        }

        public bool IsValidItem(StackItem sourceStackItem) {
            if (!IsItemTypeAccepted(sourceStackItem.Item.ItemType) || !sourceStackItem.IsSameItem(StackItem)) return false;
            return !StackItem?.IsMaxed ?? true;
        }

        private bool IsItemTypeAccepted(ItemType dragItemType) {
            return _allowedTypes.Contains(dragItemType);
        }

        public void HighlightDropzone(bool isValid) {
            Debug.Log("HIGHLIGHTING");
            DropzoneVisualElement.HighlightDropzone(isValid);
        }

        public void ResetDropzoneColor() {
            DropzoneVisualElement.ResetDropzoneColor();
        }
    }
}