using UnityEngine;

[CreateAssetMenu(menuName = "Grid Item")]
public class GridItem : ScriptableObject {
    [SerializeField] private string itemName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private Color bgColor;

    public int Height => height;
    public int Width => width;
    public Sprite Icon => icon;
    public Color BgColor => bgColor;
    public string ItemName => itemName;
}