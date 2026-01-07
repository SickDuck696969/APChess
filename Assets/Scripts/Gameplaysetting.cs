using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Player", menuName = "Scriptables/Gameplaysetting")]
public class Gameplaysetting : ScriptableObject
{
    public List<Player> players = new List<Player>();
    public List<Piecer> GY = new List<Piecer>();
    public int buildingdeck;
    public List<AudioClip> MainManuPlayList = new List<AudioClip>();
    public List<AudioClip> ShopPlayList = new List<AudioClip>();
    public List<AudioClip> LobbyPlayList = new List<AudioClip>();
    public List<AudioClip> MatchPlayList = new List<AudioClip>();
    public AudioClip playing = null;
    public Sprite example;
    public string log = "";
}
