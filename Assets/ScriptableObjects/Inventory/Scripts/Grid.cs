using System.Collections;
using System.Collections.Generic;

public class Grid {
    private int rows;
    private int cols;
    private int[,] gridArray;

    public Grid(int rows, int cols) {
        this.rows = rows;
        this.cols = cols;

        gridArray = new int[rows, cols];

        for (var x = 0; x < gridArray.GetLength(0); x++) {
            for (var y = 0; y < gridArray.GetLength(1); y++) {
                gridArray[x, y] = -1;
            }
        }
    }

    public void Add(int itemIndex, int xIndex, int yIndex, int itemWidth, int itemHeight) {
        for (var x = xIndex; x < itemWidth + xIndex; x++) {
            for (var y = yIndex; y < itemHeight + yIndex; y++) {
                gridArray[x, y] = itemIndex;
            }
        }
    }
    
    public (int x, int y) GetTopLeftCoordinateForItem(int itemIndex) {
        for (var x = 0; x < gridArray.GetLength(0); x++) {
            for (var y = 0; y < gridArray.GetLength(1); y++) {
                if (gridArray[x, y] == itemIndex) {
                    return (x, y);
                }
            }
        }

        return (-1, -1);
    }
}

public class GridInventory {
    private List<GridItem> _gridItems;
    private Grid _grid;

    public GridInventory() {
        _gridItems = new List<GridItem>();
        _grid = new Grid(13, 10);
    }

    public void AddItem(GridItem gridItem) {
        _gridItems.Add(gridItem);
        var index = _gridItems.Count - 1;
        _grid.Add(index, 0, 0, 3, 3);
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
}