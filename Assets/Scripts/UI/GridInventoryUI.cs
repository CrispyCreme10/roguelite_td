using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GridInventoryUI : MonoBehaviour {
    private VisualElement _root;
    private ScrollView _gridScrollView;
    private VisualElement[,] _gridSlots;
    private readonly Color _defaultSlotBgColor = new Color(0, 0, 0, 0.49f);

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
    private Color _sourceColor;
    private bool _hasBeenRotated = false;
    private Color[,] _gridSlotsDragSnapshot;
    private Color[,] _nextGridSlots;

    // keybinds
    private PlayerInputActions _playerInputActions;


    private void Awake() {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _root.RegisterCallback<PointerMoveEvent>(evt => {
            if (!IsDragging) return;

            if (!_hasClearedDragSource)
                ClearDragElementSourceStyles();

            UpdateSlotsOnDrag();
        });
        _root.RegisterCallback<PointerUpEvent>(evt => {
            if (!IsDragging) return;

            UpdateSlotsStyles(_xIndex, _yIndex, _width, _height, _sourceColor);
            _xIndex = -1;
            _yIndex = -1;
            _width = 0;
            _height = 0;
            _nameLabel.style.visibility = Visibility.Visible;
            _nameLabel = null;
            _hasClearedDragSource = false;
            _root.Remove(_dragElement);
            _dragElement = null;
            _gridSlotsDragSnapshot = null;
            _nextGridSlots = null;
        });
        _gridScrollView = _root.Q<ScrollView>("GridScrollView");
        SetupGridInventory();

        // Keybinds
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Inventory.Enable();
    }

    private void OnEnable() {
        _playerInputActions.Inventory.RotateDragItem.performed += RotateDragItem;
    }

    private void OnDisable() {
        _playerInputActions.Inventory.RotateDragItem.performed -= RotateDragItem;
    }

    private void SetupGridInventory() {
        // UI Toolkit Stuff
        var contentContainer = _gridScrollView.Q<VisualElement>("unity-content-container");
        contentContainer.style.flexDirection = FlexDirection.Row;
        contentContainer.style.flexWrap = Wrap.Wrap;
        contentContainer.style.position = Position.Relative;

        // Slot grid & slot visual element creation
        _gridSlots = new VisualElement[Rows, Cols];
        for (var x = 0; x < _gridSlots.GetLength(0); x++) {
            for (var y = 0; y < _gridSlots.GetLength(1); y++) {
                var gridSlot = new VisualElement {
                    name = "GridSlot"
                };
                gridSlot.AddToClassList("grid-slot");
                contentContainer.Add(gridSlot);
                _gridSlots[x, y] = gridSlot;
            }
        }

        // Test Grid Inventory Stuff
        _gridInventory = new GridInventory();
        _gridInventory.AddItem(testGridItem);

        // Use Grid Inventory data to show image(s) in UI

        // create absolutely positioned image that is based off of it's grid position
        // image should also be scaled to the size of its width & height using the slot size
        foreach (var valueTuple in _gridInventory.GetItemPositions()) {
            var image = new Image {
                name = "ItemImage",
                sprite = valueTuple.gridItem.Icon,
                style = {
                    width = SlotSize * testGridItem.Width * testGridItem.IconScale,
                    height = SlotSize * testGridItem.Height * testGridItem.IconScale,
                }
            };

            var nameLabel = new Label {
                name = "ImageLabel",
                text = valueTuple.gridItem.ShortName,
                style = {
                    color = Color.white,
                    fontSize = 12f,
                    paddingTop = 0f,
                    paddingRight = 0f,
                    paddingBottom = 0f,
                    paddingLeft = 0f,
                    marginTop = 1f,
                    marginRight = 1f,
                    marginBottom = 0f,
                    marginLeft = 0f,
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
                _dragElement = new VisualElement {
                    style = {
                        // backgroundColor = new Color(0, 0, 255, 0.05f), // DEBUG COLOR
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
                        width = SlotSize * testGridItem.Width * testGridItem.IconScale,
                        height = SlotSize * testGridItem.Height * testGridItem.IconScale,
                    }
                };

                _xIndex = valueTuple.top;
                _yIndex = valueTuple.left;
                _width = valueTuple.gridItem.Width;
                _height = valueTuple.gridItem.Height;
                _sourceColor = valueTuple.gridItem.BgColor;
                _nameLabel = nameLabel;
                _dragElement.Add(dragImage);
                _root.Add(_dragElement);
                TakeGridSnapshot();
            });

            imageContainer.Add(image);
            imageContainer.Add(nameLabel);
            contentContainer.Add(imageContainer);
            UpdateSlotsStyles(valueTuple.top, valueTuple.left, valueTuple.gridItem.Width, valueTuple.gridItem.Height,
                valueTuple.gridItem.BgColor);
        }
    }

    private void UpdateSlotsStyles(int xIndex, int yIndex, int width, int height, Color bgColor) {
        for (var x = xIndex; x < height + xIndex; x++) {
            for (var y = yIndex; y < width + yIndex; y++) {
                _gridSlots[x, y].style.backgroundColor = new Color(bgColor.r, bgColor.g, bgColor.b, bgColor.a);
            }
        }
    }

    private void ClearDragElementSourceStyles() {
        // reset bg color of relevant grid slots
        // override snapshot
        for (var x = _xIndex; x < _height + _xIndex; x++) {
            for (var y = _yIndex; y < _width + _yIndex; y++) {
                _gridSlotsDragSnapshot[x, y] = new Color(_defaultSlotBgColor.r, _defaultSlotBgColor.g, _defaultSlotBgColor.b, _defaultSlotBgColor.a);
            }
        }

        // hide item label
        _nameLabel.style.visibility = Visibility.Hidden;

        _hasClearedDragSource = true;
    }

    private void UpdateSlotsOnDrag() {
        // put drag inventory item's image under the cursor with lowered opacity
        var mousePos = Input.mousePosition;
        var mousePosAdj = new Vector2(mousePos.x, Screen.height - mousePos.y);
        mousePosAdj = RuntimePanelUtils.ScreenToPanel(_root.panel, mousePosAdj);

        _dragElement.style.top = mousePosAdj.y -
                                 (!_hasBeenRotated
                                     ? _dragElement.worldBound.height
                                     : _dragElement.worldBound.width) / 2;
        _dragElement.style.left = mousePosAdj.x -
                                  (!_hasBeenRotated
                                      ? _dragElement.worldBound.width
                                      : _dragElement.worldBound.height) / 2;
        _dragElement.style.visibility = Visibility.Visible;

        var adjWidth = _width * SlotSize;
        var adjHeight = _height * SlotSize;

        // get top-left corner of the rectangle
        var xStart = mousePosAdj.x - (!_hasBeenRotated ? adjWidth : adjHeight) / 2f;
        var yStart = mousePosAdj.y - (!_hasBeenRotated ? adjHeight : adjWidth) / 2f;

        // create Vector2 foreach square that makes up the item (total: width * height) and center those points
        // if cursor is within bounds of grid container then the following applies
        //  foreach point previously created
        //   set _nextGridSlots[row, col] styles
        // run change detection for _gridSlots

        var points = new List<Vector2>();
        var gridRowCount = _gridSlots.GetLength(0);
        var gridColCount = _gridSlots.GetLength(1);
        const float offset = SlotSize / 2f;
        for (var row = 0; row < _height; row++) {
            for (var col = 0; col < _width; col++) {
                var x = xStart + offset + (col * SlotSize);
                var y = yStart + offset + (row * SlotSize);
                points.Add(new Vector2(x, y));
            }
        }

        // only continue if mouse is within the viewport of the inventories scroll view
        var inventoryViewport = _gridScrollView.Q("unity-content-viewport");
        if (!inventoryViewport.worldBound.Contains(mousePosAdj)) return;

        foreach (var point in points) {
            // get row,col that this point is in
            var row = -1;
            var col = -1;

            // if point.x or point.y is outside bounds of viewport, skip to next point
            if (!inventoryViewport.worldBound.Contains(point)) continue;

            for (var i = 0; i < Math.Max(gridRowCount, gridColCount); i++) {
                var rect = _gridSlots[Math.Min(i, gridRowCount - 1), Math.Min(i, gridColCount - 1)].worldBound;
                // check if point.y is in this row
                if (i < gridRowCount && row == -1 && point.y >= rect.yMin && point.y < rect.yMax) {
                    row = i;
                }

                // check if point.x is in this col
                if (i < gridColCount && col == -1 && point.x >= rect.xMin && point.x < rect.xMax) {
                    col = i;
                }

                if (row > -1 && col > -1)
                    break;
            }

            if (row > -1 && col > -1)
                _nextGridSlots[row, col] = new Color(0, 255, 0, 0.05f);
        }

        GridSlotsChangeDetection();
    }

    private void RotateDragItem(InputAction.CallbackContext context) {
        if (!IsDragging) return;

        // rotate image 90deg if it hasn't been rotated; else rotate -90deg
        _dragElement.style.rotate = !_hasBeenRotated
            ? new Rotate(new Angle(90f, AngleUnit.Degree))
            : new Rotate(0f);

        // flip flag
        _hasBeenRotated = !_hasBeenRotated;

        UpdateSlotsOnDrag();
    }

    private void GridSlotsChangeDetection() {
        var gridRowCount = _gridSlots.GetLength(0);
        var gridColCount = _gridSlots.GetLength(1);
        for (var x = 0; x < gridRowCount; x++) {
            for (var y = 0; y < gridColCount; y++) {
                _gridSlots[x, y].style.backgroundColor = new Color(_nextGridSlots[x, y].r, _nextGridSlots[x, y].g,
                    _nextGridSlots[x, y].b, _nextGridSlots[x, y].a);
                _nextGridSlots[x, y] = new Color(_gridSlotsDragSnapshot[x, y].r, _gridSlotsDragSnapshot[x, y].g,
                    _gridSlotsDragSnapshot[x, y].b, _gridSlotsDragSnapshot[x, y].a); // reset
            }
        }
    }

    private void TakeGridSnapshot() {
        var gridRowCount = _gridSlots.GetLength(0);
        var gridColCount = _gridSlots.GetLength(1);
        _gridSlotsDragSnapshot = new Color[gridRowCount, gridColCount];
        _nextGridSlots = new Color[gridRowCount, gridColCount];
        for (var x = 0; x < gridRowCount; x++) {
            for (var y = 0; y < gridColCount; y++) {
                _gridSlotsDragSnapshot[x, y] = new Color(_gridSlots[x, y].resolvedStyle.backgroundColor.r,
                    _gridSlots[x, y].resolvedStyle.backgroundColor.g,
                    _gridSlots[x, y].resolvedStyle.backgroundColor.b, _gridSlots[x, y].resolvedStyle.backgroundColor.a);
                _nextGridSlots[x, y] = new Color(_gridSlotsDragSnapshot[x, y].r, _gridSlotsDragSnapshot[x, y].g,
                    _gridSlotsDragSnapshot[x, y].b, _gridSlotsDragSnapshot[x, y].a); // initialize
            }
        }
    }
}