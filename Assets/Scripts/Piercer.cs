using System.Collections.Generic;
using UnityEngine;

public class Piecer
{
    public int ID;
    public string name;
    public string alt;
    public int LV;
    public string move;
    public Tile tile;
    public bool xtramove = false;
    public Color color = Color.white;
    public int cost = 0;
    public Sprite[] Skin;

    public Effect effect = new Test();
    public List<EffectStatus> effectStatuses = new List<EffectStatus>();

    public Piecer() { }

    public virtual Piecer Clone()
    {
        Debug.Log("Cloning: Piecer");
        Piecer piece = new Piecer();
        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.tile = tile;
        piece.LV = LV;
        piece.Skin = Skin;

        return piece;
    }
}

public class Ninja : Piecer
{
    public Ninja()
    {
        ID = 9;
        name = "Ninja";
        alt = "Shinobi Verd";
        tile = null;
        LV = 1;
        effect = new SwiftMotion(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Ninja");
        Ninja piece = new Ninja();
        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class Marksman : Piecer
{
    public Marksman()
    {
        ID = 214;
        name = "Marksman";
        alt = "Choco-Slinger";
        tile = null;
        LV = 1;
        effect = new MarkShot(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Marksman");
        Marksman piece = new Marksman();
        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class Vanguard : Piecer
{
    public Vanguard()
    {
        ID = 777;
        name = "Vanguard";
        alt = "Might Solid";
        tile = null;
        LV = 2;
        effect = new UnshakableFortress(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Vanguard");
        Vanguard piece = new Vanguard();
        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;
        return piece;
    }
}

public class Warlock : Piecer
{
    public Warlock()
    {
        ID = 6779;
        name = "Warlock";
        alt = "Handy Anne";
        tile = null;
        LV = 4;
        effect = new SoulBind(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Warlock");

        Warlock piece = new Warlock();

        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class Cleric : Piecer
{
    public Cleric()
    {
        ID = 64;
        name = "Cleric";
        alt = "Hocus Sister";
        tile = null;
        LV = 1;
        effect = new HolyWard(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Cleric");

        Cleric piece = new Cleric();

        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class DragonKnight : Piecer
{
    public DragonKnight()
    {
        ID = 2002;
        name = "Dragon Knight";
        alt = "DragRider";
        tile = null;
        LV = 5;
        effect = new DragonsWrath(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: DragonKnight");

        DragonKnight piece = new DragonKnight();

        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class Ranger : Piecer
{
    public Ranger()
    {
        ID = 2000;
        name = "Ranger";
        alt = "ToraRed";
        tile = null;
        LV = 3;
        effect = new SoulBind(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Ranger");

        Ranger piece = new Ranger();

        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class Plasma : Piecer
{
    public Plasma()
    {
        ID = 110100;
        name = "Plasma";
        alt = "Inazuma Plasma";
        tile = null;
        LV = 5;
        effect = new VoltageCharge(this);
        xtramove = false;
    }

    public override Piecer Clone()
    {
        Debug.Log("Cloning: Plasma");

        Plasma piece = new Plasma();

        piece.ID = ID;
        piece.name = name;
        piece.alt = alt;
        piece.LV = LV;
        piece.xtramove = xtramove;

        return piece;
    }
}

public class Avatar : Piecer
{
    public Avatar()
    {
        ID = 1;
        name = "Avatar";
        alt = "Base";
        tile = null;
        LV = 1;
        effect = new SoulBind(this);
        xtramove = false;
    }
}