using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Gacha : MonoBehaviour
{
    public GameObject notch;
    public AudioSource audioSource;
    public GameObject Ringlight;
    public int roundaround = 0;
    public InventoryDatabase InventoryDB;
    public StorageDatabase StorageDB;
    public CapsuleDatabase CapsuleDB;
    public piercerdatabase piercerdatabase;
    public SpriteData spriteData;
    public List<CapsuleDataDB> instock = new List<CapsuleDataDB>();
    public List<CapsuleDataDB> bag = new List<CapsuleDataDB>();
    public Player player;
    public Button oneppullbutt;
    public Button tenpullbutt;
    public int howpany = 0;
    public GameObject capsemm;
    public GameObject background;
    public GameObject butt;
    public Animator animator;
    void Start()
    {
        InventoryDB = new InventoryDatabase();
        StorageDB = new StorageDatabase();
        CapsuleDB = new CapsuleDatabase();
        notch.GetComponent<Button>().onClick.AddListener(turn);
        List<StorageDataDB> storage = StorageDB.GetStorage(2);
        instock.Clear();
        foreach (StorageDataDB c in storage)
        {
            instock.Add(CapsuleDB.GetCapsule(c.capsuleid));
        }
    }
    public void turn()
    {
        Debug.Log("Turn");
        StartCoroutine(SpinNotch());
    }
    private IEnumerator SpinNotch()
    {
        notch.GetComponent<Button>().enabled = false;
        float duration = 0.25f;
        float elapsed = 0f;

        Quaternion startRotation = notch.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0f, 0f, -90f);
        audioSource.clip = Resources.Load<AudioClip>("sfx/crank");
        audioSource.Play();
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            notch.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }
        roundaround++;
        if(roundaround == 1)
        {
            Ringlight.GetComponentInChildren<Image>().color = Color.cyan;
        }else if(roundaround == 2)
        {
            Ringlight.GetComponentInChildren<Image>().color = Color.cyan;
        }else if(roundaround == 3)
        {
            int caproll = 0;
            for(int i = 0; i < howpany; i++)
            {
                int roll = RollLevel();
                if(roll > caproll) { caproll = roll;}
                Debug.Log(roll);
                if (caproll == 1)
                {
                    Ringlight.GetComponentInChildren<Image>().color = Color.cyan;
                }
                else if (caproll == 2)
                {
                    Ringlight.GetComponentInChildren<Image>().color = Color.green;
                }
                else if (caproll == 3)
                {
                    Ringlight.GetComponentInChildren<Image>().color = Color.red;
                }
                else if (caproll == 4)
                {
                    Ringlight.GetComponentInChildren<Image>().color = Color.yellow;
                }
                else if (caproll == 5)
                {
                    Ringlight.GetComponentInChildren<Image>().color = Color.purple;
                }
                CapsuleDataDB temp = new CapsuleDataDB();
                List<CapsuleDataDB> caps = new List<CapsuleDataDB>();
                foreach (CapsuleDataDB a in instock)
                {
                    if (a.type == "piercer")
                    {
                        foreach (Piecer b in piercerdatabase.roster)
                        {
                            Debug.Log(b.name);
                            if (a.itemid == b.ID && b.LV == roll)
                            {
                                Debug.Log(b.name);
                                caps.Add(a);
                            }
                        }
                    }
                }
                if (caps.Count > 0)
                {
                    CapsuleDataDB temptemp = caps[UnityEngine.Random.Range(0, caps.Count)];
                    temp.capid = temptemp.capid;
                    temp.itemid = temptemp.itemid;
                    temp.variant = temptemp.variant;
                    Debug.Log($"{temp.capid}, {temp.variant}");
                }
                roll = RollLevel();
                bag.Add(temp);
            }
            howpany = 0;
            roundaround = 0;
            endRotation = Quaternion.Euler(0f, 0f, -90f);
            StartCoroutine(drop());
            foreach (CapsuleDataDB c in bag)
            {
                Debug.Log($"{c.capid}, {c.variant}");
                InventoryDB.AddToInventory(player.data.user_id, c.capid);
            }
            
        }
        Ringlight.GetComponent<Animator>().Play("pulse");
        notch.transform.rotation = endRotation;
        notch.GetComponent<Button>().enabled = true;
    }

    IEnumerator drop()
    {
        audioSource.clip = Resources.Load<AudioClip>("sfx/Metal Trash Can Filled [SOUND EFFECT]");
        audioSource.Play();

        yield return new WaitForSeconds(audioSource.clip.length);

        StartCoroutine(PlayAnimationThenIdle());
    }

    public void onepull()
    {
        audioSource.clip = Resources.Load<AudioClip>("sfx/onepull");
        audioSource.Play();
        howpany++;
        player.data.gems -= 180;
        if(player.data.gems < 0 ) player.data.gems = 0;
        if (howpany > 1)
        {
            oneppullbutt.GetComponentInChildren<TMP_Text>().text = $"{howpany} pull";
        }
    }
    public void tenpull()
    {
        audioSource.clip = Resources.Load<AudioClip>("sfx/onepull");
        audioSource.Play();
        howpany = 10;
        player.data.gems -= 1800;
        if (player.data.gems < 0) player.data.gems = 0;
    }
    void Update()
    {
        if (player.data.gems < 180 || howpany == 10)
        {
            oneppullbutt.enabled = false;
        }
        else
        {
            oneppullbutt.enabled = true;
        }

        if (player.data.gems < 1800 || howpany == 10)
        {
            tenpullbutt.enabled = false;
        }
        else
        {
            tenpullbutt.enabled = true;
        }

        if(!(howpany > 1))
        {
            oneppullbutt.GetComponentInChildren<TMP_Text>().text = $"1 pull";
        }

        if (howpany <= 0)
        {
            notch.GetComponent<Button>().enabled = false;
        }
        else
        {
            notch.GetComponent<Button>().enabled = true;
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
    private IEnumerator PlayAnimationThenIdle()
    {
        if (bag.Count > 0)
        {
            foreach (visuals v in spriteData.spdt)
            {
                if (bag[0].itemid == v.id && bag[0].variant == v.variant)
                {
                    capsemm.transform.Find("Layer 2").GetComponent<SpriteRenderer>().color = HexToColor(v.hexcolor);
                    butt.GetComponent<SpriteRenderer>().sprite = v.sprites[0];
                    background.GetComponent<Collider2D>().enabled = false;
                }
            }
        }
        audioSource.clip = Resources.Load<AudioClip>("sfx/caps");
        audioSource.Play();
        animator.SetBool("wedon", true);
        yield return new WaitForSeconds(audioSource.clip.length-0.01f);
        animator.SetBool("wedon", false);
        animator.Play("huh", 0, 0f);
        bag.Remove(bag[0]);
        background.GetComponent<Collider2D>().enabled = true;
        Debug.Log(bag.Count);
        background.GetComponent<Button>().onClick.RemoveAllListeners();
        background.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (bag.Count > 0)
            {
                StartCoroutine(PlayAnimationThenIdle());
            }
            else
            {
                animator.SetBool("wedon", true);
                background.GetComponent<Collider2D>().enabled = false;
                background.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0);
                butt.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0);
                Ringlight.GetComponentInChildren<Image>().color = Color.white;

            }
        });
    }

    public static int RollLevel()
    {
        float roll = UnityEngine.Random.Range(0f, 100f);

        if (roll < 0.6f)
            return 5;

        if (roll < 0.6f + 5.1f)
            return 4;

        if (roll < 0.6f + 5.1f + 24.3f)
            return 3;

        if (roll < 0.6f + 5.1f + 24.3f + 40.0f)
            return 2;

        return 1;
    }
}
