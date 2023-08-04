using DragDrop;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomVisualElements {
    public class DropzoneVisualElement {
        private readonly VisualElement _dropzoneSlotVisualElement;
        private readonly StyleColor _defaultBgColor;
        private readonly Color _green;
        private readonly Color _red;

        private Image _itemImage;
        private Label _itemCountLabel;
        
        public VisualElement MainVisualElement { get; }

        public DropzoneVisualElement(VisualElement mainVisualElement) {
            MainVisualElement = mainVisualElement;
            _dropzoneSlotVisualElement = MainVisualElement.Q<VisualElement>("DropzoneSlot");
            _defaultBgColor = _dropzoneSlotVisualElement.style.backgroundColor;
            _green = Color.green;
            _green.a = .05f;
            _red = Color.red;
            _red.a = .05f;
        }
        
        public void InitStackItemElements(StackItemDropzone parentDropzone) {
            if (parentDropzone.StackItem == null) return;
            CreateAndSetImage(parentDropzone);
            if (parentDropzone.StackItem.Amount <= 1) return;
            CreateAndSetLabel(parentDropzone.StackItem);
        }

        private static Image CreateImage(StackItem stackItem) {
            var length = new Length(stackItem.Amount > 1 ? 50 : 90, LengthUnit.Percent);
            
            return new Image {
                name = "ItemImage",
                sprite = stackItem.Item.Icon,
                style = {
                    height = length,
                    width = length
                }
            };
        }

        private static Label CreateLabel(StackItem stackItem) {
            var label = new Label {
                name = "ItemCount",
                text = stackItem.Amount.ToString(),
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
            return label;
        }

        private void CreateAndSetImage(StackItemDropzone parentDropzone) {
            if (parentDropzone.StackItem.IsEmpty) return;
            _itemImage = CreateImage(parentDropzone.StackItem);
            MainVisualElement.Add(_itemImage);
            _itemImage.RegisterCallback<MouseDownEvent, StackItemDropzone>(StackItemDragDropHandler.ImageMouseDown, parentDropzone);
        }
        
        private void CreateAndSetLabel(StackItem stackItem) {
            _itemCountLabel = CreateLabel(stackItem);
            MainVisualElement.Add(_itemCountLabel);
        }

        private bool UpdateImage(Sprite newIcon) {
            if (_itemImage == null) return false;
            _itemImage.sprite = newIcon;
            return true;
        }

        private bool UpdateLabel(int amount) {
            if (_itemCountLabel == null) return false;
            
            _itemCountLabel.text = amount.ToString();
            return true;
        }

        private void RemoveImage() {
            MainVisualElement.Remove(_itemImage);
            _itemImage = null;
        }

        private void RemoveLabel() {
            MainVisualElement.Remove(_itemCountLabel);
            _itemCountLabel = null;
        }

        public void UpdateElements(StackItemDropzone parentDropzone) {
            if (parentDropzone.StackItem.Item == null) {
                if (_itemImage != null) 
                    RemoveImage();
            } else {
                var success = UpdateImage(parentDropzone.StackItem.Item.Icon);
                if (!success)
                    CreateAndSetImage(parentDropzone);
            }

            if (parentDropzone.StackItem.Amount <= 0) {
                if (_itemCountLabel != null)
                    RemoveLabel();
            } else {
                var success = UpdateLabel(parentDropzone.StackItem.Amount);
                if (!success)
                    CreateAndSetLabel(parentDropzone.StackItem);
            }
        }
        
        public void HighlightDropzone(bool isValid) {
            _dropzoneSlotVisualElement.style.backgroundColor = isValid ? _green : _red;
        }
        
        public void ResetDropzoneColor() {
            _dropzoneSlotVisualElement.style.backgroundColor = _defaultBgColor;
        }
        
        public void ApplySourceDragStyling() {
            MainVisualElement.style.opacity = 0.1f;
        }

        public void ApplySourceDragCompleteStyling() {
            MainVisualElement.style.opacity = 1f;
        }
    }
}