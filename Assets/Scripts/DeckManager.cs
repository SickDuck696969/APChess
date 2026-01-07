using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public GameObject DeckPrefab;
    public Player player;
    public DeckDatabase decks;
    public InventoryDatabase inventory;
    public CapsuleDatabase capsuleDatabase;
    public List<DeckDataDB> deckList = new List<DeckDataDB>();
    public SpriteData sprites;
    public piercerdatabase piercers;
    public float rowHeight = 150f;
    public float rowSpacing = 20f;

    private int totalRows = 0;

    void Start()
    {
        decks = new DeckDatabase();
        inventory = new InventoryDatabase();
        capsuleDatabase = new CapsuleDatabase();
        deckList = decks.GetPlayerDecks(player.data.user_id);
        PopulateDecks();
    }

    private void Update()
    {
        totalRows = player.Decks.Count;
        ResizePanelToFitRows();
    }

    public void adddeck()
    {
        decks.SaveDeck(player.data.user_id, "new deck", 0, 0, 0, 0, 0, 0, false);
        PopulateDecks();
    }

    public void PopulateDecks()
    {
        deckList = decks.GetPlayerDecks(player.data.user_id);
        player.Decks.Clear();
        foreach (Transform child in transform)
        {
            if (child.name != "AddDeck")
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate a DeckPrefab for each deck in the player's deck list
        foreach (var deck in deckList)
        {
            Deck tempdeck = new Deck();
            if(deck.mat_id == 21000932)
            {
                foreach (Mat mat in sprites.mats)
                {
                    if (1612 == mat.id)
                    {
                        tempdeck.mat = mat;
                        break;
                    }
                }
            }
            else
            {
                if (deck.mat_id != 0)
                {
                    InventoryDataDB itr = inventory.GetInventoryItem(deck.mat_id);
                    CapsuleDataDB capp = capsuleDatabase.GetCapsule(itr.capsuleid);
                    foreach (Mat mat in sprites.mats)
                    {
                        if (capp.itemid == mat.id)
                        {
                            tempdeck.mat = mat;
                            break;
                        }
                    }
                }
            }
            List<CapsuleDataDB> list = new List<CapsuleDataDB>();
            for(int i = 1; i <= 5; i++)
            {
                int who = 0;
                if(i == 1) { who = deck.slot1; }
                else if (i == 2) { who = deck.slot2; }
                else if (i == 3) { who = deck.slot3; }
                else if (i == 4) { who = deck.slot4; }
                else if (i == 5) { who = deck.slot5; }
                if(who != 0)
                {
                    InventoryDataDB item = inventory.GetInventoryItem(who);
                    CapsuleDataDB cap = capsuleDatabase.GetCapsule(item.capsuleid);
                    list.Add(cap);
                }
            }
            foreach (CapsuleDataDB cap in list)
            {
                foreach (Piecer a in piercers.roster)
                {
                    if(cap.itemid == a.ID)
                    {
                        Piecer temp = a.Clone();
                        foreach(visuals d in sprites.spdt)
                        {
                            if(a.ID == d.id && cap.variant == d.variant)
                            {
                                temp.Skin = d.sprites;
                            }
                        }
                        temp.alt = cap.variant;
                        tempdeck.AddPiecer(temp);
                        break;
                    }
                }
            }
            tempdeck.name = deck.name;
            tempdeck.deckid = deck.deck_id;
            tempdeck.maindeck = deck.maindeck;
            player.Decks.Add(tempdeck);
        }

        int foundyoun = 0;

        foreach (Deck deck in player.Decks)
        {
            if (deck.maindeck) foundyoun = player.Decks.IndexOf(deck);
        }

        for (int i = foundyoun; i < player.Decks.Count-1; i++)
        {
            Deck temp = player.Decks[i];
            player.Decks[i] = player.Decks[i + 1];
            player.Decks[i + 1] = temp;
        }

        foreach(Deck deck in player.Decks)
        {
            GameObject obj = Instantiate(DeckPrefab, transform);
            DeckObject d = obj.GetComponent<DeckObject>();
            d.deck = deck;
        }

        ResizePanelToFitRows();
    }
    void ResizePanelToFitRows()
    {
        if (totalRows <= 0)
            return;

        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null)
            return;

        // Total height = rows * rowHeight + gaps
        float totalHeight = totalRows * rowHeight + (totalRows - 1) * rowSpacing;

        Vector2 size = rt.sizeDelta;
        size.y = 0f;
        rt.sizeDelta = size;
        size.y = totalHeight - 300f;
        if (size.y < 0f) size.y = 0f;
        rt.sizeDelta = size;
    }
}


[RequireComponent(typeof(RectTransform))]
public class DeckListResizer : MonoBehaviour
{
    public float elementHeight = 120f; // height of DeckPrefab
    public float spacing = 10f;         // spacing between decks
    public int extraPadding = 20;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Resize(int elementCount)
    {
        float height = (elementHeight + spacing) * elementCount - spacing + extraPadding;
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(height, 0));
    }
}