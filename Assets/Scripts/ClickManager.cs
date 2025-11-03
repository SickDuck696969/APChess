using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public SelectionManager selectionManager;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        // Convert screen mouse to ray
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 🔹 Get all colliders under the mouse (units + tiles)
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);

        if (hits.Length == 0)
        {
            selectionManager.Deselect();
            Debug.Log("Clicked empty area");
            return;
        }

        // 🔹 Sort hits by Z depth (closest to camera first)
        System.Array.Sort(hits, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        // 🔹 Check frontmost hit
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // Prefer unit first
            var unit = hit.collider.GetComponent<UnitDisplay>();
            if (unit != null)
            {
                selectionManager.SelectUnit(unit);
                Debug.Log("Clicked UNIT: " + unit.name);
                return;
            }

            // Then tile
            var tile = hit.collider.GetComponent<TileClickHandler>();
            if (tile != null)
            {
                selectionManager.Deselect();
                Debug.Log("Clicked TILE: " + tile.name);
                return;
            }
        }
    }
}



}
