using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    public Vector2Int gridPos;
    public bool isOccupied = false;
    public SpriteRenderer sr;

    private Color baseColor;
    public Color highlightColor = Color.green;
    
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    public void Highlight()
    {
        sr.color = highlightColor;
    }

    public void Unhighlight()
    {
        sr.color = baseColor;
    }
}
