using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GridInventoryUI : MonoBehaviour {
    private VisualElement _root;
    private ScrollView _gridScrollView;
    private VisualElement[,] _slotGrid;
    private Color _defaultSlotBgColor;

    // grid
    private GridInventory _gridInventory;
    [SerializeField] private GridItem testGridItem;
    private const int Rows = 13;
    private const int Cols = 10;
    private const int SlotSize = 60;
    
    // drag drop
    private bool IsDragging => _dragElement != null;
    private VisualElement _dragElement;
    private bool _hasClearedDragSource;
    private int _xIndex = -1, _yIndex = -1, _width, _height;
    private Label _nameLabel;

    private void Awake() {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.RegisterCallback<PointerMoveEvent>(evt => {
            if (!IsDragging) return;
            
            Debug.Log("DRAG");
            
            if (!_hasClearedDragSource)
                ClearDragElementSourceStyles();
            
            // put drag inventory item's image under the cursor with lowered opacity
            var mousePos = Input.mousePosition;
            var mousePosAdj = new Vector2(mousePos.x, Screen.height - mousePos.y);
            mousePosAdj = RuntimePanelUtils.ScreenToPanel(_root.panel, mousePosAdj);
            
            _dragElement.style.top =
                mousePosAdj.y - _dragElement.worldBound.height / 2;
            _dragElement.style.left =
                mousePosAdj.x - _dragElement.worldBound.width / 2;
            _dragElement.style.visibility = Visibility.Visible;
        });
        _root.RegisterCallback<PointerUpEvent>(evt => {
            if (!IsDragging) return;
            
            Debug.Log("MOUSE UP");

            _xIndex = -1;
            _yIndex = -1;
            _width = 0;
            _height = 0;
            _nameLabel.style.visibility = Visibility.Visible;
            _nameLabel = null;
            _hasClearedDragSource = false;
            _root.Remove(_dragElement);
            _dragElement = null;
        });
        _gridScrollView = _root.Q<ScrollView>("GridScrollView");
        SetupGridInventory();
    }

    private void SetupGridInventory() {
        // UI Toolkit Stuff
        var contentContainer = _gridScrollView.Q<VisualElement>("unity-content-container");
        contentContainer.style.flexDirection = FlexDirection.Row;
        contentContainer.style.flexWrap = Wrap.Wrap;
        contentContainer.style.position = Position.Relative;

        // Slot grid & slot visual element creation
        _slotGrid = new VisualElement[Rows, Cols];
        for (var x = 0; x < _slotGrid.GetLength(0); x++) {
            for (var y = 0; y < _slotGrid.GetLength(1); y++) {
                var gridSlot = new VisualElement {
                    name = "GridSlot"
                };
                gridSlot.AddToClassList("grid-slot");
                contentContainer.Add(gridSlot);
                _slotGrid[x, y] = gridSlot;
                if (x == 0 && y == 0) {
                    _defaultSlotBgColor = gridSlot.style.backgroundColor.value;
                }
                    
            }
        }

        // Test Grid Inventory Stuff
        _gridInventory = new GridInventory();
        _gridInventory.AddItem(testGridItem);

        // Use Grid Inventory data to show image(s) in UI

        // create absolutely positioned image that is based off of it's grid position
        // image should also be scaled to the size of its width & height using the slot size
        const float imageScale = 0.75f;
        foreach (var valueTuple in _gridInventory.GetItemPositions()) {
            var image = new Image {
                name = "ItemImage",
                sprite = valueTuple.gridItem.Icon,
                style = {
                    width = SlotSize * testGridItem.Width * imageScale,
                    height = SlotSize * testGridItem.Height * imageScale,
                }
            };

            var nameLabel = new Label {
                name = "ImageLabel",
                text = valueTuple.gridItem.ItemName,
                style = {
                    color = Color.white,
                    fontSize = 14f,
                    position = Position.Absolute,
                    top = 0,
                    right = 0
                }
            };

            var imageContainer = new VisualElement {
                name = "ImageContainer",
                style = {
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    width = SlotSize * testGridItem.Width,
                    height = SlotSize * testGridItem.Height,
                    position = Position.Absolute,
                    top = valueTuple.top,
                    left = valueTuple.left
                }
            };

            imageContainer.RegisterCallback<PointerDownEvent>(evt => {
                Debug.Log("MOUSE DOWN");
                
                _dragElement = new VisualElement {
                    style = {
                        visibility = Visibility.Hidden,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center,
                        width = SlotSize * testGridItem.Width,
                        height = SlotSize * testGridItem.Height,
                        position = Position.Absolute
                    }
                };
                
                var dragImage = new Image {
                    name = "DragImage",
                    sprite = valueTuple.gridItem.Icon,
                    style = {
                        width = SlotSize * testGridItem.Width * imageScale,
                        height = SlotSize * testGridItem.Height * imageScale,
                    }
                };

                _nameLabel = nameLabel;
                _dragElement.Add(dragImage);
                _root.Add(_dragElement);
            });

            imageContainer.Add(nameLabel);
            imageContainer.Add(image);
            contentContainer.Add(imageContainer);
            _xIndex = valueTuple.top;
            _yIndex = valueTuple.left;
            _width = valueTuple.gridItem.Width;
            _height = valueTuple.gridItem.Height;
            UpdateSlotsStyles(_xIndex, _yIndex, _width, _height, valueTuple.gridItem.BgColor);
        }
    }

    private void UpdateSlotsStyles(int xIndex, int yIndex, int width, int height, Color bgColor) {
        for (var x = xIndex; x < width + xIndex; x++) {
            for (var y = yIndex; y < height + yIndex; y++) {
                _slotGrid[x, y].style.backgroundColor = bgColor;
            }
        }
    }

    private void ClearDragElementSourceStyles() {
        // reset bg color of relevant grid slots
        UpdateSlotsStyles(_xIndex, _yIndex, _width, _height, _defaultSlotBgColor);
        
        // hide item label
        _nameLabel.style.visibility = Visibility.Hidden;
        
        _hasClearedDragSource = true;
    }
}
