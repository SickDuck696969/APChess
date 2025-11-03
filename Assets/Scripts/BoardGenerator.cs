using UnityEngine;

[ExecuteAlways]
public class BoardGenerator : MonoBehaviour
{
    [Header("Board Settings")]
    public int columns = 5;
    public int rows = 8;
    public GameObject tilePrefab;
    public Vector2 tileSize = Vector2.one;

    [Header("Tile Visuals")]
    [Range(0f, 1f)] public float tileAlpha = 0.4f;
    public Color colorA = new Color(0.05f, 0.05f, 0.05f, 1f); // Dark gray
    public Color colorB = new Color(0.2f, 0.2f, 0.2f, 1f);    // Medium dark gray

    [Header("Outlines")]
    public bool showTileOutline = true;
    public bool showBoardOutline = true;
    public float outlineThickness = 0.05f;
    public Color outlineColor = Color.black;

    [Header("Section Backgrounds")]
    public Sprite playerBackground;
    public Sprite opponentBackground;
    [Range(0f, 1f)] public float backgroundOpacity = 1f;
    public Vector2 playerBgWorldPos = new Vector2(-2.5f, 0f);
    public Vector2 opponentBgWorldPos = new Vector2(-2.5f, 4f);

    [Header("Scene Background (Optional)")]
    public Sprite sceneBackground;
    [Range(0f, 1f)] public float sceneBgOpacity = 1f;
    public Vector2 sceneBgSize = new Vector2(20f, 12f);

    private GameObject[,] tiles;

    void Start()
    {
        if (Application.isPlaying)
            GenerateBoard();
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            CleanupBoard();
#endif
    }

    void OnApplicationQuit() => CleanupBoard();

    private void CleanupBoard()
    {
#if UNITY_EDITOR
        foreach (Transform child in transform)
            DestroyImmediate(child.gameObject);
#else
        foreach (Transform child in transform)
            Destroy(child.gameObject);
#endif
    }

    public void GenerateBoard()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab not assigned!");
            return;
        }

        CleanupBoard();
transform.position = new Vector3(transform.position.x, transform.position.y, 0f); // ✅ keep board at z=0

        float boardWidth = columns * tileSize.x;
        float boardHeight = rows * tileSize.y;
        Vector2 bottomLeft = new Vector2(-boardWidth / 2f, -boardHeight / 2f);

        // 🔹 Scene background (behind everything)
        if (sceneBackground != null)
            CreateBackground(sceneBackground, new Vector3(0, 0, 10f), sceneBgSize, sceneBgOpacity, -100);

        // 🔹 Section backgrounds (above scene, behind board)
        CreateBackground(playerBackground, new Vector3(playerBgWorldPos.x, playerBgWorldPos.y, 5f),
            new Vector2(boardWidth, boardHeight / 2f), backgroundOpacity, -50);

        CreateBackground(opponentBackground, new Vector3(opponentBgWorldPos.x, opponentBgWorldPos.y, 5f),
            new Vector2(boardWidth, boardHeight / 2f), backgroundOpacity, -50);

        // 🔹 Create tiles (main layer)
        tiles = new GameObject[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 pos = new Vector3(
                    bottomLeft.x + (x + 0.5f) * tileSize.x,
                    bottomLeft.y + (y + 0.5f) * tileSize.y,
                    0f // ✅ ensure tiles spawn at Z = 0
                );

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
tile.name = $"Tile_{x}_{y}";
tile.transform.localScale = tileSize;

// 🔹 Force absolute world and local Z = 0
tile.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, 0f);
tile.transform.localPosition = new Vector3(tile.transform.localPosition.x, tile.transform.localPosition.y, 0f);

                var sr = tile.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    bool useA = ((x + y) % 2 == 0);
                    Color c = useA ? colorA : colorB;
                    c.a = tileAlpha;

                    sr.material = new Material(Shader.Find("Sprites/Default"));
                    sr.color = c;
                    sr.sortingOrder = 0;
                }

                // Add tile outline
                if (showTileOutline)
                    AddOutline(tile, outlineThickness, outlineColor, 1);

                tiles[x, y] = tile;
            }
        }

        // 🔹 Add board outline (top layer)
        if (showBoardOutline)
        {
            GameObject boardOutline = new GameObject("Board_Outline");
            boardOutline.transform.parent = transform;
            boardOutline.transform.position = new Vector3(0, 0, 0f);

            LineRenderer lr = boardOutline.AddComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.widthMultiplier = outlineThickness;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = outlineColor;
            lr.endColor = outlineColor;
            lr.sortingOrder = 5;

            Vector3[] corners = new Vector3[]
            {
                new Vector3(bottomLeft.x, bottomLeft.y, 0),
                new Vector3(bottomLeft.x + boardWidth, bottomLeft.y, 0),
                new Vector3(bottomLeft.x + boardWidth, bottomLeft.y + boardHeight, 0),
                new Vector3(bottomLeft.x, bottomLeft.y + boardHeight, 0)
            };
            lr.positionCount = 4;
            lr.SetPositions(corners);
        }

        // 🔹 Enforce all tile Z = 0 (prevents them from overlapping units)
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Tile_"))
            {
                Vector3 p = child.position;
                p.z = 0f;
                child.position = p;
            }
        }
    }

    private void AddOutline(GameObject tile, float thickness, Color color, int sortingOrder)
    {
        GameObject outline = new GameObject(tile.name + "_Outline");
        outline.transform.parent = tile.transform;
        outline.transform.localPosition = Vector3.zero;

        LineRenderer lr = outline.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.widthMultiplier = thickness;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
        lr.sortingOrder = sortingOrder;

        Vector3 half = tile.transform.localScale / 2f;
        Vector3[] corners = new Vector3[]
        {
            new Vector3(-half.x, -half.y, 0),
            new Vector3(half.x, -half.y, 0),
            new Vector3(half.x, half.y, 0),
            new Vector3(-half.x, half.y, 0)
        };
        lr.positionCount = 4;
        lr.SetPositions(corners);
    }

    private void CreateBackground(Sprite sprite, Vector3 worldCenter, Vector2 targetSize, float opacity, int sortingOrder)
    {
        if (sprite == null) return;

        GameObject bg = new GameObject(sprite.name + "_BG");
        bg.transform.position = worldCenter;

        var sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 1f, 1f, opacity);
        sr.sortingOrder = sortingOrder;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = targetSize;
        sr.material = new Material(Shader.Find("Sprites/Default"));
    }
    
}
