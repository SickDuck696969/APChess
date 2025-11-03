using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject optionPanel; // assign in Inspector
    public GameObject optionPanelAvatar; // assign in Inspector
    private UnitDisplay selectedUnit;
    

    void Start()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void SelectUnit(UnitDisplay unit)
    {
        // 🔹 Always show the panel when clicking a unit
        if (optionPanel == null) return;

        // If clicking the same unit again, re-show (refresh) the panel
        selectedUnit = unit;

        optionPanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    public void Deselect()
    {
        selectedUnit = null;

        if (optionPanel != null)
            optionPanel.SetActive(false);
    }
}
