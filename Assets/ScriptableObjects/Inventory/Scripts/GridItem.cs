using UnityEngine;

[CreateAssetMenu(menuName = "Grid Item")]
public class GridItem : ScriptableObject {
    [SerializeField] private string itemName;
    [SerializeField] private string shortName; // possibly shorter item name that looks better in Inventory Grid Cells
    [SerializeField] private Sprite icon;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private float iconScale = 1f;
    [SerializeField] private float iconRotation = 0f; // initial rotation of icon
    [SerializeField] private int defaultStackSize = 1;
    [SerializeField] private bool isContainer = false;
    [SerializeField] private int containerRows = 0;
    [SerializeField] private int containerCols = 0;
    [SerializeField] private Color bgColor;
    [SerializeField] private Color statColor;

    public string ItemName => itemName;
    public string ShortName => !string.IsNullOrEmpty(shortName) ? shortName : itemName;
    public Sprite Icon => icon;
    public int Width => width;
    public int Height => height;
    public float IconScale => iconScale;
    public float IconRotation => iconRotation;
    public int DefaultStackSize => defaultStackSize;
    public bool IsContainer => isContainer;
    public int ContainerRows => containerRows;
    public int ContainerCols => containerCols;
    public Color BgColor => bgColor;
    public Color StatColor => statColor;
}

public class StackGridItem {
    public int Amount { get; private set; }
    public GridItem GridItem { get; }
    public bool IsRotated { get; private set; }
    public int ColIndex { get; private set; }
    public int RowIndex { get; private set; }
    public GridInventory GridInventory { get; }
    
    public bool IsMaxed => Amount == GridItem.DefaultStackSize;
    public int WidthAdjForRotation => !IsRotated ? GridItem.Width : GridItem.Height;
    public int HeightAdjForRotation => !IsRotated ? GridItem.Height : GridItem.Width;

    public StackGridItem(GridItem gridItem, int amount) {
        GridItem = gridItem;
        Amount = amount;
        if (GridItem.IsContainer) {
            GridInventory = new GridInventory(GridItem.ContainerRows, GridItem.ContainerCols);
        }
    }

    public void UpdateGridData(int rowIndex, int colIndex, bool isRotated) {
        RowIndex = rowIndex;
        ColIndex = colIndex;
        IsRotated = isRotated;
    }

    public int DecreaseAmount(int amt) {
        var newAmt = Amount - amt;
        Amount = newAmt <= 0 ? 0 : newAmt;
        return Amount;
    }

    public int IncreaseAmount(int amt) {
        if (!CanStack(amt)) return Amount;
        Amount += amt;
        return Amount;
    }

    public bool CanMerge(StackGridItem stackGridItem) {
        return IsSameItem(stackGridItem) && CanStack(stackGridItem.Amount);
    }

    private bool CanStack(int increaseAmt) {
        return Amount + increaseAmt <= GridItem.DefaultStackSize;
    }

    private bool IsSameItem(StackGridItem stackGridItem) {
        return GridItem.ItemName == stackGridItem.GridItem.ItemName;
    }
}