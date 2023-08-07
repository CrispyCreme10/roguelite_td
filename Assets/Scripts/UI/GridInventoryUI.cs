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
    [SerializeField] private List<GridItem> testGridItems;
    private const int Rows = 13;
    private const int Cols = 10;
    private const int SlotSize = 60;

    // drag drop perm
    private bool IsDragging => _dragElement != null;
    private readonly Color _validDragColor = new (0, 255, 0, 0.05f);
    private readonly Color _invalidDragColor = new (255, 0, 0, 0.05f);
    
    // drag drop state
    private VisualElement _dragElement;
    private bool _hasClearedDragSource;
    private int _xIndex = -1, _yIndex = -1;
    private int _newRowIndex = -1, _newColIndex = -1;
    private GridItem _dragGridItem;
    private Label _nameLabel;
    private Color _sourceColor;
    private bool _hasBeenRotated;
    private Color[,] _gridSlotsDragSnapshot;
    private Color[,] _nextGridSlots;
    private bool _isValidPlacement;
    
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

            // reset source items slot bg colors
            if (_isValidPlacement) {
                _gridInventory.MoveItem(_newRowIndex, _newColIndex, _dragGridItem);
                
                // update _nextGridSlots with new grid
                RefreshGridSlotsFromSource();
            } else {
                for (var x = _xIndex; x < _dragGridItem.Height + _xIndex; x++) {
                    for (var y = _yIndex; y < _dragGridItem.Width + _yIndex; y++) {
                        _nextGridSlots[x, y] = new Color(_sourceColor.r, _sourceColor.g, _sourceColor.b, _sourceColor.a);
                    }
                }
                
                UpdateGridSlotsFromNext();
            }

            // reset drag drop state
            _root.Remove(_dragElement);
            _dragElement = null;
            _hasClearedDragSource = false;
            _xIndex = -1;
            _yIndex = -1;
            _newRowIndex = -1;
            _newColIndex = -1;
            _dragGridItem = null;
            _nameLabel.style.visibility = Visibility.Visible;
            _nameLabel = null;
            _sourceColor = Color.white;
            _hasBeenRotated = false;
            _gridSlotsDragSnapshot = null;
            _nextGridSlots = null;
            _isValidPlacement = false;
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
        foreach (var testGridItem in testGridItems) {
            _gridInventory.TryAddItem(testGridItem);
        }

        // Use Grid Inventory data to show image(s) in UI

        // create absolutely positioned image that is based off of it's grid position
        // image should also be scaled to the size of its width & height using the slot size
        foreach (var valueTuple in _gridInventory.GetItemPositions()) {
            var image = new Image {
                name = "ItemImage",
                sprite = valueTuple.gridItem.Icon,
                style = {
                    rotate = new Rotate(new Angle(valueTuple.gridItem.IconRotation, AngleUnit.Degree)),
                    width = SlotSize * valueTuple.gridItem.Width * valueTuple.gridItem.IconScale,
                    height = SlotSize * valueTuple.gridItem.Height * valueTuple.gridItem.IconScale,
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
                    width = SlotSize * valueTuple.gridItem.Width,
                    height = SlotSize * valueTuple.gridItem.Height,
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
                        width = SlotSize * valueTuple.gridItem.Width,
                        height = SlotSize * valueTuple.gridItem.Height,
                        position = Position.Absolute
                    }
                };

                var dragImage = new Image {
                    name = "DragImage",
                    sprite = valueTuple.gridItem.Icon,
                    style = {
                        rotate = new Rotate(new Angle(valueTuple.gridItem.IconRotation, AngleUnit.Degree)),
                        width = SlotSize * valueTuple.gridItem.Width * valueTuple.gridItem.IconScale,
                        height = SlotSize * valueTuple.gridItem.Height * valueTuple.gridItem.IconScale,
                    }
                };

                _xIndex = valueTuple.top;
                _yIndex = valueTuple.left;
                _dragGridItem = valueTuple.gridItem;
                _sourceColor = new Color(valueTuple.gridItem.BgColor.r, valueTuple.gridItem.BgColor.g,
                    valueTuple.gridItem.BgColor.b, valueTuple.gridItem.BgColor.a);
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
        for (var x = _xIndex; x < _dragGridItem.Height + _xIndex; x++) {
            for (var y = _yIndex; y < _dragGridItem.Width + _yIndex; y++) {
                _gridSlotsDragSnapshot[x, y] = new Color(_defaultSlotBgColor.r, _defaultSlotBgColor.g,
                    _defaultSlotBgColor.b, _defaultSlotBgColor.a);
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

        var totalSlotWidth = _dragGridItem.Width * SlotSize;
        var totalSlotHeight = _dragGridItem.Height * SlotSize;

        // get top-left corner of the rectangle
        var xStart = mousePosAdj.x - (!_hasBeenRotated ? totalSlotWidth : totalSlotHeight) / 2f;
        var yStart = mousePosAdj.y - (!_hasBeenRotated ? totalSlotHeight : totalSlotWidth) / 2f;

        // create potential offsets to clamp drag element slot highlight styles to container (viewport of scrollview)
        var xEnd = xStart + totalSlotWidth;
        var yEnd = yStart + totalSlotHeight;
        var inventoryViewport = _gridScrollView.Q("unity-content-viewport");

        var shiftXOffset = 0f;
        var shiftYOffset = 0f;
        
        // point(s) exists outside the right/left of the container
        if (xEnd > inventoryViewport.worldBound.xMax) {
            shiftXOffset = inventoryViewport.worldBound.xMax - xEnd; // want negative offset
        } else if (xStart < inventoryViewport.worldBound.xMin) {
            shiftXOffset = inventoryViewport.worldBound.xMin - xStart; // want positive offset
        }

        // point(s) exists outside the bottom/top of the container
        if (yEnd > inventoryViewport.worldBound.yMax) {
            shiftYOffset = inventoryViewport.worldBound.yMax - yEnd; // want negative offset
        } else if (yStart < inventoryViewport.worldBound.yMin) {
            shiftYOffset = inventoryViewport.worldBound.yMin - yStart; // want positive offset
        }
        
        // create Vector2 foreach square that makes up the item (total: width * height) and center those points
        // if cursor is within bounds of grid container then the following applies
        //  foreach point previously created
        //   set _nextGridSlots[row, col] styles
        // run change detection for _gridSlots
        
        var points = new List<Vector2>();
        var gridRowCount = _gridSlots.GetLength(0);
        var gridColCount = _gridSlots.GetLength(1);
        var rotateAdjHeight = (!_hasBeenRotated ? _dragGridItem.Height : _dragGridItem.Width);
        var rotateAdjWidth = (!_hasBeenRotated ? _dragGridItem.Width : _dragGridItem.Height);
        const float offset = SlotSize / 2f;
        for (var row = 0; row < rotateAdjHeight; row++) {
            for (var col = 0; col < rotateAdjWidth; col++) {
                var x = xStart + offset + (col * SlotSize) + shiftXOffset;
                var y = yStart + offset + (row * SlotSize) + shiftYOffset;
                
                points.Add(new Vector2(x, y));
            }
        }

        // only continue if mouse is within the viewport of the inventories scroll view
        if (inventoryViewport.worldBound.Contains(mousePosAdj)) {
            // validate placement by sending top-left coordinate, item width, & item height to data grid
            for (var p = 0; p < points.Count; p++) {
                // get row,col that this point is in
                var row = -1;
                var col = -1;
                var point = points[p];

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

                Debug.Log($"points ({p}): {row}, {col}");
                if (p == 0) {
                    _isValidPlacement = _gridInventory.CanAddItemAtCoordinate(row, col, _dragGridItem, _hasBeenRotated);
                    if (_isValidPlacement) {
                        _newRowIndex = row;
                        _newColIndex = col;
                    }
                }

                // dont need to worry about negative indices since _isValidPlacement should rule those out
                _nextGridSlots[row, col] = _isValidPlacement ? _validDragColor : _invalidDragColor;
            }
        }

        UpdateGridSlotsFromNext();
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

    private void UpdateGridSlotsFromNext() {
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

    private void RefreshGridSlotsFromSource() {
        var gridRowCount = _gridSlots.GetLength(0);
        var gridColCount = _gridSlots.GetLength(1);
        for (var row = 0; row < gridRowCount; row++) {
            for (var col = 0; col < gridColCount; col++) {
                _gridSlots[row, col].style.backgroundColor = _defaultSlotBgColor;
            }
        }

        foreach (var valueTuple in _gridInventory.GetItemPositions()) {
            UpdateSlotsStyles(valueTuple.top, valueTuple.left, valueTuple.gridItem.Width, valueTuple.gridItem.Height,
                valueTuple.gridItem.BgColor);
        }
    }
}