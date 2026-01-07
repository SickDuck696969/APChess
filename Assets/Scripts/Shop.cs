using SQLite4Unity3d;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.Services.Matchmaker.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public Animator talkingAnimator;
    public Player player;
    public TextMeshProUGUI dialoguetext;
    public float typingSpeed = 0.04f;
    public List<CapsuleDataDB> cart = new List<CapsuleDataDB>();
    public GameObject recuitprefab;
    private Coroutine typingCoroutine;
    private bool isTyping;
    public AudioSource sfx;
    public InventoryDatabase InventoryDB;
    public StorageDatabase StorageDB;
    public CapsuleDatabase CapsuleDB;
    public piercerdatabase piercerdatabase;
    public SpriteData spriteData;
    public Button piecers;
    public Button decors;
    public Button avatars;
    public Button holditem;
    public List<CapsuleDataDB> instock = new List<CapsuleDataDB>();
    public GameObject merchprefab;
    public GameObject display;
    public Vector2 initsize;
    public GameObject describe;
    public GameObject regular;
    public string stsa;
    public string shopping = "";
    public List<string> welcome = new List<string>() {
        "MERRY CHRISTMAS!",
        "happy hollyday!",
        "buy some sh#t."
    };
    public List<string> piercers = new List<string>() {
        "all new.",
        "alright, run them pockets little buddy, i've got some good $#!# here.",
        "got something for me too? i can trade ya."
    };
    public List<string> decor = new List<string>() {
        "the finest here.",
        "feelin a lil fancy, are we?",
        "anything ya catch your eye, just grab it it's just money."
    };
    public List<string> shorton = new List<string>()
    {
        "sorry buddy, looks like you're short",
        "try again when you have the money?",
        "not for you, brokie"
    };
    public GameObject banner;
    void Start()
    {
        describe.SetActive(false);
        talkingAnimator.SetBool("isTyping", false);
        initsize = display.transform.GetComponent<RectTransform>().sizeDelta;
        InventoryDB = new InventoryDatabase();
        StorageDB = new StorageDatabase();
        CapsuleDB = new CapsuleDatabase();
        piecers.onClick.AddListener(() => 
        {
            StartDialogue(dialoguetext, "all new.");
            RefreshUI("piercers");
        });
        decors.onClick.AddListener(() =>
        {
            StartDialogue(dialoguetext, "feelin fancy are we?");
            RefreshUI("decors");
        });
        avatars.onClick.AddListener(() =>
        {
            StartDialogue(dialoguetext, "ez presets.");
            RefreshUI("avatars");
        });
        holditem.onClick.AddListener(() =>
        {
            StartDialogue(dialoguetext, "coming later");
            RefreshUI("holditems");
        });
        sfx.clip = Resources.Load<AudioClip>("sfx/Shop Door Bell");
        sfx.Play();
        welcomehollyday();
    }
    void demo()
    {
        CapsuleDB.DeleteAllCapsules();
        StorageDB.DeleteAllStorage();
        CapsuleDB.SaveCapsule(214, "piercer", "Chocold-Slinger", 85700, false);
        CapsuleDB.SaveCapsule(9, "piercer", "Shinobi Verd", 25700, false);
        CapsuleDB.SaveCapsule(64, "piercer", "Strawberry Sister", 100000, false);
        CapsuleDB.SaveCapsule(6779, "piercer", "Warm Anne", 2570000, false);
        CapsuleDB.SaveCapsule(214, "piercer", "Ruby Choco-Slinger", 55000, false);
        CapsuleDB.SaveCapsule(2002, "piercer", "DragRider", 100000000, true);
        CapsuleDB.SaveCapsule(2000, "piercer", "ToraWhite", 857000, false);
        CapsuleDB.SaveCapsule(777, "piercer", "Might Barrier", 85700, false);
        CapsuleDB.SaveCapsule(4510, "mat", "Grass", 85000, false);
        List<CapsuleDataDB> box = CapsuleDB.GetAllCapsules();
        foreach (CapsuleDataDB c in box)
        {
            StorageDB.SaveStorage(2, c.capid);
        }
        List<StorageDataDB> storage = StorageDB.GetStorage(2);
        instock.Clear();
        foreach (StorageDataDB c in storage)
        {
            instock.Add(CapsuleDB.GetCapsule(c.capsuleid));
        }
    }
    public void welcomehollyday()
    {
        string emotion = null;
        if (welcome.Count >= 3)
        {
            emotion = "happy";
        }
        else if (welcome.Count >= 2)
        {
            emotion = "look";
        }
        else
        {
            emotion = "idle";
        }
        talkingAnimator.SetBool(stsa, false);
        stsa = emotion;
        talkingAnimator.SetBool(stsa, true);
        if (welcome.Count > 0)
        {
            StartDialogue(dialoguetext, welcome[0]);
            welcome.RemoveAt(0);
        }
    }
    public void updatebox()
    {
        RectTransform boxrect = display.GetComponent<RectTransform>();
        float wantedheight = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(boxrect);
        foreach (Transform child in display.transform)
        {
            if (child != null)
            {
                Debug.Log(child.name);
                wantedheight += child.GetComponent<RectTransform>().sizeDelta.y;
                child.GetComponent<RectTransform>().localPosition = new Vector3(child.GetComponent<RectTransform>().localPosition.x, child.GetComponent<RectTransform>().localPosition.y, -0.7f);
            }
        }
        boxrect.sizeDelta = new Vector2(boxrect.sizeDelta.x, initsize.y + wantedheight);
    }
    public void RefreshUIReceit()
    {
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("infocard"))
        {
            Destroy(o);
        }
        int sum = 0;
        foreach (var item in cart)
        {

            GameObject obj = Instantiate(recuitprefab, GameObject.FindGameObjectWithTag("mine").transform);
            obj.transform.Find("id").GetComponent<TMP_Text>().text = item.capid.ToString();
            obj.transform.Find("category").GetComponent<TMP_Text>().text = item.type.ToString();
            if(item.type == "piercer")
            {
                foreach (Piecer a in piercerdatabase.roster)
                {
                    if (a.ID == item.itemid)
                    {
                        obj.transform.Find("name").GetComponent<TMP_Text>().text = a.name;
                    }
                }
            }
            else
            {
                obj.transform.Find("name").GetComponent<TMP_Text>().text = item.variant;
            }
            obj.transform.Find("price").GetComponent<TMP_Text>().text = item.fragments.ToString();
            obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                cart.Remove(item);
                RefreshUIReceit();
            });
            sum += item.fragments;
        }
        GameObject.FindGameObjectWithTag("antag").GetComponent<TMP_Text>().text = sum.ToString();
    }

    public void RefreshUI(string value)
    {
        shopping = value;
        List<StorageDataDB> storage = StorageDB.GetStorage(2);
        instock.Clear();
        foreach (StorageDataDB c in storage)
        {
            instock.Add(CapsuleDB.GetCapsule(c.capsuleid));
        }
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("row"))
        {
            Destroy(o);
        }
        foreach (var item in instock)
        {
            if(shopping == "piercers")
            {
                Debug.Log(item.type);
                if (item.type == "piercer")
                {
                    GameObject obj = Instantiate(merchprefab, display.transform);
                    obj.transform.Find("RawImage/RawImage/Text (TMP) (1)").GetComponent<TMP_Text>().text = item.fragments.ToString();
                    obj.transform.Find("RawImage/RawImage").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        int sum = 0;
                        foreach (var item in cart)
                        {
                            sum += item.fragments;
                        }
                        if ((player.data.fragments - sum) < item.fragments)
                        {
                            if (!regular.active)
                            {
                                regular.SetActive(true);
                                describe.SetActive(false);
                            }
                            if (sum != 0)
                            {
                                StartDialogue(dialoguetext, "that's enough for you");
                            }
                            else
                            {
                                StartDialogue(dialoguetext, shorton[UnityEngine.Random.Range(0, shorton.Count)]);
                            }
                        }
                        else
                        {
                            cart.Add(item);
                            RefreshUIReceit();
                        }
                    });
                    foreach (visuals a in spriteData.spdt)
                    {
                        if (a.id == item.itemid && a.variant == item.variant)
                        {
                            obj.transform.Find("spritedisplay").GetComponent<Image>().sprite = a.sprites[0];
                        }
                        else if (a.id == item.itemid && item.variant == null)
                        {
                            foreach (Piecer b in piercerdatabase.roster)
                            {
                                if (b.ID == a.id && b.alt == a.variant)
                                {
                                    obj.transform.Find("spritedisplay").GetComponent<Image>().sprite = a.sprites[0];
                                }
                            }
                        }
                    }
                    foreach (Piecer a in piercerdatabase.roster)
                    {
                        if (a.ID == item.itemid)
                        {
                            if (item.variant != null)
                            {
                                obj.transform.Find("RawImage/name_level").GetComponent<TMP_Text>().text = $"{item.variant}-{a.LV}";
                            }
                            else
                            {
                                obj.transform.Find("RawImage/name_level").GetComponent<TMP_Text>().text = $"{a.alt}-{a.LV}";
                            }
                            obj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                            {
                                regular.SetActive(false);
                                describe.GetComponent<Button>().onClick.AddListener(() =>
                                {
                                    regular.SetActive(true);
                                    describe.SetActive(false);
                                });
                                describe.SetActive(true);
                                describe.transform.Find("Panel/RawImage/Text (TMP)").GetComponent<TMP_Text>().text = a.name;
                                describe.transform.Find("Panel/RawImage/Text (TMP) (1)").GetComponent<TMP_Text>().text = item.variant;
                                describe.transform.Find("Panel/RawImage/Text (TMP) (2)").GetComponent<TMP_Text>().text = $"LVL {a.LV}";
                                describe.transform.Find("Panel/RawImage (1)/Text (TMP)").GetComponent<TMP_Text>().text = a.effect.nature;
                                describe.transform.Find("Panel/RawImage (1)/Text (TMP) (2)").GetComponent<TMP_Text>().text = a.effect.type;
                                describe.transform.Find("Panel/RawImage (1)/Text (TMP) (1)").GetComponent<TMP_Text>().text = a.effect.name;
                                foreach (visuals a in spriteData.spdt)
                                {
                                    if (a.id == item.itemid && a.variant == item.variant)
                                    {
                                        describe.GetComponent<SpriteRenderer>().color = HexToColor(a.hexcolor);
                                        describe.transform.Find("Panel/Scroll View").GetComponent<Image>().color = HexToColor(a.hexcolor);
                                        describe.transform.Find("Panel/Spritedisplay").GetComponent<SpriteRenderer>().sprite = a.sprites[0];
                                    }
                                    else if (a.id == item.itemid && a.variant != item.variant)
                                    {
                                        foreach (Piecer v in piercerdatabase.roster)
                                        {
                                            if (v.ID == item.itemid && v.ID == a.id && v.alt == a.variant)
                                            {
                                                describe.GetComponent<SpriteRenderer>().color = HexToColor(a.hexcolor);
                                                describe.transform.Find("Panel/Scroll View").GetComponent<Image>().color = HexToColor(a.hexcolor);
                                                describe.transform.Find("Panel/Spritedisplay").GetComponent<SpriteRenderer>().sprite = a.sprites[0];
                                            }
                                        }
                                    }
                                }
                                StartDialogue(describe.transform.Find("Panel/Scroll View/Viewport/Content/Text (TMP)").GetComponent<TextMeshProUGUI>(), a.effect.desc);
                            });
                        }
                    }
                }
            }else if (shopping == "decors")
            {
                Debug.Log(item.type);
                if (item.type == "mat")
                {
                    GameObject obj = Instantiate(merchprefab, display.transform);
                    obj.transform.Find("RawImage/RawImage/Text (TMP) (1)").GetComponent<TMP_Text>().text = item.fragments.ToString();
                    obj.transform.Find("RawImage/RawImage").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        int sum = 0;
                        foreach (var item in cart)
                        {
                            sum += item.fragments;
                        }
                        if ((player.data.fragments - sum) < item.fragments)
                        {
                            if (!regular.active)
                            {
                                regular.SetActive(true);
                                describe.SetActive(false);
                            }
                            if (sum != 0)
                            {
                                StartDialogue(dialoguetext, "that's enough for you");
                            }
                            else
                            {
                                StartDialogue(dialoguetext, shorton[UnityEngine.Random.Range(0, shorton.Count)]);
                            }
                        }
                        else
                        {
                            cart.Add(item);
                            RefreshUIReceit();
                        }
                    });
                    foreach (Mat a in spriteData.mats)
                    {
                        if (a.id == item.itemid)
                        {
                            obj.transform.Find("spritedisplay").GetComponent<Image>().sprite = a.sprite;
                            obj.transform.Find("RawImage/name_level").GetComponent<TMP_Text>().text = a.name;
                            obj.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
                            {
                                regular.SetActive(false);
                                describe.GetComponent<Button>().onClick.AddListener(() =>
                                {
                                    regular.SetActive(true);
                                    describe.SetActive(false);
                                });
                                describe.SetActive(true);
                                describe.transform.Find("Panel/RawImage/Text (TMP)").GetComponent<TMP_Text>().text = a.name;
                                describe.transform.Find("Panel/RawImage/Text (TMP) (1)").GetComponent<TMP_Text>().text = item.type;
                                describe.transform.Find("Panel/RawImage/Text (TMP) (2)").GetComponent<TMP_Text>().text = "";
                                describe.transform.Find("Panel/RawImage (1)/Text (TMP)").GetComponent<TMP_Text>().text = "";
                                describe.transform.Find("Panel/RawImage (1)/Text (TMP) (2)").GetComponent<TMP_Text>().text = "";
                                describe.transform.Find("Panel/RawImage (1)/Text (TMP) (1)").GetComponent<TMP_Text>().text = "";
                                describe.GetComponent<SpriteRenderer>().color = HexToColor(a.hexcolor);
                                describe.transform.Find("Panel/Scroll View").GetComponent<Image>().color = HexToColor(a.hexcolor);
                                describe.transform.Find("Panel/Spritedisplay").GetComponent<SpriteRenderer>().sprite = a.sprite;
                                StartDialogue(describe.transform.Find("Panel/Scroll View/Viewport/Content/Text (TMP)").GetComponent<TextMeshProUGUI>(), a.desc);
                            });
                        }
                    }
                }
            }
        }
        updatebox();
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

    public void StartDialogue(TextMeshProUGUI dialogueText, string sentence)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeSentence(dialogueText, sentence));
    }

    IEnumerator TypeSentence(TextMeshProUGUI dialogueText, string sentence)
    {
        isTyping = true;
        talkingAnimator.SetBool(stsa, true);
        talkingAnimator.SetBool("isTyping", true);

        dialogueText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.text = sentence;

        while (dialogueText.maxVisibleCharacters < sentence.Length)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        talkingAnimator.SetBool("isTyping", false);
    }

    public void Skip(TextMeshProUGUI dialogueText, string fullSentence)
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = fullSentence;
            isTyping = false;
        }
    }

    public void Update()
    {
        foreach (visuals a in spriteData.spdt)
        {
            foreach(CapsuleDataDB b in instock)
            {
                if(a.id == b.itemid && a.variant == b.variant && b.featured)
                {
                    banner.transform.Find("Featured").GetComponent<SpriteRenderer>().sprite = a.sprites[0];
                    foreach (Piecer c in piercerdatabase.roster)
                    {
                        if(a.id == c.ID)
                        {
                            banner.transform.Find("Name").GetComponent<TMP_Text>().text = c.name;
                            banner.transform.Find("level").GetComponent<TMP_Text>().text = c.LV.ToString();
                        }
                    }
                }
            }
        }
    }
    public void PAY()
    {
        sfx.clip = Resources.Load<AudioClip>("sfx/Cash Register (Kaching) - Sound Effect (HD)");
        sfx.Play();
        StartDialogue(dialoguetext, "cash");
        if (cart.Count > 0)
        {
            int sum = 0;
            foreach (var item in cart)
            {
                sum += item.fragments;
            }
            player.data.fragments -= sum;
            if(player.data.fragments < 0)
            {
                player.data.fragments = 0;
            }
            foreach (var item in cart)
            {
                StorageDB.DeleteFromStorage(item.capid);
                InventoryDB.AddToInventory(player.data.user_id, item.capid);
            }
            cart.Clear();
            RefreshUI(shopping);
            RefreshUIReceit();
        }
        else
        {
            if (!regular.active)
            {
                regular.SetActive(true);
                describe.SetActive(false);
            }
            StartDialogue(dialoguetext, "pick sum, mf");
        }
    }
}
