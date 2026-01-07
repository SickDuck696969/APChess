using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Actions : NetworkBehaviour
{
    public Piecer piecer; 
    public Animator animator;
    public UnityEngine.UI.Button actionButton;
    public GameObject effectbox;
    public Vector3 initpos;
    public Vector2 initsize;
    public GameObject statuseffectprefab;
    void Start()
    {
        animator = GetComponent<Animator>();
        actionButton = GetComponentInChildren<UnityEngine.UI.Button>();
        actionButton.onClick.AddListener(Action);
        transform.Find("SpriteDisplay").GetComponent<Animator>().enabled = false;
        effectbox = GameObject.Find("Canvas/Status Effects");
        initpos = new Vector3(27.435f, -46.65599f, -0.7f);
        initsize = new Vector2(17.834f, 0);
    }
    void Update()
    {
        effectbox = GameObject.Find("Canvas/Status Effects");
        if (piecer != null)
        {
            transform.Find("SpriteDisplay").GetComponent<SpriteRenderer>().sprite = piecer.Skin[0];
            transform.Find("Canvas/Name").GetComponent<TextMeshProUGUI>().text = piecer.name;
            transform.Find("Canvas/Level").GetComponent<TextMeshProUGUI>().text = "Level " + piecer.LV;
            transform.Find("Canvas/Scroll View/Viewport/Content/Effect Name").GetComponent<TextMeshProUGUI>().text = piecer.effect.name;
            transform.Find("Canvas/Scroll View/Viewport/Content/Effect Description").GetComponent<TextMeshProUGUI>().text = piecer.effect.desc;
            if (!piecer.effect.enabled || piecer.tile.getsatus(0) || !piecer.tile.getsatus(1))
            {
                actionButton.enabled= false;
            }
            else
            {
                actionButton.enabled= true;
            }
        }
        foreach (EffectStatus status in piecer.effectStatuses)
        {
            bool found = false;
            foreach (Transform child in effectbox.transform)
            {
                if (status.name == child.name)
                {
                    TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>();
                    string txt = tmp.text;
                    if(txt != "")
                    {
                        string num = txt.Substring(2);
                        if (int.Parse(num) < status.stack)
                        {
                            tmp.text = "x " + (int.Parse(num) + 1);
                        }
                    }
                    else
                    {
                        if (status.stack > 1)
                        {
                            tmp.text = "x " + status.stack;
                        }
                    }
                        found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.Log("butts");
                if (statuseffectprefab == null)
                {
                    Debug.LogWarning("No infocard prefab assigned!");
                    return;
                }
                GameObject statusnew = Instantiate(statuseffectprefab);
                statusnew.transform.SetParent(effectbox.transform);
                statusnew.name = status.name;
                statusnew.GetComponent<SpriteRenderer>().sprite = status.visual;
                statusnew.GetComponent<RectTransform>().localScale = new Vector3(28, 28, 28);
            }
        }
        updatebox();
    }

    public void Action()
    {
        StartCoroutine(PlayAnimationThenIdle());
    }

    public void updatebox()
    {
        RectTransform boxrect = effectbox.GetComponent<RectTransform>();
        float wantedheight = 0;
        LayoutRebuilder.ForceRebuildLayoutImmediate(boxrect);
        foreach (Transform child in effectbox.transform)
        {
            if (child != null)
            {
                Debug.Log(child.name);
                wantedheight += child.GetComponent<RectTransform>().sizeDelta.y;
                child.GetComponent<RectTransform>().localPosition = new Vector3(child.GetComponent<RectTransform>().localPosition.x, child.GetComponent<RectTransform>().localPosition.y, -0.7f);
            }
        }
        boxrect.sizeDelta = new Vector2(boxrect.sizeDelta.x, initsize.y + wantedheight);
        boxrect.localPosition = new Vector3(initpos.x, initpos.y + wantedheight/2f, initpos.z);
    }

    private IEnumerator PlayAnimationThenIdle()
    {
        animator.Play("act");
        if(piecer.effect.clip != null)
        {
            transform.Find("SpriteDisplay").GetComponent<Animator>().enabled = true;
            transform.Find("SpriteDisplay").GetComponent<Animator>().Play(piecer.effect.clip.name);
        }
        yield return null;
        if (piecer.effect != null && piecer.effect.type == "Active")
        {
            piecer.effect.action();
        }
        yield return new WaitForSeconds(transform.Find("SpriteDisplay").GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        if (piecer.effect.clip != null)
        {
            transform.Find("SpriteDisplay").GetComponent<Animator>().enabled = false;
        }
        animator.Play("New Animation");
    }

}
