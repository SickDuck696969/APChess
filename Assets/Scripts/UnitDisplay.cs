using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class UnitDisplay : MonoBehaviour
{
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private Unit unitData;
    public bool isPlayer;
    [Header("Position")]
public Vector2Int gridPos; // current tile position

[Header("Turn State")]
public bool hasMoved = false;

    [Header("Collider Settings (world units)")]
    // Desired collider size in WORLD units (eg. 1 tile = 1 unit)
    public Vector2 fixedColliderWorldSize = new Vector2(1f, 1f);

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        // ensure collider exists and is initialized
        if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
    }

    public void SetData(Unit data, bool isPlayerSide)
    {
        unitData = data;
        isPlayer = isPlayerSide;

        if (sr != null && data.unitSprite != null)
        {
            sr.sprite = data.unitSprite;
            sr.sortingOrder = 5;
            FitToTile();
        }

        // always enforce collider world size after scaling
        ApplyFixedColliderWorldSize();
    }

    private void FitToTile()
    {
        if (sr == null || sr.sprite == null) return;

#if UNITY_2023_1_OR_NEWER
        var board = Object.FindFirstObjectByType<BoardGenerator>();
#else
        var board = Object.FindObjectOfType<BoardGenerator>();
#endif
        float tileWidth = 1f, tileHeight = 1f;
        if (board != null)
        {
            tileWidth = board.tileSize.x;
            tileHeight = board.tileSize.y;
        }

        Vector2 spriteSize = sr.sprite.bounds.size;

        // scale sprite so sprite bounds fill the tile (visual)
        float scaleX = tileWidth / spriteSize.x;
        float scaleY = tileHeight / spriteSize.y;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private void ApplyFixedColliderWorldSize()
    {
        if (col == null) col = GetComponent<BoxCollider2D>();
        if (col == null) return;

        // Use lossyScale because localScale can be affected by parents
        Vector3 lossy = transform.lossyScale;
        // protect from zero scale
        float sx = Mathf.Approximately(lossy.x, 0f) ? 1f : lossy.x;
        float sy = Mathf.Approximately(lossy.y, 0f) ? 1f : lossy.y;

        // compute the required local-space size so that:
        // localSize * lossyScale = desired world size
        Vector2 localSize = new Vector2(fixedColliderWorldSize.x / sx, fixedColliderWorldSize.y / sy);

        col.size = localSize;
        col.offset = Vector2.zero;
        col.isTrigger = false;
    }

#if UNITY_EDITOR
    // keep collider correct in editor when you change properties
    void OnValidate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<BoxCollider2D>();
        ApplyFixedColliderWorldSize();
    }
#endif
}
