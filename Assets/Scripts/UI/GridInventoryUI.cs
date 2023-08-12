using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GridInventoryUI : MonoBehaviour {
    private VisualElement _root;
    private ScrollView _gridScrollView;
    private VisualElement _inventoryViewport;
    private VisualElement _contentContainer;
    private VisualElement[,] _gridSlots;
    private Label _itemNameTooltip;
    private Vector2 _mousePosAdj;
    private bool _isHover;
    private readonly Color _defaultSlotBgColor = new Color(0, 0, 0, 0.49f);
    private readonly Color _hoverSlotBgColor = new Color(96, 96, 95, 0.05f);

    // grid
    private GridInventory _gridInventory;
    [SerializeField] private List<GridItem> testGridItems;
    private const int Rows = 13;
    private const int Cols = 10;
    private const int SlotSize = 60;

    // drag drop perm
    private bool IsDragging => _dragElement != null;
    private readonly Color _validDragColor = new(0, 255, 0, 0.05f);
    private readonly Color _invalidDragColor = new(255, 0, 0, 0.05f);

    // drag drop state
    private VisualElement _dragElement;
    private bool _hasClearedDragSource;
    private int _xIndex = -1, _yIndex = -1;
    private int _newRowIndex = -1, _newColIndex = -1;
    private StackGridItem _dragStackGridItem;
    private int _dragGridItemIndex = -1;
    private Label _nameLabel;
    private Label _statLabel;
    private Color _sourceColor;
    private bool _isRotatedFromUserInput;
    private Color[,] _gridSlotsDragSnapshot;
    private Color[,] _nextGridSlots;
    private bool _isValidPlacement;
    private bool _isMerge;

    // keybinds
    private PlayerInputActions _playerInputActions;

    private void Awake() {
        SetupRoot();

        // Visual Elements
        _gridScrollView = _root.Q<ScrollView>("GridScrollView");
        _inventoryViewport = _gridScrollView.Q("unity-content-viewport");
        _contentContainer = _gridScrollView.Q<VisualElement>("unity-content-container");
        _contentContainer.style.flexDirection = FlexDirection.Row;
        _contentContainer.style.flexWrap = Wrap.Wrap;
        _contentContainer.style.position = Position.Relative;
        SetupNameTooltip();
        
        SetupGridInventory();
        PaintGridInventory();

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

    private void SetupRoot() {
        _root = GetComponent<UIDocument>().rootVisualElement;
        // POINTER MOVE EVENT
        _root.RegisterCallback<PointerMoveEvent>(evt => {
            // keep!!
            SetMousePosAdj();
            
            // update tooltip
            if (_isHover)
                SetNameTooltipPosition();

            if (!IsDragging) return;
            
            // hide tooltip if applicable
            if (_itemNameTooltip.style.display == DisplayStyle.Flex)
                HideNameTooltip();

            if (!_hasClearedDragSource)
                ClearDragElementSourceStyles();

            // update drag element
            _dragElement.style.top = _mousePosAdj.y -
                                     (!_isRotatedFromUserInput
                                         ? _dragElement.worldBound.height
                                         : _dragElement.worldBound.width) / 2;
            _dragElement.style.left = _mousePosAdj.x -
                                      (!_isRotatedFromUserInput
                                          ? _dragElement.worldBound.width
                                          : _dragElement.worldBound.height) / 2;
            _dragElement.style.visibility = Visibility.Visible;

            UpdateSlotsOnDrag();
        });

        // POINTER UP EVENT
        _root.RegisterCallback<PointerUpEvent>(evt => {
            if (!IsDragging || evt.button != 0) return;

            if (_isValidPlacement && _inventoryViewport.worldBound.Contains(_mousePosAdj)) {
                if (!_isMerge)
                    _gridInventory.MoveItem(_newRowIndex, _newColIndex, _dragGridItemIndex, _isRotatedFromUserInput);
                else
                    _gridInventory.MergeItems(_newRowIndex, _newColIndex, _dragGridItemIndex);

                // update _nextGridSlots with new grid
                RepaintGridInventory();
            } else {
                for (var x = _xIndex; x < _dragStackGridItem.HeightAdjForRotation + _xIndex; x++) {
                    for (var y = _yIndex; y < _dragStackGridItem.WidthAdjForRotation + _yIndex; y++) {
                        _nextGridSlots[x, y] =
                            new Color(_sourceColor.r, _sourceColor.g, _sourceColor.b, _sourceColor.a);
                    }
                }

                UpdateGridSlotsFromNext();
            }

            // reset drag drop state
            _root.Remove(_dragElement);
            _dragElement = null;
            _dragGridItemIndex = -1;
            _hasClearedDragSource = false;
            _xIndex = -1;
            _yIndex = -1;
            _newRowIndex = -1;
            _newColIndex = -1;
            _dragStackGridItem = null;
            _nameLabel.style.visibility = Visibility.Visible;
            _nameLabel = null;
            _sourceColor = Color.white;
            _isRotatedFromUserInput = false;
            _gridSlotsDragSnapshot = null;
            _nextGridSlots = null;
            _isValidPlacement = false;
            _isMerge = false;
        });
    }
    
    private void SetMousePosAdj() {
        var mousePos = Input.mousePosition;
        var mousePosAdj = new Vector2(mousePos.x, Screen.height - mousePos.y);
        _mousePosAdj = RuntimePanelUtils.ScreenToPanel(_root.panel, mousePosAdj);
    }
    
    private void SetupGridInventory() {
        // Test Grid Inventory Stuff
        _gridInventory = new GridInventory(13, 10);
        foreach (var testGridItem in testGridItems) {
            var newItem = testGridItem.ItemName == "Gold"
                ? new StackGridItem(testGridItem, 5_000)
                : new StackGridItem(testGridItem, 1);
            _gridInventory.TryAddItem(newItem);
        }
    }

    private void PaintGridInventory() {
        // Slot grid & slot visual element creation
        _gridSlots = new VisualElement[Rows, Cols];
        for (var x = 0; x < _gridSlots.GetLength(0); x++) {
            for (var y = 0; y < _gridSlots.GetLength(1); y++) {
                var gridSlot = new VisualElement {
                    name = "GridSlot"
                };
                gridSlot.AddToClassList("grid-slot");
                _contentContainer.Add(gridSlot);
                _gridSlots[x, y] = gridSlot;
            }
        }

        // create absolutely positioned image that is based off of it's grid position
        // image should also be scaled to the size of its width & height using the slot size
        foreach (var valueTuple in _gridInventory.GetItemPositions()) {
            var itemWidth = valueTuple.stackGridItem.WidthAdjForRotation * SlotSize;
            var itemHeight = valueTuple.stackGridItem.HeightAdjForRotation * SlotSize;
            var imageRotation = new Rotate(new Angle(valueTuple.stackGridItem.GridItem.IconRotation, AngleUnit.Degree));
            if (valueTuple.stackGridItem.IsRotated) {
                imageRotation.angle = new Angle(imageRotation.angle.value + 90f, AngleUnit.Degree);
            }

            var itemContainer = new VisualElement {
                name = "ItemContainer",
                style = {
                    justifyContent = Justify.Center,
                    alignItems = Align.Center,
                    width = itemWidth,
                    height = itemHeight,
                    position = Position.Absolute,
                    top = valueTuple.top * SlotSize,
                    left = valueTuple.left * SlotSize
                }
            };

            var image = new Image {
                name = "ItemImage",
                sprite = valueTuple.stackGridItem.GridItem.Icon,
                pickingMode = PickingMode.Ignore,
                style = {
                    rotate = imageRotation,
                    scale = new Vector2(valueTuple.stackGridItem.GridItem.IconScale,
                        valueTuple.stackGridItem.GridItem.IconScale)
                }
            };
            itemContainer.Add(image);

            var nameLabel = new Label {
                name = "ImageLabel",
                text = valueTuple.stackGridItem.GridItem.ShortName,
                style = {
                    color = new Color(164, 171, 175),
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
            itemContainer.Add(nameLabel);

            Label statLabel = null;
            if (valueTuple.stackGridItem.Amount > 1) {
                statLabel = new Label {
                    name = "StatLabel",
                    text = valueTuple.stackGridItem.Amount.ToString(),
                    style = {
                        color = valueTuple.stackGridItem.GridItem.StatColor,
                        fontSize = 12f,
                        paddingTop = 0f,
                        paddingRight = 0f,
                        paddingBottom = 0f,
                        paddingLeft = 0f,
                        marginTop = 0f,
                        marginRight = 1f,
                        marginBottom = 1f,
                        marginLeft = 0f,
                        position = Position.Absolute,
                        right = 0,
                        bottom = 0
                    }
                };
                itemContainer.Add(statLabel);
            }

            itemContainer.RegisterCallback<PointerDownEvent>(evt => {
                if (evt.button == 1) {
                    // show context menu at mouse position
                    // Info
                    // Open (if container item)
                    // Remove
                }
                
                if (evt.button != 0) return;

                _dragElement = new VisualElement {
                    style = {
                        visibility = Visibility.Hidden,
                        justifyContent = Justify.Center,
                        alignItems = Align.Center,
                        width = itemWidth,
                        height = itemHeight,
                        position = Position.Absolute
                    }
                };

                var dragImage = new Image {
                    name = "DragImage",
                    sprite = valueTuple.stackGridItem.GridItem.Icon,
                    style = {
                        rotate = imageRotation,
                        width = itemWidth * valueTuple.stackGridItem.GridItem.IconScale,
                        height = itemHeight * valueTuple.stackGridItem.GridItem.IconScale,
                    }
                };

                _xIndex = valueTuple.top;
                _yIndex = valueTuple.left;
                _dragStackGridItem = valueTuple.stackGridItem;
                _dragGridItemIndex = valueTuple.gridItemIndex;
                _sourceColor = new Color(valueTuple.stackGridItem.GridItem.BgColor.r,
                    valueTuple.stackGridItem.GridItem.BgColor.g,
                    valueTuple.stackGridItem.GridItem.BgColor.b, valueTuple.stackGridItem.GridItem.BgColor.a);
                _nameLabel = nameLabel;
                _statLabel = statLabel;
                _dragElement.Add(dragImage);
                _root.Add(_dragElement);
                TakeGridSnapshot();
            });
            
            itemContainer.RegisterCallback<PointerEnterEvent>(evt => {
                _isHover = true;
                
                // highlight grid slots
                UpdateSlotsStyles(valueTuple.top, valueTuple.left, valueTuple.stackGridItem.WidthAdjForRotation,
                    valueTuple.stackGridItem.HeightAdjForRotation, _hoverSlotBgColor);
                
                // show item name tooltip after 500ms delay
                ShowNameTooltip(valueTuple.stackGridItem.GridItem.ItemName);
            });
            
            itemContainer.RegisterCallback<PointerLeaveEvent>(evt => {
                _isHover = false;
                
                // unhighlight grid slots
                UpdateSlotsStyles(valueTuple.top, valueTuple.left, valueTuple.stackGridItem.WidthAdjForRotation,
                    valueTuple.stackGridItem.HeightAdjForRotation, valueTuple.stackGridItem.GridItem.BgColor);
                
                // hide item name tooltip
                HideNameTooltip();
            });

            _contentContainer.Add(itemContainer);
            UpdateSlotsStyles(valueTuple.top, valueTuple.left, valueTuple.stackGridItem.WidthAdjForRotation,
                valueTuple.stackGridItem.HeightAdjForRotation, valueTuple.stackGridItem.GridItem.BgColor);
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
        for (var x = _xIndex; x < _dragStackGridItem.HeightAdjForRotation + _xIndex; x++) {
            for (var y = _yIndex; y < _dragStackGridItem.WidthAdjForRotation + _yIndex; y++) {
                _gridSlotsDragSnapshot[x, y] = new Color(_defaultSlotBgColor.r, _defaultSlotBgColor.g,
                    _defaultSlotBgColor.b, _defaultSlotBgColor.a);
            }
        }

        // hide item label
        _nameLabel.style.visibility = Visibility.Hidden;

        // hide stat label
        if (_statLabel != null) {
            _statLabel.style.visibility = Visibility.Hidden;
        }

        _hasClearedDragSource = true;
    }

    private void UpdateSlotsOnDrag() {
        var isRotated = _isRotatedFromUserInput ^ _dragStackGridItem.IsRotated;
        var rotateAdjHeight =
            (!isRotated ? _dragStackGridItem.GridItem.Height : _dragStackGridItem.GridItem.Width);
        var rotateAdjWidth =
            (!isRotated ? _dragStackGridItem.GridItem.Width : _dragStackGridItem.GridItem.Height);

        var totalSlotWidth = rotateAdjWidth * SlotSize;
        var totalSlotHeight = rotateAdjHeight * SlotSize;

        // get top-left corner of the rectangle
        var xStart = _mousePosAdj.x - totalSlotWidth / 2f;
        var yStart = _mousePosAdj.y - totalSlotHeight / 2f;

        // create potential offsets to clamp drag element slot highlight styles to container (viewport of scrollview)
        var xEnd = xStart + totalSlotWidth;
        var yEnd = yStart + totalSlotHeight;

        var shiftXOffset = 0f;
        var shiftYOffset = 0f;

        // point(s) exists outside the right/left of the container
        if (xEnd > _inventoryViewport.worldBound.xMax) {
            shiftXOffset = _inventoryViewport.worldBound.xMax - xEnd; // want negative offset
        } else if (xStart < _inventoryViewport.worldBound.xMin) {
            shiftXOffset = _inventoryViewport.worldBound.xMin - xStart; // want positive offset
        }

        // point(s) exists outside the bottom/top of the container
        if (yEnd > _inventoryViewport.worldBound.yMax) {
            shiftYOffset = _inventoryViewport.worldBound.yMax - yEnd; // want negative offset
        } else if (yStart < _inventoryViewport.worldBound.yMin) {
            shiftYOffset = _inventoryViewport.worldBound.yMin - yStart; // want positive offset
        }

        // create Vector2 foreach square that makes up the item (total: width * height) and center those points
        // if cursor is within bounds of grid container then the following applies
        //  foreach point previously created
        //   set _nextGridSlots[row, col] styles
        // run change detection for _gridSlots
        var points = new List<Vector2>();
        var gridRowCount = _gridSlots.GetLength(0);
        var gridColCount = _gridSlots.GetLength(1);
        const float offset = SlotSize / 2f;
        for (var row = 0; row < rotateAdjHeight; row++) {
            for (var col = 0; col < rotateAdjWidth; col++) {
                var x = xStart + offset + (col * SlotSize) + shiftXOffset;
                var y = yStart + offset + (row * SlotSize) + shiftYOffset;
                var point = new Vector2(x, y);

                points.Add(point);
            }
        }

        // only continue if mouse is within the viewport of the inventories scroll view
        if (_inventoryViewport.worldBound.Contains(_mousePosAdj)) {
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

                if (p == 0) {
                    var (canMerge, canPlace) =
                        _gridInventory.IsValidPlacement(row, col, _dragGridItemIndex, _isRotatedFromUserInput);
                    _isValidPlacement = canMerge || canPlace;
                    if (canMerge) {
                        _isMerge = true;
                    }

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

        // flip flag
        _isRotatedFromUserInput = !_isRotatedFromUserInput;

        // rotate image based on UI rotation (user action) and its stored rotation (StackGridItem -> IsRotated)
        _dragElement.style.rotate = _isRotatedFromUserInput
            ? new Rotate(new Angle(!_dragStackGridItem.IsRotated ? 90f : -90f, AngleUnit.Degree))
            : new Rotate(0f);

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

    private void RepaintGridInventory() {
        _contentContainer.Clear();
        PaintGridInventory();
    }

    private void SetupNameTooltip() {
        _itemNameTooltip = new Label {
            name = "ItemNameTooltip"
        };
        _itemNameTooltip.AddToClassList("item-name-tooltip");
        _root.Add(_itemNameTooltip);
    }
    
    private void ShowNameTooltip(string itemName) {
        _itemNameTooltip.text = itemName;
        _itemNameTooltip.style.display = DisplayStyle.Flex;
        SetNameTooltipPosition();
    }

    private void SetNameTooltipPosition() {
        _itemNameTooltip.style.top = _mousePosAdj.y - 35;
        _itemNameTooltip.style.left = _mousePosAdj.x + 5;
    }

    private void HideNameTooltip() {
        _itemNameTooltip.text = "";
        _itemNameTooltip.style.display = DisplayStyle.None;
    }
}