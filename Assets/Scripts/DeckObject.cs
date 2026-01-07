using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckObject : MonoBehaviour, IPointerDownHandler
{
    

    public Deck deck;
    

    [Header("Indicators (show when empty)")]
    public GameObject[] indicators = new GameObject[5];

    [Header("Unit objects (with SpriteRenderers)")]
    public GameObject[] unitObjects = new GameObject[5];

    [Header("Deck Name UI (optional TMP or Text)")]
    public TMP_InputField name;
    public Button rename;
    public Button save;
    public Button trash;
    public TMP_Text Cost;

    public Gameplaysetting gameplaysetting;

    private void Start()
    {
    }
    void Update()
    {
        if (deck == null)
            return;

        UpdateDeckUI();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gameplaysetting.buildingdeck = deck.deckid;
        SceneManager.LoadScene("Deckbuilder", LoadSceneMode.Single);
    }

    void UpdateDeckUI()
    {
        // Update deck name text
        if (name != null)
            name.text = deck.name;

        IReadOnlyList<Piecer> pies = deck.Piecers;
        int count = 0;
        if(pies.Count > 0)
        {
            for (int i = 0; i < 5; i++)
            {
                count += pies[i].LV;
                bool hasPiecer = (i < pies.Count && pies[i] != null);

                // -------------------------
                // INDICATOR
                // -------------------------
                if (indicators[i] != null)
                {
                    CanvasGroup cg = indicators[i].GetComponent<CanvasGroup>();
                    if (cg == null)
                        cg = indicators[i].AddComponent<CanvasGroup>();

                    cg.alpha = hasPiecer ? 0f : 1f;   // visible only when empty
                }

                // -------------------------
                // UNIT SPRITE (SpriteRenderer)
                // -------------------------
                if (unitObjects[i] != null)
                {
                    SpriteRenderer sr = unitObjects[i].GetComponent<SpriteRenderer>();
                    if (sr == null)
                        sr = unitObjects[i].AddComponent<SpriteRenderer>();

                    if (hasPiecer)
                    {
                        Piecer p = pies[i];
                        sr.sprite = p.Skin[0];
                        sr.color = Color.white; // Visible
                    }
                    else
                    {
                        sr.sprite = null;
                        sr.color = new Color(1, 1, 1, 0); // Invisible
                    }
                }
            }
            Cost.text = $"{count}/15";
        }
        
    }
}
