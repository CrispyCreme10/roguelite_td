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
        Debug.Log($"{itemIndex}, {rowIndex}, {colIndex}, {itemWidth}, {itemHeight}");
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
    private readonly List<GridItem> _gridItems;
    private readonly List<GridItemCache> _gridItemCache;
    private readonly Grid _grid;

    public GridInventory() {
        _gridItems = new List<GridItem>();
        _gridItemCache = new List<GridItemCache>();
        _grid = new Grid(13, 10);
    }

    public bool TryAddItem(GridItem gridItem) {
        _gridItems.Add(gridItem);
        var index = _gridItems.Count - 1;
        var (rowIndex, colIndex, width, height) = _grid.TryAdd(index, gridItem.Width, gridItem.Height);
        if (rowIndex == -1 || colIndex == -1 || width == 0 || height == 0) return false;
        _gridItemCache.Add(new GridItemCache(rowIndex, colIndex, width, height));
        return true;
    }

    public void MoveItem(int rowIndex, int colIndex, GridItem gridItem) {
        var index = _gridItems.IndexOf(gridItem);
        if (index == -1) return;
        var gridItemCache = _gridItemCache[index];
        _grid.Remove(gridItemCache.RowIndex, gridItemCache.ColIndex, gridItemCache.ItemWidth, gridItemCache.ItemHeight);
        _grid.AddAtCoordinate(index, rowIndex, colIndex, gridItem.Width, gridItem.Height);
        _gridItemCache[index] = new GridItemCache(rowIndex, colIndex, gridItem.Width, gridItem.Height);
    }

    public bool CanAddItemAtCoordinate(int xIndex, int yIndex, GridItem gridItem, bool isRotated) {
        var index = _gridItems.IndexOf(gridItem);
        return index != -1 && _grid.CanAddAtCoordinate(index, xIndex, yIndex, !isRotated ? gridItem.Width : gridItem.Height, !isRotated ? gridItem.Height : gridItem.Width);
    }

    public List<(GridItem gridItem, int top, int left)> GetItemPositions() {
        var list = new List<(GridItem gridItem, int top, int left)>();
        for (var i = 0; i < _gridItems.Count; i++) {
            var gridItem = _gridItems[i];
            var pos = _grid.GetTopLeftCoordinateForItem(i);
            list.Add((gridItem, pos.x, pos.y));
        }

        return list;
    }

    private void UpdateGridItemCache(int index, int rowIndex, int colIndex, int width, int height) {
        
    }
}

// candidate for struct?
public class GridItemCache {
    private int _rowIndex;
    private int _colIndex;
    private int _itemWidth;
    private int _itemHeight;
    
    public GridItemCache(int rowIndex = default, int colIndex = default, int itemWidth = default, int itemHeight = default) {
        _rowIndex = rowIndex;
        _colIndex = colIndex;
        _itemWidth = itemWidth;
        _itemHeight = itemHeight;
    }

    public int ItemHeight => _itemHeight;

    public int ItemWidth => _itemWidth;

    public int ColIndex => _colIndex;

    public int RowIndex => _rowIndex;
}