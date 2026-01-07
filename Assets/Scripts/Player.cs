using NUnit.Framework;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;
[CreateAssetMenu(fileName = "Player", menuName = "Scriptables/Player")]
public class Player : ScriptableObject
{
    public PlayerData data;
    public Color pColor = Color.black;
    public string ip;
    public string Mat;
    public int gems = 0;
    public int fragments = 0;
    public List<Deck> Decks = new List<Deck>();
    public List<Piecer> roster = new List<Piecer>();
    public Sprite ava;
    public void HexToColor(string hex)
    {
        Color col;
        string hexColor = "#"+hex;
        bool ok = ColorUtility.TryParseHtmlString(hexColor, out col);

        if (!ok)
            Debug.LogError("Invalid hex string: " + hex);

        pColor = col;
        Debug.Log("Parsed color: " + pColor);
    }

    public void OnEnable()
    {
        ip = GetLocalIPAddress();
    }
    string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "No IPv4 address found";
    }
}
