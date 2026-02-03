using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.Rendering.DebugUI;
using static UnityEngine.Rendering.DebugUI.Table;

public class deckbuilder : MonoBehaviour, IDropHandler
{
    public int deckid;
    public GameObject[] unitObjects = new GameObject[5];
    public GameObject[] TempDeck = new GameObject[5];
    public GameObject matplate;
    public List<GameObject> allthecapsem = new List<GameObject>();
    public Gameplaysetting gameplaysetting;
    public Player player;
    public Transform array;
    public Transform parent;
    public piercerdatabase piercerdatabase;
    public SpriteData spritedata;

    public GameObject Row;
    public GameObject Capsem;
    public GameObject MatPlate;

    public SpriteRenderer spritedisplay;
    public TMP_Text iddisplay;
    public TMP_Text namedisplay;
    public TMP_Text altdisplay;

    public TMP_Text movename;
    public TMP_Text movedescript;
    public Image tagger;

    public List<InventoryDataDB> caps = new List<InventoryDataDB>();
    public List<InventoryDataDB> mats = new List<InventoryDataDB>();

    public TMP_Dropdown type;
    public TMP_Dropdown level;
    public TMP_InputField search;

    public Image movetype;
    public Image movenature;

    public Sprite Passive;
    public Sprite Active;
    public Sprite Physical;
    public Sprite Meta;

    public float rowHeight = 150f;
    public float rowSpacing = 20f;

    private int totalRows = 0;

    public InventoryDatabase inventorydb;
    public CapsuleDatabase capdb;
    public DeckDatabase decks;

    public GameObject BattalliaArray;
    public DeckDataDB localtempdeck;

    public TMP_InputField deckname;
    public string localdeckname;
    public TMP_Text deckiddisplay;

    public bool maindeck = false;
    public UnityEngine.UI.Button maindeckbutton;
    void Start()
    {
        deckid = gameplaysetting.buildingdeck;
        inventorydb = new InventoryDatabase();
        capdb = new CapsuleDatabase();
        decks = new DeckDatabase();
        localtempdeck = decks.GetDeck(deckid);
        localdeckname = localtempdeck.name;
        type.value = 0;
        level.value = 0;
        listitems(type.options[1].text, 0, string.Empty);
        listitems(type.options[0].text, 0, string.Empty);
        level.onValueChanged.AddListener(changelevel);
        type.onValueChanged.AddListener(changetype);
        search.onValueChanged.AddListener(OnTextChanged);
        deckname.onDeselect.AddListener((_) =>
        {
            deckname.text = localdeckname;
        });
        deckname.text = localdeckname;
        deckiddisplay.text = $"#{deckid}";
        deckname.onSubmit.AddListener((_) => 
        {
            localdeckname = deckname.text;
        });
        maindeck = localtempdeck.maindeck;
        if (maindeck == true)
        {
            maindeckbutton.GetComponent<Image>().color = HexToColor("FFE100");
        }
        else
        {
            maindeckbutton.GetComponent<Image>().color = HexToColor("4C4C4C");
        }
        maindeckbutton.onClick.AddListener(() =>
        {
            if (maindeck == true)
            {
                if (EventSystem.current.currentSelectedGameObject == maindeckbutton.gameObject)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
            maindeck = maindeck == true ? false : true;
            if (maindeck == true)
            {
                maindeckbutton.GetComponent<Image>().color = HexToColor("FFE100");
            }
            else
            {
                maindeckbutton.GetComponent<Image>().color = HexToColor("4C4C4C");
            }
        });
    }

    public UnityEngine.Color HexToColor(string hex)
    {
        UnityEngine.Color col;
        string hexColor = "#" + hex;
        bool ok = UnityEngine.ColorUtility.TryParseHtmlString(hexColor, out col);

        if (!ok)
            Debug.LogError("Invalid hex string: " + hex);

        return col;
    }

    public void changelevel(int index)
    {
        listitems(type.options[type.value].text, index, search.text);
    }

    public void changetype(int index)
    {
        listitems(type.options[index].text, level.value, search.text);
    }

    public void OnTextChanged(string text)
    {
        listitems(type.options[type.value].text, level.value, search.text);
    }

    public void listitems(string type, int index, string search)
    {
        Debug.Log(type);
        Debug.Log(index);
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("row"))
        {
            DestroyImmediate(o);
        }
        int count = 0;
        totalRows = 0;
        caps.Clear();
        mats.Clear();
        GameObject currentrow = null;
        array = GameObject.FindGameObjectWithTag("Player").transform;
        foreach (InventoryDataDB a in inventorydb.GetPlayerInventory(player.data.user_id).ToList())
        {
            Debug.Log(a.capsuleid);
            CapsuleDataDB cap = capdb.GetCapsule(a.capsuleid);
            if (cap != null)
            {
                Debug.Log(cap.variant);
                if (cap.type == "piercer")
                {
                    caps.Add(a);
                }
                else if (capdb.GetCapsule(a.capsuleid).type == "mat")
                {
                    mats.Add(a);
                }
            }
        }
        if (type == "piercer") 
        {
            foreach (GameObject o in GameObject.FindGameObjectsWithTag("antag"))
            {
                DestroyImmediate(o);
            }
            foreach (InventoryDataDB acap in caps)
            {
                if (currentrow == null)
                {
                    currentrow = Instantiate(Row, transform);
                    totalRows++;
                }
                CapsuleDataDB a = capdb.GetCapsule(acap.capsuleid);
                foreach (Piecer b in piercerdatabase.roster)
                {
                    bool through = false;
                    if (index == 0)
                    {
                        if (a.itemid == b.ID)
                        {
                            through = true;
                            Transform where = currentrow.transform.Find("row (1)");
                            if (acap.inventoryid == localtempdeck.slot1)
                            {
                                where = BattalliaArray.transform.Find("Array/slot");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot2)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (1)");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot3)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (2)");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot4)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (3)");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot5)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (4)");
                                through = true;
                            }
                            if (search != string.Empty)
                            {
                                through = false;
                                if(b.name.Contains(search) || a.variant.Contains(search))
                                {
                                    through = true;
                                }
                            }
                            if (through)
                            {
                                bool runnit = true;
                                GameObject cap = Instantiate(Capsem, where);
                                cap.GetComponent<capsem>().capid = acap.inventoryid;
                                cap.GetComponent<capsem>().spritedisplay = spritedisplay; cap.GetComponent<capsem>().namedisplay = namedisplay; cap.GetComponent<capsem>().iddisplay = iddisplay; cap.GetComponent<capsem>().altdisplay = altdisplay;
                                cap.GetComponent<capsem>().movetype = movetype; cap.GetComponent<capsem>().movenature = movenature; cap.GetComponent<capsem>().Passive = Passive; cap.GetComponent<capsem>().Active = Active; cap.GetComponent<capsem>().Meta = Meta; cap.GetComponent<capsem>().Physical = Physical;
                                cap.GetComponent<capsem>().movedescript = movedescript; cap.GetComponent<capsem>().movename = movename; cap.GetComponent<capsem>().tagger = tagger;
                                cap.GetComponent<capsem>().piecer = b.Clone();
                                cap.GetComponent<capsem>().piecer.alt = a.variant;
                                if (where != currentrow.transform.Find("row (1)"))
                                {
                                    where.GetComponent<slotting>().it = cap;
                                }
                                allthecapsem.Add(cap);
                                count++;

                                if (count == 5)
                                {
                                    currentrow = null;
                                    count = 0;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (a.itemid == b.ID && b.LV == index)
                        {
                            through = true;
                            Transform where = currentrow.transform.Find("row (1)");
                            if (acap.inventoryid == localtempdeck.slot1)
                            {
                                where = BattalliaArray.transform.Find("Array/slot");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot2)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (1)");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot3)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (2)");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot4)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (3)");
                                through = true;
                            }
                            else if (acap.inventoryid == localtempdeck.slot5)
                            {
                                where = BattalliaArray.transform.Find("Array/slot (4)");
                                through = true;
                            }
                            if (search != string.Empty)
                            {
                                through = false;
                                if (b.name.Contains(search) || a.variant.Contains(search))
                                {
                                    through = true;
                                }
                            }
                            if (search != string.Empty)
                            {
                                through = false;
                                if (b.name.Contains(search) || a.variant.Contains(search))
                                {
                                    through = true;
                                }
                            }
                            if (through)
                            {
                                GameObject cap = Instantiate(Capsem, where);
                                cap.GetComponent<capsem>().capid = acap.inventoryid;
                                cap.GetComponent<capsem>().spritedisplay = spritedisplay; cap.GetComponent<capsem>().namedisplay = namedisplay; cap.GetComponent<capsem>().iddisplay = iddisplay; cap.GetComponent<capsem>().altdisplay = altdisplay;
                                cap.GetComponent<capsem>().movetype = movetype; cap.GetComponent<capsem>().movenature = movenature; cap.GetComponent<capsem>().Passive = Passive; cap.GetComponent<capsem>().Active = Active; cap.GetComponent<capsem>().Meta = Meta; cap.GetComponent<capsem>().Physical = Physical;
                                cap.GetComponent<capsem>().movedescript = movedescript; cap.GetComponent<capsem>().movename = movename; cap.GetComponent<capsem>().tagger = tagger;
                                cap.GetComponent<capsem>().piecer = b.Clone();
                                cap.GetComponent<capsem>().piecer.alt = a.variant;
                                if (where != currentrow.transform.Find("row (1)"))
                                {
                                    where.GetComponent<slotting>().it = cap;
                                }
                                allthecapsem.Add(cap);
                                count++;

                                if (count == 5)
                                {
                                    currentrow = null;
                                    count = 0;
                                }
                            }
                        }
                    }
                }
            }
            localtempdeck.slot1 = TempDeck[0] != null ? TempDeck[0].GetComponent<capsem>().capid : 0;
            localtempdeck.slot2 = TempDeck[1] != null ? TempDeck[1].GetComponent<capsem>().capid : 0;
            localtempdeck.slot3 = TempDeck[2] != null ? TempDeck[2].GetComponent<capsem>().capid : 0;
            localtempdeck.slot4 = TempDeck[3] != null ? TempDeck[3].GetComponent<capsem>().capid : 0;
            localtempdeck.slot5 = TempDeck[4] != null ? TempDeck[4].GetComponent<capsem>().capid : 0;
        }
        else if (type == "mat")
        {
            foreach (GameObject o in GameObject.FindGameObjectsWithTag("protag"))
            {
                DestroyImmediate(o);
            }
            foreach (InventoryDataDB acap in mats)
            {
                CapsuleDataDB a = capdb.GetCapsule(acap.capsuleid);
                bool through = false;
                if (currentrow == null)
                {
                    currentrow = Instantiate(Row, transform);
                    totalRows++;
                    if (transform.childCount <= 1)
                    {
                        foreach (Mat b in spritedata.mats)
                        {
                            if (b.id == 1612)
                            {
                                through = true;
                                Transform where = currentrow.transform.Find("row (1)");
                                if (localtempdeck.mat_id == 21000932)
                                {
                                    where = BattalliaArray.transform.Find("Array/basematplate");
                                    through = true;
                                }
                                if (search != string.Empty)
                                {
                                    through = false;
                                    if (b.name.Contains(search))
                                    {
                                        through = true;
                                    }
                                }
                                if (through) 
                                {
                                    GameObject plate = Instantiate(MatPlate, currentrow.transform.Find("row (1)"));
                                    plate.GetComponent<capsem>().capid = 21000932;
                                    Mat newmat = new Mat();
                                    newmat = b;
                                    plate.GetComponent<capsem>().mat = newmat;
                                    if (where != currentrow.transform.Find("row (1)"))
                                    {
                                        plate.transform.SetParent(where, true);
                                        where.GetComponent<slotting>().it = plate;
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (Mat b in spritedata.mats)
                {
                    if (a.itemid == b.id)
                    {
                        through = true;
                        Transform where = currentrow.transform.Find("row (1)");
                        if (localtempdeck.mat_id == acap.inventoryid)
                        {
                            where = BattalliaArray.transform.Find("Array/basematplate");
                            through = true;
                        }
                        if (search != string.Empty)
                        {
                            through = false;
                            if (b.name.Contains(search))
                            {
                                through = true;
                            }
                        }
                        if (through)
                        {
                            GameObject plate = Instantiate(MatPlate, currentrow.transform.Find("row (1)"));
                            plate.GetComponent<capsem>().capid = acap.inventoryid;
                            Mat newmat = new Mat();
                            newmat = b;
                            plate.GetComponent<capsem>().mat = newmat;
                            if (where != currentrow.transform.Find("row (1)"))
                            {
                                plate.transform.SetParent(where, true);
                                where.GetComponent<slotting>().it = plate;
                            }
                        }
                    }
                }
            }
        }
        ResizePanelToFitRows();
    }

    public void save()
    {
        if(matplate != null && matplate.GetComponent<capsem>().mat != null)
        {
            Debug.Log($"Mat: {matplate.GetComponent<capsem>().mat.name}");
            decks.UpdateDeckSlot(player.data.user_id, deckid, 0, matplate.GetComponent<capsem>().capid, deckname.text, maindeck);
        }
        else
        {
            Debug.Log($"Mat: --");
        }
        foreach (GameObject a in TempDeck)
        {
            if (a != null && a.GetComponent<capsem>().piecer != null)
            {
                Debug.Log($"Deck[{Array.IndexOf(TempDeck, a) + 1}]: {a.GetComponent<capsem>().piecer.alt}");
                decks.UpdateDeckSlot(player.data.user_id, deckid, Array.IndexOf(TempDeck, a) + 1, a.GetComponent<capsem>().capid, deckname.text, maindeck);
            }
            else
            {
                Debug.Log($"Deck[{Array.IndexOf(TempDeck, a) + 1}]: --");
            }
        }
        
        localtempdeck = decks.GetDeck(deckid);
        localdeckname = localtempdeck.name;
    }

    public void OnDrop(PointerEventData eventData)
    {
        capsem c = eventData.pointerDrag.GetComponent<capsem>();
        if (c == null) return;

        // FIX: Remove from TempDeck safely
        int dex = Array.IndexOf(TempDeck, c.gameObject);
        if (dex >= 0)
            TempDeck[dex] = null;

        var rows = GetComponentsInChildren<Transform>()
                   .Where(t => t.CompareTag("row"))
                   .ToList();

        if (rows.Count == 0)
        {
            Debug.LogWarning("DropPoint has no row children!");
            return;
        }
        else
        {
            foreach (Transform row in rows)
            {
                int count = 0;
                foreach (Transform child in row.transform.Find("row (1)"))
                {
                    if (!child.CompareTag("row"))
                        count++;
                }

                if (count < 5)
                {
                    c.transform.SetParent(row.transform.Find("row (1)"), true);
                    c.originalParent = c.transform.parent;
                    c.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    TempDeck[c.a] = null;

                    Debug.Log($"Dropped {c.name} in row: {row.name}");
                    return;
                }
            }

            Debug.Log("All rows are full, drop rejected.");
        }
    }

    void Update()
    {
        for (int i = 0; i < TempDeck.Length; i++)
        {
            if (TempDeck[i] == null || unitObjects[i] == null)
                continue;
            if (TempDeck[i].GetComponent<capsem>() != null && TempDeck[i].GetComponent<capsem>().isdragged)
            {
                continue;
            }
            else
            {
                TempDeck[i].transform.position = unitObjects[i].transform.position;
                TempDeck[i].transform.rotation = unitObjects[i].transform.rotation;
                TempDeck[i].transform.localScale = unitObjects[i].transform.localScale;
            }
        }
        SortRows();
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
        if(size.y < 0f) size.y = 0f;
        rt.sizeDelta = size;
    }

    public GameObject[] getas()
    {
        return TempDeck;
    }

    public GameObject getmat()
    {
        return matplate;
    }

    public void SortRows()
    {
        var rows = GetComponentsInChildren<Transform>()
                   .Where(t => t.CompareTag("row"))
                   .ToList();

        if (rows.Count == 0)
        {
            Debug.LogWarning("DropPoint has no row children!");
            return;
        }
        else
        {
            foreach (Transform row in rows)
            {
                if(row.transform.Find("row (1)") != null)
                {
                    Debug.Log("sad");
                    int count = 0;
                    foreach (Transform child in row.transform.Find("row (1)"))
                    {
                        if (!child.CompareTag("row"))
                            count++;
                    }
                    if (count < 5)
                    {
                        Debug.Log(count);
                        if (rows.IndexOf(row) + 1 <= rows.Count - 1)
                        {
                            Debug.Log($"{rows.IndexOf(row)}, {rows.Count}");
                            Debug.Log(rows[rows.IndexOf(row) + 1].transform.Find("row (1)"));
                            if(rows[rows.IndexOf(row) + 1].transform.Find("row (1)") != null){
                                Debug.Log("sad2");
                                if (rows[rows.IndexOf(row) + 1].transform.Find("row (1)").childCount > 0)
                                {
                                    Transform firstChild = rows[rows.IndexOf(row) + 1].transform.Find("row (1)").GetChild(0);
                                    if (firstChild.GetComponent<capsem>().piecer != null) Debug.Log(firstChild.GetComponent<capsem>().piecer.name);
                                    if (firstChild.GetComponent<capsem>().mat != null) Debug.Log(firstChild.GetComponent<capsem>().mat.name);
                                    firstChild.SetParent(row.transform.Find("row (1)"), true);
                                }
                            }
                        }
                    }
                    else if (count > 5)
                    {
                        row.transform.Find("row (1)").GetChild(5).SetParent(rows[rows.IndexOf(row) + 1].transform.Find("row (1)"), true);
                    }
                }
                
            }
        }
    }
}
