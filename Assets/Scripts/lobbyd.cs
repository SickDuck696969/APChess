using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class lobbyd : MonoBehaviour, IPointerDownHandler
{
    public Sprite basetexture;
    public TMP_InputField roomname;
    public GameObject hostbutton;
    public GameObject playbutton;
    public GameObject refreshbutton;
    public Lobbysc ass;
    public Sprite a;
    public LobbyInfo lobby;
    public string currentFilter;
    public Player player;
    // Update is called once per frame
    void Update()
    {
        transform.Find("ID").GetComponent<TMP_Text>().text = lobby.lobbyId;
        transform.Find("Roomname").GetComponent<TMP_Text>().text = lobby.lobbyName;
        transform.Find("Hostname").GetComponent<TMP_Text>().text = lobby.hostName;
        if (a == null)
        {
            transform.Find("RawImage").GetComponent<Image>().sprite = basetexture;
        }
        else
        {
            transform.Find("RawImage").GetComponent<Image>().sprite = a;
        }
    }

    public async void OnPointerDown(PointerEventData eventData)
    {
        await ass.clientConnector.LeaveLobby();
        ass.clientConnector.JoinLobby(lobby.lobbyId, player.name);
    }

    public string SpriteToBase64(Sprite sprite)
    {
        Texture2D tex = sprite.texture;
        byte[] bytes = tex.EncodeToPNG();
        return Convert.ToBase64String(bytes);
    }

    public Sprite Base64ToSprite(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
}
