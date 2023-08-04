using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace DragDrop {
    public static class StackItemDragDropHandler {
        private static List<StackItemDropzone> Dropzones { get; }
        private static VisualElement Root { get; set; }
        private static StackItemDropzone SourceDropzone { get; set; }
        private static StackItemDropzone TargetDropzone { get; set; }
        private static VisualElement DragElement { get; set; }
        private static bool IsValidDrop { get; set; }
        private static bool IsDragging => SourceDropzone != null;
        private static bool IsOverTargetDropzone => TargetDropzone != null;

        static StackItemDragDropHandler() {
            Dropzones = new List<StackItemDropzone>();
        }

        public static void RegisterDropzone(StackItemDropzone dropzone) {
            Dropzones.Add(dropzone);
            dropzone.DropzoneVisualElement.InitStackItemElements(dropzone);
        }

        public static void SetRoot(VisualElement root) {
            Root = root;
            Root.RegisterCallback<MouseMoveEvent>(RootMouseMove);
            Root.RegisterCallback<MouseUpEvent>(ImageMouseUp);
        }

        private static void SetSourceDropzone(StackItemDropzone sourceDropzone) {
            SourceDropzone = sourceDropzone;
        }

        private static void SetTargetDropzone(StackItemDropzone targetDropzone) {
            if (TargetDropzone != null) {
                // unset previous target
                UnsetTargetDropzone();
            }

            TargetDropzone = targetDropzone;
            IsValidDrop = TargetDropzone.IsValidItem(SourceDropzone.StackItem);
            TargetDropzone?.HighlightDropzone(IsValidDrop);
        }

        private static void UnsetTargetDropzone() {
            TargetDropzone.ResetDropzoneColor();
            TargetDropzone = null;
        }

        private static void Drop() {
            var result = TargetDropzone.TargetEvent(SourceDropzone.StackItem);
            SourceDropzone.SourceEvent(SourceDropzone.StackItem.Amount, result);

            if (!result.ItemAccepted) return;

            SourceDropzone.UpdateItemImage();
            TargetDropzone.UpdateItemImage();
        }

        private static void CleanUp() {
            ApplySourceDragCompleteStyling();
            TargetDropzone?.ResetDropzoneColor();
            
            DragElement = null;
            SourceDropzone = null;
            TargetDropzone = null;
        }

        private static void ApplySourceDragStyling() {
            SourceDropzone.DropzoneVisualElement.ApplySourceDragStyling();
        }

        private static void ApplySourceDragCompleteStyling() {
            SourceDropzone.DropzoneVisualElement.ApplySourceDragCompleteStyling();
        }
        
        #region Events

        public static void ImageMouseDown(MouseDownEvent evt, StackItemDropzone dropzone) {
            var dragImage = new Image {
                sprite = dropzone.StackItem.Item.Icon,
                style = {
                    height = 115f * ( dropzone.StackItem.Amount > 1 ? 0.5f : 0.85f),
                    width = 115f * ( dropzone.StackItem.Amount > 1 ? 0.5f : 0.85f)
                }
            };
            DragElement = new VisualElement {
                name = "DragElement",
                style = {
                    position = new StyleEnum<Position>(Position.Absolute),
                    opacity = 0f
                }
            };

            SetSourceDropzone(dropzone);
            DragElement.Add(dragImage);
            Root.Add(DragElement);
        }

        private static void RootMouseMove(MouseMoveEvent evt) {
            if (!IsDragging) return;

            // lower opacity of item in inventory that is being drug
            ApplySourceDragStyling();

            // put drag inventory item's image under the cursor with lowered opacity
            var mousePos = Input.mousePosition;
            var mousePosAdj = new Vector2(mousePos.x, Screen.height - mousePos.y);
            mousePosAdj = RuntimePanelUtils.ScreenToPanel(Root.panel, mousePosAdj);

            DragElement.style.top =
                mousePosAdj.y - DragElement.worldBound.height / 2;
            DragElement.style.left =
                mousePosAdj.x - DragElement.worldBound.width / 2;
            DragElement.style.opacity = 0.15f;
            DragElement.pickingMode = PickingMode.Ignore;

            // check dropzone overlaps
            var overlappedDropzones = Dropzones.Where(dropzone =>
                dropzone.DropzoneVisualElement.MainVisualElement.worldBound.Contains(mousePosAdj)).ToList();
            if (overlappedDropzones.Count == 0) {
                LeaveDropzone();
                return;
            }
            
            foreach (var overlappedDropzone in overlappedDropzones) {
                Debug.Log("OVERLAP: " + overlappedDropzone.DropzoneVisualElement.MainVisualElement.name);
                DragOverDropzone(overlappedDropzone);
            }
        }

        private static void ImageMouseUp(MouseUpEvent evt) {
            if (!IsDragging) return;

            Debug.Log("IMAGE MOUSE UP: " + IsOverTargetDropzone);

            // if mouse is over a dropzone
            if (IsOverTargetDropzone && IsValidDrop) {
                Drop();
            }

            // cleanup
            Root.Remove(DragElement);
            CleanUp();
        }
        
        #endregion
    
        private static void DragOverDropzone(StackItemDropzone targetDropzone) {
            if (TargetDropzone == targetDropzone) return;
            SetTargetDropzone(targetDropzone);
        }

        private static void LeaveDropzone() {
            if (TargetDropzone == null) return;
            UnsetTargetDropzone();
        }
    }
}