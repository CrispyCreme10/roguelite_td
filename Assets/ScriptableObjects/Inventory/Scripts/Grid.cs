using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid {
    private int _rows;
    private int _cols;
    private readonly int[,] _gridArray;

    public Grid(int rows, int cols) {
        _rows = rows;
        _cols = cols;

        _gridArray = new int[rows, cols];

        for (var x = 0; x < _gridArray.GetLength(0); x++) {
            for (var y = 0; y < _gridArray.GetLength(1); y++) {
                _gridArray[x, y] = -1;
            }
        }
    }

    public int GetIndexAtCoordinate(int rowIndex, int colIndex) {
        if (rowIndex < 0 || rowIndex >= _rows || colIndex < 0 || colIndex >= _cols)
            return -2; // -1 represent open index
        return _gridArray[rowIndex, colIndex];
    }

    public (int, int, int, int) TryAdd(int itemIndex, int itemWidth, int itemHeight) {
        // look for an open grouping of cells within the grid to place this item
        for (var row = 0; row < _gridArray.GetLength(0); row++) {
            for (var col = 0; col < _gridArray.GetLength(1); col++) {
                if (!CanAddAtCoordinate(itemIndex, row, col, itemWidth, itemHeight)) continue;
                Add(itemIndex, row, col, itemWidth, itemHeight);
                return (row, col, itemWidth, itemHeight);
            }
        }

        return (-1, -1, 0, 0);
    }

    public void AddAtCoordinate(int itemIndex, int rowIndex, int colIndex, int itemWidth, int itemHeight) {
        Add(itemIndex, rowIndex, colIndex, itemWidth, itemHeight);
    }


    public bool CanAddAtCoordinate(int itemIndex, int rowIndex, int colIndex, int itemWidth, int itemHeight) {
        if (rowIndex < 0 || rowIndex + itemHeight > _rows || colIndex < 0 || colIndex + itemWidth > _cols) return false;
        for (var x = rowIndex; x < itemHeight + rowIndex; x++) {
            for (var y = colIndex; y < itemWidth + colIndex; y++) {
                if (_gridArray[x, y] != -1 && _gridArray[x, y] != itemIndex)
                    return false;
            }
        }

        return true;
    }

    private void Add(int itemIndex, int xIndex, int yIndex, int itemWidth, int itemHeight) {
        for (var x = xIndex; x < itemHeight + xIndex; x++) {
            for (var y = yIndex; y < itemWidth + yIndex; y++) {
                _gridArray[x, y] = itemIndex;
            }
        }
    }

    public void Remove(int xIndex, int yIndex, int itemWidth, int itemHeight) {
        for (var x = xIndex; x < itemHeight + xIndex; x++) {
            for (var y = yIndex; y < itemWidth + yIndex; y++) {
                _gridArray[x, y] = -1;
            }
        }
    }

    public (int x, int y) GetTopLeftCoordinateForItem(int itemIndex) {
        for (var x = 0; x < _gridArray.GetLength(0); x++) {
            for (var y = 0; y < _gridArray.GetLength(1); y++) {
                if (_gridArray[x, y] == itemIndex) {
                    return (x, y);
                }
            }
        }

        return (-1, -1);
    }
}

public class GridInventory {
    private readonly Dictionary<int, StackGridItem> _gridItems;
    private readonly Grid _grid;
    private int _autoIncrementingIndex;

    public GridInventory() {
        _gridItems = new Dictionary<int, StackGridItem>();
        _grid = new Grid(13, 10);
    }

    private void AddGridItem(StackGridItem stackGridItem) {
        _gridItems[_autoIncrementingIndex++] = stackGridItem;
    }

    public bool TryAddItem(StackGridItem stackGridItem) {
        if (stackGridItem.Amount == 0) return false;
        var indexSnapshot = _autoIncrementingIndex;
        var (rowIndex, colIndex, width, height) =
            _grid.TryAdd(indexSnapshot, stackGridItem.GridItem.Width, stackGridItem.GridItem.Height);
        if (rowIndex == -1 || colIndex == -1 || width == 0 || height == 0) return false;

        AddGridItem(stackGridItem);
        UpdateGridData(rowIndex, colIndex, false, stackGridItem);
        return true;
    }

    private static void UpdateGridData(int rowIndex, int colIndex, bool isRotated, StackGridItem stackGridItem) {
        stackGridItem.UpdateGridData(rowIndex, colIndex, isRotated);
    }

    public void MoveItem(int rowIndex, int colIndex, int gridItemIndex, bool isRotated) {
        if (gridItemIndex == -1) return;
        
        RemoveGridRef(gridItemIndex);
        AddItemAtCoordinate(rowIndex, colIndex, gridItemIndex, isRotated);
    }

    public void MergeItems(int rowIndex, int colIndex, int gridItemIndex) {
        var itemIndex = _grid.GetIndexAtCoordinate(rowIndex, colIndex);
        if (itemIndex < 0) return;
        var mergingItem = _gridItems[gridItemIndex];
        var squashItem = _gridItems[itemIndex];
        squashItem.IncreaseAmount(mergingItem.Amount);
        // remove item at gridItemIndex
        RemoveItemAtIndex(gridItemIndex);
    }

    /// <summary>
    /// Remove old ref in grid
    /// </summary>
    /// <param name="gridItemIndex"></param>
    private void RemoveGridRef(int gridItemIndex) {
        _grid.Remove(_gridItems[gridItemIndex].RowIndex, _gridItems[gridItemIndex].ColIndex,
            _gridItems[gridItemIndex].WidthAdjForRotation, _gridItems[gridItemIndex].HeightAdjForRotation);
    }
    
    private void RemoveItemAtIndex(int gridItemIndex) {
        RemoveGridRef(gridItemIndex);
        _gridItems.Remove(gridItemIndex);
    }

    private void AddItemAtCoordinate(int rowIndex, int colIndex, int gridItemIndex, bool isRotated) {
        var stackGridItem = _gridItems[gridItemIndex];
        stackGridItem.UpdateGridData(rowIndex, colIndex, isRotated); // update before add??
        _grid.AddAtCoordinate(gridItemIndex, rowIndex, colIndex, stackGridItem.WidthAdjForRotation,
            stackGridItem.HeightAdjForRotation);
    }

    public (bool, bool) IsValidPlacement(int rowIndex, int colIndex, int gridItemIndex, bool isRotated) {
        var stackGridItem = _gridItems[gridItemIndex];
        if (IsSameCoordinate(rowIndex, colIndex, stackGridItem) && !isRotated)
            return (false, false);
        
        // only 1x1 items have the option to merge; can then be further overridden metadata
        var canMerge = false;
        if (stackGridItem.GridItem.Width == 1 && stackGridItem.GridItem.Height == 1) {
            canMerge = CanMergeWithItemAtCoordinate(rowIndex, colIndex, gridItemIndex);
        }

        return (canMerge, CanAddItemAtCoordinate(rowIndex, colIndex, gridItemIndex, isRotated));
    }

    private bool CanMergeWithItemAtCoordinate(int rowIndex, int colIndex, int gridItemIndex) {
        var indexAtCoordinate = _grid.GetIndexAtCoordinate(rowIndex, colIndex);
        if (indexAtCoordinate < 0) return false;
        var stackGridItem = _gridItems[gridItemIndex];
        return _gridItems[indexAtCoordinate].CanMerge(stackGridItem);
    }

    private bool CanAddItemAtCoordinate(int rowIndex, int colIndex, int gridItemIndex, bool isRotated) {
        // handle being in the same spot as the GridItem's origin
        var stackGridItem = _gridItems[gridItemIndex];
        if (IsSameCoordinate(rowIndex, colIndex, stackGridItem) && !isRotated)
            return false;
        return gridItemIndex != -1 && _grid.CanAddAtCoordinate(gridItemIndex, rowIndex, colIndex,
            !isRotated ? stackGridItem.GridItem.Width : stackGridItem.GridItem.Height,
            !isRotated ? stackGridItem.GridItem.Height : stackGridItem.GridItem.Width);
    }

    private static bool IsSameCoordinate(int rowIndex, int colIndex, StackGridItem stackGridItem) {
        return rowIndex == stackGridItem.RowIndex && colIndex == stackGridItem.ColIndex;
    }

    public List<(int gridItemIndex, StackGridItem stackGridItem, int top, int left)> GetItemPositions() {
        var list = new List<(int gridItemIndex, StackGridItem stackGridItem, int top, int left)>();
        for (var i = 0; i < _gridItems.Count; i++) {
            var stackGridItem = _gridItems[i];
            var pos = _grid.GetTopLeftCoordinateForItem(i);
            list.Add((i, stackGridItem, pos.x, pos.y));
        }

        return list;
    }
}