using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitDisplay : MonoBehaviour
{
    private SpriteRenderer sr;
    private Unit unitData;
    public bool isPlayer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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
    }

    private void FitToTile()
    {
        if (sr.sprite == null) return;

        // 🔹 Get BoardGenerator reference
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

        // 🔹 Get sprite world size
        Vector2 spriteSize = sr.sprite.bounds.size;

        // 🔹 Scale exactly to tile size (fill fully)
        float scaleX = tileWidth / spriteSize.x;
        float scaleY = tileHeight / spriteSize.y;

        transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }
}
