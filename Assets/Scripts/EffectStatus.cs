using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class EffectStatus
{
    public int ID;
    public string name;
    public Sprite visual;
    public string desc;
    public Piecer from;
    public Piecer on;
    public string nature;
    public int stack = 1;
    public bool isbad = false;

    public virtual void actioncheck() { }
}

public class Chocodan : EffectStatus
{
    public Chocodan(Piecer from, Piecer on, string nature, Sprite visual)
    {
        ID = 2141;
        name = "Chocodan";
        this.visual = visual;
        desc = "You've been Shot and Marked, 3 of these will render your ability null, 4 and you're dead.";
        this.from = from;
        this.on = on;
        this.nature = nature;
        isbad = true;
    }

    public override void actioncheck()
    {
        int count = 0;
        foreach(EffectStatus shot in on.effectStatuses)
        {
            if (shot.name == name)
            {
                count++;
            }
        }
        if(count == 3)
        {
            on.effect.enabled = false;
        } else if (count == 4)
        {
            on.tile.gameplaysetting.GY.Add(on.tile.piecer);
            on.tile.piecer = null;
            foreach (Piecer a in on.tile.gameplaysetting.GY)
            {
                Debug.Log(a.name);
            }
        }
    }
}

public class HolyBarrier : EffectStatus
{
    public HolyBarrier(Piecer from, Piecer on, string nature, Sprite visual)
    {
        ID = 641;
        name = "HolyBarrier";
        this.visual = visual;
        desc = "If you have a negative effect on you, you will be cured.";
        this.from = from;
        this.on = on;
        this.nature = nature;
        isbad = false;
        if (on.effectStatuses.Where(e => e.isbad).ToList().Count == 0) on.tile.react("Holy Barrier", from.Skin[1]);
    }

    public override void actioncheck()
    {
        List<EffectStatus> badStatuses =
            on.effectStatuses.Where(e => e.isbad).ToList();
        if(badStatuses.Count > 0)
        {
            EffectStatus badStatus = badStatuses[UnityEngine.Random.Range(0, badStatuses.Count)];
            if(on.tile.animationruuning != null)
            {
                on.tile.StopCoroutine(on.tile.animationruuning);
            }
            on.tile.animationruuning = on.tile.StartCoroutine(cureqeue(badStatus));
            on.effectStatuses.Remove(badStatus);
            on.effectStatuses.Remove(this);
        }
    }

    public IEnumerator cureqeue(EffectStatus badStatus)
    {
        Animator anim = null;
        try
        {
            anim = on.tile.transform.Find("effect").GetComponent<Animator>();
        }
        catch (ArgumentNullException ex)
        {
            Debug.Log(ex);
            yield break;
        }
        on.tile.transform.Find("effect").GetComponent<SpriteRenderer>().sprite = visual;
        anim.Play("Holy Barrier", 0, 0f);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        on.tile.transform.Find("effect").GetComponent<SpriteRenderer>().sprite = badStatus.visual;
        yield return new WaitUntil(() => on.tile.transform.Find("effect").GetComponent<SpriteRenderer>().sprite != null);
        anim.Play("cure", 0, 0f);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
    }
}


public class Corrupted : EffectStatus
{
    public Corrupted(Piecer from, Piecer on, string nature, Sprite visual)
    {
        ID = 677911;
        name = "Corrupted";
        this.visual = visual;
        desc = "You've been Shot and Marked, 3 of these will render your ability null, 4 and you're dead.";
        this.from = from;
        this.on = on;
        this.nature = nature;
        isbad = true;
    }
}