using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "piercerdatabase", menuName = "Scriptable Objects/piercerdatabase")]
public class piercerdatabase : ScriptableObject
{
    public List<Piecer> roster = new List<Piecer>();
    public void OnEnable()
    {
        roster.Clear();
        roster.Add(new Ninja());
        roster.Add(new Marksman());
        roster.Add(new Vanguard());
        roster.Add(new Warlock());
        roster.Add(new Cleric());
        roster.Add(new DragonKnight());
        roster.Add(new Ranger());
        roster.Add(new Avatar());
        roster.Add(new Plasma());
    }
}
