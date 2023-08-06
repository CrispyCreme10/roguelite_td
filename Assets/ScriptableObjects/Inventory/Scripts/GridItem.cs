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
    [SerializeField] private Color bgColor;

    public string ItemName => itemName;
    public string ShortName => !string.IsNullOrEmpty(shortName) ? shortName : itemName;
    public Sprite Icon => icon;
    public int Width => width;
    public int Height => height;
    public float IconScale => iconScale;
    public float IconRotation => iconRotation;
    public Color BgColor => bgColor;
}