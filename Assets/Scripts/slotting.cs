using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static Unity.Burst.Intrinsics.X86.Avx;

public class slotting : MonoBehaviour, IDropHandler
{
    public GameObject bigboard;
    public int slot;
    public string type;
    deckbuilder db;
    public GameObject it;
    void Start()
    {
        db = bigboard.GetComponent<deckbuilder>();
    }
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Slotting");
        capsem c = eventData.pointerDrag.GetComponent<capsem>();
        GameObject[] a = db.getas();
        GameObject matplate = db.getmat();
        bool pass = false;
        if (type == "piercer")
        {
            if (c.piecer != null)
            {
                pass = true;
            }
        }
        else if (type == "mat")
        {
            if (c.mat != null)
            {
                pass = true;
            }
        }
        if (pass)
        {
            if (type == "piercer")
            {
                if (a[slot - 1] == null)
                {
                    it = c.gameObject;
                    c.transform.SetParent(transform, true);
                    c.originalParent = c.transform.parent;
                    c.GetComponent<capsem>().display();
                }
                else
                {
                    Debug.Log("butt");
                }
            }
            else if (type == "mat")
            {
                if (matplate == null)
                {
                    it = c.gameObject;
                    c.transform.SetParent(transform, true);
                    c.originalParent = c.transform.parent;
                }
                else
                {
                    Debug.Log("butt");
                }
            }
        }
    }

    void Update()
    {
        if(transform.childCount != 0)
        {
            GameObject a = transform.GetChild(0).gameObject;
            if (type == "mat")
            {
                db.matplate = it;
                db.localtempdeck.mat_id = it.GetComponent<capsem>().capid;
            }
            else if (type == "piercer")
            {
                db.TempDeck[slot - 1] = it;
                switch (slot)
                {
                    case 1:
                        db.localtempdeck.slot1 = it.GetComponent<capsem>().capid;
                        break;
                    case 2:
                        db.localtempdeck.slot2 = it.GetComponent<capsem>().capid;
                        break;
                    case 3:
                        db.localtempdeck.slot3 = it.GetComponent<capsem>().capid;
                        break;
                    case 4:
                        db.localtempdeck.slot4 = it.GetComponent<capsem>().capid;
                        break;
                    case 5:
                        db.localtempdeck.slot5 = it.GetComponent<capsem>().capid;
                        break;
                }
            }
        }
        else
        {
            if (type == "mat")
            {
                db.matplate = null;
                db.localtempdeck.mat_id = 0;
            }
            else if (type == "piercer")
            {
                db.TempDeck[slot - 1] = null;
                switch (slot)
                {
                    case 1:
                        db.localtempdeck.slot1 = 0;
                        break;
                    case 2:
                        db.localtempdeck.slot2 = 0;
                        break;
                    case 3:
                        db.localtempdeck.slot3 = 0;
                        break;
                    case 4:
                        db.localtempdeck.slot4 = 0;
                        break;
                    case 5:
                        db.localtempdeck.slot5 = 0;
                        break;
                }
            }
        }
    }
}
