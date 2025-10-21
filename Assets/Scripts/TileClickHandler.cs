using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class TileClickHandler : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color baseColor; // original tile color (A or B)
    private float brightnessBoost = 1.3f; // brighten highlight slightly
    private float saturationBoost = 1.2f; // make color richer

    public Color highlightColor = Color.yellow;
    public static TileClickHandler currentlySelected;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // Capture the board-assigned color (keeps alpha)
        baseColor = sr.color;
    }

    void OnMouseDown()
    {
        // Deselect previous tile
        if (currentlySelected != null && currentlySelected != this)
            currentlySelected.Deselect();

        // Toggle
        if (currentlySelected == this)
        {
            Deselect();
            currentlySelected = null;
        }
        else
        {
            Highlight();
            currentlySelected = this;
        }
    }

    void Highlight()
    {
        // Blend base color with yellow
        Color blended = Color.Lerp(baseColor, highlightColor, 0.7f);

        // Boost brightness and saturation for clarity
        blended = AdjustBrightness(blended, brightnessBoost);
        blended = AdjustSaturation(blended, saturationBoost);

        // Keep original transparency
        blended.a = baseColor.a;

        sr.color = blended;
    }

    void Deselect()
    {
        sr.color = baseColor;
    }

    // Helper: increase brightness
    Color AdjustBrightness(Color color, float factor)
    {
        return new Color(
            Mathf.Clamp01(color.r * factor),
            Mathf.Clamp01(color.g * factor),
            Mathf.Clamp01(color.b * factor),
            color.a
        );
    }

    // Helper: increase saturation (HSL-based tweak)
    Color AdjustSaturation(Color color, float factor)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * factor);
        return Color.HSVToRGB(h, s, v);
    }
}
