using UnityEngine;
using System.Collections.Generic;

public class FormationManager : MonoBehaviour
{
    [Header("References")]
    public BoardGenerator boardGenerator;
    public GameObject unitPrefab; // prefab with SpriteRenderer + UnitDisplay

    [Header("Unit Data")]
    public List<Unit> playerUnits;   // 5 normal + 1 avatar
    public List<Unit> opponentUnits; // 5 normal + 1 avatar

    private GameObject[,] spawnedUnits;

    public void GenerateFormation()
    {
        if (boardGenerator == null || boardGenerator.columns != 5 || boardGenerator.rows != 8)
        {
            Debug.LogError("BoardGenerator not found or wrong size (need 5x8).");
            return;
        }

        spawnedUnits = new GameObject[boardGenerator.columns, boardGenerator.rows];

        float boardWidth = boardGenerator.columns * boardGenerator.tileSize.x;
        float boardHeight = boardGenerator.rows * boardGenerator.tileSize.y;
        Vector2 bottomLeft = new Vector2(-boardWidth / 2f, -boardHeight / 2f);

        // === Player formation ===
        int playerRow = 1;          // second last row bottom side
        int avatarRow = 0;          // bottom-most
        SpawnRow(playerUnits, playerRow, avatarRow, bottomLeft, true);

        // === Opponent formation ===
        int oppRow = 6;             // second last row top side
        int oppAvatarRow = 7;       // top-most
        SpawnRow(opponentUnits, oppRow, oppAvatarRow, bottomLeft, false);
    }

    private void SpawnRow(List<Unit> list, int unitRow, int avatarRow, Vector2 bottomLeft, bool isPlayer)
    {
        float tileSizeX = boardGenerator.tileSize.x;
        float tileSizeY = boardGenerator.tileSize.y;

        // Spawn 5 normal units
        for (int x = 0; x < 5 && x < list.Count - 1; x++)
        {
            Vector3 pos = new Vector3(
                bottomLeft.x + (x + 0.5f) * tileSizeX,
                bottomLeft.y + (unitRow + 0.5f) * tileSizeY,
                -1f
            );

            SpawnUnit(list[x], pos, isPlayer);
        }

        // Spawn avatar (center)
        if (list.Count > 0)
        {
            int centerX = 2; // middle of 5 columns
            Vector3 pos = new Vector3(
                bottomLeft.x + (centerX + 0.5f) * tileSizeX,
                bottomLeft.y + (avatarRow + 0.5f) * tileSizeY,
                -1f
            );

            SpawnUnit(list[list.Count - 1], pos, isPlayer);
        }
    }

    private void SpawnUnit(Unit data, Vector3 pos, bool isPlayer)
    {
        if (unitPrefab == null || data == null) return;

        GameObject obj = Instantiate(unitPrefab, pos, Quaternion.identity);
        obj.name = data.unitName;

        var display = obj.GetComponent<UnitDisplay>();
        if (display != null)
        {
            display.SetData(data, isPlayer);
        }
    }
    void Start()
{
    GenerateFormation();
}

}
