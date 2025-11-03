using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector] public UnitDisplay occupyingUnit;

    public bool IsOccupied => occupyingUnit != null;
}
