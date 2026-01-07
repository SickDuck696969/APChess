using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class capsem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int capid;
    public Piecer piecer;
    public Mat mat;
    public GameObject capcase;
    public SpriteRenderer spr;
    public SpriteData spriteData;
    public int a;

    private Camera cam;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    public Transform originalParent;

    public Transform dragParent;
    public bool isdragged = false;

    public Image spritedisplay;
    public TMP_Text iddisplay;
    public TMP_Text namedisplay;
    public TMP_Text altdisplay;

    public TMP_Text movename;
    public TMP_Text movedescript;
    public Image tagger;

    public Image movetype;
    public Image movenature;

    public Sprite Passive;
    public Sprite Active;
    public Sprite Physical;
    public Sprite Meta;

    private LayoutElement layoutElement;

    public AudioSource audio;

    public string deckname;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // ADD IMAGE BEFORE ANY UI EVENTS CAN FIRE
        Image img = GetComponent<Image>();
        if (img == null)
        {
            img = gameObject.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.01f); // tiny alpha, clickable
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();
    }

    private void Start()
    {
        cam = Camera.main;
        dragParent = GameObject.FindGameObjectWithTag("Player").transform;
        // sprite renderer on the capcase object
        if(capcase != null)
        {
            spr = capcase.GetComponent<SpriteRenderer>();
        }
        display();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isdragged = true;
        Debug.Log("Begin Drag WORKS");

        originalParent = transform.parent;

        layoutElement.ignoreLayout = true;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(dragParent, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        isdragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isdragged = false;
        int count = 0;
        transform.SetParent(originalParent, true);

        layoutElement.ignoreLayout = false;
        canvasGroup.blocksRaycasts = true;
    }

    void OnMouseEnter()
    {
        if (piecer != null)
        {
            if (Tooltip.Instance != null) Tooltip.Instance.Show(piecer.name);
        }
        if (mat != null)
        {
            if (Tooltip.Instance != null) Tooltip.Instance.Show(mat.name);
        }
    }
    public void display()
    {
        foreach (visuals a in spriteData.spdt)
        {
            if (a.id == piecer.ID && a.variant == piecer.alt)
            {
                altdisplay.text = a.variant;
                altdisplay.color = HexToColor(a.hexcolor);
                namedisplay.text = piecer.name;
                iddisplay.text = "#" + piecer.ID.ToString();
                spritedisplay.sprite = a.sprites[0];
                movename.text = piecer.effect.name;
                movedescript.text = piecer.effect.desc;
                if(a.sprites.Length > 1)
                {
                    if (a.sprites[1] != null)
                    {
                        tagger.enabled = true;
                        tagger.sprite = a.sprites[1];
                    }
                }
                else
                {
                    tagger.sprite = null;
                    tagger.enabled = false;
                }
                movetype.transform.parent.GetComponent<Image>().color = HexToColor(a.hexcolor);
                movename.transform.parent.parent.parent.GetComponent<Image>().color = HexToColor(a.hexcolor);
                namedisplay.transform.Find("level").GetComponent<TMP_Text>().text = $"LvL {piecer.LV}";
            }
        }
        if (piecer.effect.type == "Passive") movetype.sprite = Passive;
        else if (piecer.effect.type == "Active") movetype.sprite = Active;
        else movetype.sprite = null;
        if (piecer.effect.nature == "meta") movenature.sprite = Meta;
        else if (piecer.effect.nature == "physical") movenature.sprite = Physical;
        else movenature.sprite = null;
    }

    void Update()
    {
        if(piecer != null)
        {
            foreach (visuals a in spriteData.spdt)
            {
                if (a.id == piecer.ID && a.variant == piecer.alt)
                {
                    transform.Find("Layer 2").GetComponent<Image>().color = HexToColor(a.hexcolor);
                }
            }
        }
        else if (mat != null)
        {
            GetComponent<Image>().sprite = mat.sprite;
        }
        if (!isdragged)
        {
            transform.localPosition = Vector3.zero;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPos =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit =
                Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {

            }
        }
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
}
