using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneNavigator : NetworkBehaviour
{
    public Gameplaysetting gameplaysetting;
    public Player player;
    public piercerdatabase pcdtb;
    public GameObject inputField;

    private HostStarter? hostStarter;
    private ClientConnector? clientConnector;

    public ScriptableRendererFeature feature;

    public bool online;

    public TMP_Text username;
    public SpriteRenderer profile;
    private async void Start()
    {
        if (SceneManager.GetActiveScene().name == "scene0")
        {
            SceneManager.LoadScene("Login", LoadSceneMode.Single);
        }else if(SceneManager.GetActiveScene().name == "MainMenu")
        {
        }
        if (feature != null) 
        {
            if (SceneManager.GetActiveScene().name == "Login")
            {
                feature.SetActive(true);
            }
            else
            {
                feature.SetActive(false);
            }
        }
        hostStarter = GetComponent<HostStarter>();
        clientConnector = GetComponent<ClientConnector>();
    }

    private void Update()
    {
        StartCoroutine(CheckInternet(isConnected =>
        {
            online = isConnected;
        }));
        if (username != null)
        {
            username.text = player.data.username;
        }
        if (profile != null)
        {
            if (player.data.profilePicBase64 != string.Empty && player.ava != null)
            {
                profile.sprite = player.ava;
            }
        }
    }

    public void demodata()
    {
        gameplaysetting.players.Clear();

        player.HexToColor("39b2ff");
        player.Decks = new List<Deck>();
        player.Mat = "Grass";
        player.Decks.Clear();
        player.Decks.Add(new Deck());
        player.roster.Add(new Marksman());
        player.roster.Add(new Ninja());
        player.roster.Add(new Vanguard());
        player.roster.Add(new Warlock());
        player.roster.Add(new Cleric());
        player.roster.Add(new Warlock());
        player.roster.Add(new Marksman());
        player.roster.Add(new Ninja());
        player.roster.Add(new Marksman());
        player.roster[0].alt = "Chocold-Slinger";
        player.roster[3].alt = "Warm Anne";
        player.Decks[0].AddPiecer(player.roster[0]);
        player.Decks[0].AddPiecer(player.roster[1]);
        player.Decks[0].AddPiecer(player.roster[2]);
        player.Decks[0].AddPiecer(player.roster[3]);
        player.Decks[0].AddPiecer(player.roster[4]);
    }

    public void demodataa()
    {
        gameplaysetting.players.Clear();
        Player black = player;
        black.HexToColor("39b2ff");
        black.Decks = new List<Deck>();
        black.Mat = "Grass";
        black.Decks.Clear();
        black.Decks.Add(new Deck());
        black.Decks[0].AddPiecer(new Marksman());
        black.Decks[0].AddPiecer(new Marksman());
        black.Decks[0].AddPiecer(new Ninja());
        black.Decks[0].AddPiecer(new Warlock());
        black.Decks[0].AddPiecer(new Marksman());
        black.Decks[0].AddPiecer(new Ninja());
    }

    public void logout()
    {
        player.data = new PlayerData();
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("password");
        SceneManager.LoadScene("Login");
    }
    public IEnumerator CheckInternet(System.Action<bool> callback)
    {
        UnityWebRequest req = UnityWebRequest.Get("https://www.google.com/generate_204");
        req.timeout = 3;

        yield return req.SendWebRequest();

        bool online =
            req.result == UnityWebRequest.Result.Success &&
            (req.responseCode == 204 || req.responseCode == 200);

        callback(online);
    }
    public void GoToMainMenu()
    {
        Debug.Log("[SceneNavigator] Loading MainMenu scene...");
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void GoToShop()
    {
        Debug.Log("[SceneNavigator] Loading Shop scene...");
        SceneManager.LoadScene("Shop", LoadSceneMode.Single);
    }

    public void GoToGacha()
    {
        Debug.Log("[SceneNavigator] Loading Gacha scene...");
        SceneManager.LoadScene("Gacha", LoadSceneMode.Single);
    }

    public void Play()
    {
        Debug.Log("[SceneNavigator] Loading Lobby scene...");
        SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    public void gotocollection()
    {
        Debug.Log("[SceneNavigator] Loading Collection scene...");
        SceneManager.LoadScene("Collection", LoadSceneMode.Single);
    }

    public Sprite Base64ToSprite(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

}
