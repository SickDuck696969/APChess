using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Unit", menuName = "Unit")]
public class Unit : ScriptableObject {
    public int unitID;
    public string unitName;
    public Sprite unitSprite;
    public string unitDescription;
    public int cost;
    public bool isOwned;
}
