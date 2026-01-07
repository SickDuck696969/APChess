using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Effect
{
    public int ID;
    public string name = "";
    public string type = "";
    public string desc = "";
    public string belong = "";
    public int cooldown;
    public Piecer caster;
    public AnimationClip clip;
    public bool enabled = true;
    public string range;
    public bool friendly = false;
    public string nature = "";
    public SpriteData sprites;

    public virtual void end()
    {
        Debug.Log("spell end");
        caster.tile.resetattacked();
        caster.tile.isSpelling = false;
        caster.tile.setstatus(0, true);
        caster.tile.range = "1 diagonal";
    }

    public virtual void action() { }

    public virtual IEnumerator PlayAnimationThenIdle() {
        yield return null;
    }

    public virtual void actioncheck() { }

    public virtual Effect equip()
    {
        Effect ability = new Effect();
        ability.ID = ID;
        ability.name = name;
        ability.type = type;
        ability.desc = desc;
        ability.belong = belong;
        return ability;
    }
}
public class Test : Effect
{
    public Test()
    {
        name = "Test";
        type = "Passive";
        desc = "we shmovin";
        cooldown = 0;
    }
    public override void action()
    {
        Debug.Log("testing testing");
    }
}

public class SwiftMotion : Effect
{
    public SwiftMotion(Piecer caster)
    {
        ID = 901;
        name = "Swift Motion";
        desc = "Make one more move, capture allowed.";
        type = "Active";
        belong = "Ninja";
        cooldown = 3;
        this.caster = caster;
        clip = Resources.Load<AnimationClip>("Images/Sprites/Cost 1/Ninja/SwiftMotion");
        nature = "physical";
    }
    public override void action()
    {
        if (caster.tile.getsatus(1))
        {
            Debug.Log($"{caster.tile.pos[0]}, {caster.tile.pos[1]}");
            caster.xtramove = true;
            caster.tile.setstatus(0, true);
            caster.tile.clip = clip;
            end();
        }
    }
}

public class MarkShot : Effect
{
    public MarkShot(Piecer caster)
    {
        ID = 21401;
        name = "Mark Shot";
        desc = "Mark an enemy within a 2 tiles radius with 1 Chocodan.\r\nIf an enemy accumulates 4 Chocodan, it is immediately captured. The Chocodan status remains until removed.";
        type = "Active";
        belong = "Marksman";
        this.caster = caster;
        cooldown = 1;
        clip = Resources.Load<AnimationClip>("Images/Sprites/Cost 1/Marksman/shot");
        range = "2 radius";
        nature = "physical";
    }

    public override void action()
    {
        caster.tile.range = range;
        caster.tile.setstatus(2, true);
    }

    public override void actioncheck()
    {
        Debug.Log($"{caster.tile.gettarget()}, {caster.tile.getsatus(2)}, [{caster.tile.pos[0]}, {caster.tile.pos[1]}]");
        if (caster.tile.getsatus(2) && caster.tile.gettarget() != null)
        {
            Tile target = caster.tile.gettarget();
            caster.tile.setstatus(2, false);
            Chocodan bullet = new Chocodan(caster, target.piecer, nature, caster.Skin[1]);
            EffectStatus it = null;
            foreach(EffectStatus status in target.piecer.effectStatuses)
            {
                if(status.name == bullet.name)
                {
                    status.stack++;
                }
            }
            if (it == null)
            {
                target.piecer.effectStatuses.Add(bullet);
                target.GetComponent<SpriteRenderer>().sprite = caster.Skin[0];
                target.react("shot", caster.Skin[1]);
                target.resettarget();
            }
            end();
        }
    }
}

public class UnshakableFortress : Effect
{
    public UnshakableFortress(Piecer caster)
    {
        ID = 7771;
        name = "Unshakable Fortress";
        desc = "If an adjacent ally is to be captured or attacked by physical abilities, it won’t. Vanguard itself is immune to all status effects and cannot be captured by skill effects.";
        type = "Passive";
        belong = "Vanguard";
        this.caster = caster;
        clip = Resources.Load<AnimationClip>("Images/Sprites/Cost 1/Ninja/SwiftMotion");
        nature = "physical";
    }

    public override void actioncheck()
    {
        foreach (Player a in caster.tile.gameplaysetting.players)
        {
            foreach (Piecer b in a.Decks[0].Piecers)
            {
                if(b.tile != null)
                {
                    if (Mathf.Abs(b.tile.pos[0] - caster.tile.pos[0]) <= 1 && Mathf.Abs(b.tile.pos[1] - caster.tile.pos[1]) <= 1 && b.tile != caster.tile)
                    {
                        if (b.tile.getattacked() != null && b.tile.getattacked() == b.tile)
                        {
                            if (b.tile.getcurrentselected() != null && b.tile.getcurrentselected().piecer != null) 
                            {
                                if (b.tile.getcurrentselected().isSpelling && b.tile.getcurrentselected().piecer.effect.nature != "meta")
                                {
                                    b.tile.react("guard", caster.Skin[1]);
                                    b.tile.setattacked(caster.tile);
                                    b.tile.getcurrentselected().piecer.effect.end();
                                }
                            }
                            else
                            {
                                b.tile.react("guard", caster.Skin[1]);
                                b.tile.resetattacked();
                                b.tile.getcurrentselected().setstatus(1, true);
                            }
                            b.tile.objection = false;
                        }
                        else
                        {
                            Debug.Log("reset");
                            b.tile.objection = true;
                        }
                    }
                }
            }
        }
    }
}

public class SoulBind : Effect
{
    public List<EffectStatus> keeptrack = new List<EffectStatus>();
    public SoulBind(Piecer caster)
    {
        ID = 67791;
        name = "Soul Bind";
        desc = "Choose an enemy and mark it as Corrupted. If the Warlock is captured, all Corrupted enemies are captured as well. You can have up to 3 enemies marked with Corrupted at once.";
        type = "Active";
        belong = "Warlock";
        this.caster = caster;
        cooldown = 4;
        nature = "meta";
        int stack = 0;
        range = "0 fullboard";
        friendly = true;
    }

    public override void action()
    {
        caster.tile.range = range;
        caster.tile.setstatus(2, true);
    }

    public override void actioncheck()
    {
        if (caster.tile.gameplaysetting.GY.Contains(caster))
        {
            if (keeptrack.Count > 0) 
            {
                foreach (Corrupted bind in keeptrack)
                {
                    if(bind.on != null)
                    {
                        bind.on.tile.react("corrupt", caster.Skin[1]);
                        caster.tile.gameplaysetting.GY.Add(bind.on);
                        bind.on.tile.piecer = null;
                        bind.on.effectStatuses.Remove(bind);
                        keeptrack.Remove(bind);
                    }
                    if(bind.on == null)
                    {
                        keeptrack.Remove(bind);
                    }
                }
            }
        }
        if (caster.tile.getsatus(2) && caster.tile.gettarget() != null)
        {
            Tile target = caster.tile.gettarget();
            caster.tile.setstatus(2, false);
            Corrupted bind = new Corrupted(caster, target.piecer, nature, caster.Skin[1]);
            if (!target.piecer.effectStatuses.Contains(bind) && keeptrack.Count < 2)
            {
                target.piecer.effectStatuses.Add(bind);
                keeptrack.Add(bind);
                target.GetComponent<SpriteRenderer>().sprite = caster.Skin[0];
                target.react("corrupt", caster.Skin[1]);
                target.resettarget();
                end();
            }
        }
    }
}

public class HolyWard : Effect
{
    public HolyWard(Piecer caster)
    {
        ID = 6401;
        name = "Holy Ward";
        desc = "Grant an ally a Holy Barrier that nullifies the next negative status inflicted on them. If the ally already has a negative status, Holy Barrier instead cures 1 random negative status.";
        type = "Active";
        belong = "Cleric";
        this.caster = caster;
        cooldown = 5;
        range = "0 fullboard";
        nature = "meta";
        friendly = true;
    }

    public override void action()
    {
        caster.tile.range = range;
        caster.tile.setstatus(2, true);
    }

    public override void actioncheck()
    {
        Debug.Log($"{caster.tile.gettarget()}, {caster.tile.getsatus(2)}, [{caster.tile.pos[0]}, {caster.tile.pos[1]}]");
        if (caster.tile.getsatus(2) && caster.tile.gettarget() != null)
        {
            Tile target = caster.tile.gettarget();
            caster.tile.setstatus(2, false);
            HolyBarrier barrier = new HolyBarrier(caster, target.piecer, nature, caster.Skin[1]);
            EffectStatus it = null;
            foreach (EffectStatus status in target.piecer.effectStatuses)
            {
                if (status.name == barrier.name)
                {
                    status.stack++;
                }
            }
            if (it == null)
            {
                target.piecer.effectStatuses.Add(barrier);
                target.GetComponent<SpriteRenderer>().sprite = caster.Skin[0];
                target.resettarget();
            }
            end();
        }
    }
}

public class DragonsWrath : Effect
{
    public DragonsWrath(Piecer caster)
    {
        ID = 20021;
        name = "Dragon's Wrath";
        desc = "Move up to 3 in any direction, ignoring any units in the path. Leave a Flaming Trail on every tile passed through. The Flaming Trail lasts until the end of your next turn. Any enemy that steps";
        type = "Active";
        belong = "DragonKnight";
        this.caster = caster;
        cooldown = 4;
        range = "3 straight";
        nature = "physical";
        friendly = true;
    }
}

public class FrenzyCircuit: Effect
{
    public FrenzyCircuit(Piecer caster)
    {
        ID = 20001;
        name = "Frenzy Circuit";
        desc = "Move up to 2 tiles diagonally, then up to 2 tiles straight.\r\nRanger ignores all units while moving (cannot be blocked and does not capture while passing through).\r\n";
        type = "Active";
        belong = "Ranger";
        this.caster = caster;
        cooldown = 3;
        range = "0 fullboard";
        nature = "physical";
        friendly = true;
    }
}