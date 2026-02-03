using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D.Animation;
[CreateAssetMenu(fileName = "SpriteData", menuName = "Scriptables/SpriteData")]
public class SpriteData : ScriptableObject
{
    public List<visuals> spdt = new List<visuals>();   
    public List<Mat> mats = new List<Mat>();
    public List<avatar> avatars = new List<avatar>();
}
[System.Serializable]
public class visuals
{
    public int id;
    public Sprite[] sprites;
    public string variant;
    public int tier = 1;
    public string hexcolor = string.Empty;
    public SpriteLibraryAsset librabry;
}

[System.Serializable]
public class avatar
{
    public int id;
    public Sprite avatara;
}

[System.Serializable]
public class Mat
{
    public int id;
    public string name;
    public Sprite[] mat;
    public Sprite deck;
    public Sprite sprite;
    public int tier = 1;
    public string hexcolor = string.Empty;
    public string desc = "";
}
