using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class Datamanager : MonoBehaviour
{
    [Header("Managers")]
    public CloudDatabaseManager cloudDB;
    public PlayerDatabase localplayerDB;
    public DeckDatabase decks;
    public InventoryDatabase inventory;
    public CapsuleDatabase capsuleDatabase;
    public List<DeckDataDB> deckList = new List<DeckDataDB>();
    public SpriteData sprites;
    public piercerdatabase piercers;

    [Header("Player")]
    public Player player;

    [Header("UI - Register")]
    public TMP_InputField usernameField;
    public TMP_InputField emailField;
    public TMP_InputField DD;
    public TMP_InputField MM;
    public TMP_InputField YYYY;
    public TMP_InputField passwordField;

    [Header("UI - Login")]
    public TMP_InputField loginUsernameField;
    public TMP_InputField loginPasswordField;

    [Header("Canvases")]
    public Canvas loginCanvas;
    public Canvas registerCanvas;

    [Header("Buttons")]
    public Button registerButton;
    public Button loginButton;
    public Button switchToRegisterButton;
    public Button switchToLoginButton;
    public Button googleLoginButton;

    [Header("Other")]
    public SceneNavigator navigator;

    public bool isLogin = true;

    async void Start()
    {
        cloudDB = GetComponent<CloudDatabaseManager>();
        localplayerDB = new PlayerDatabase();
        decks = new DeckDatabase();
        inventory = new InventoryDatabase();
        capsuleDatabase = new CapsuleDatabase();
        navigator = GetComponent<SceneNavigator>();
        if (PlayerPrefs.HasKey("username") && PlayerPrefs.HasKey("password"))
        { await Login(PlayerPrefs.GetString("username"), PlayerPrefs.GetString("password")); localplayerDB.SetCurrencyAsync(player.data.user_id, 1800, 1000000  );
        }
        registerButton.onClick.AddListener(OnRegisterPressed);
        loginButton.onClick.AddListener(OnLoginPressed);
        switchToLoginButton.onClick.AddListener(() => SwitchUI(true));
        switchToRegisterButton.onClick.AddListener(() => SwitchUI(false));
        googleLoginButton.onClick.AddListener(OnGoogleLoginPressed);

        SwitchUI(isLogin);
    }


    #region UI Actions

    void SwitchUI(bool login)
    {
        isLogin = login;
        loginCanvas.gameObject.SetActive(login);
        registerCanvas.gameObject.SetActive(!login);
    }

    void OnLoginPressed()
    {
        _ = Login(loginUsernameField.text, loginPasswordField.text);
    }

    void OnRegisterPressed()
    {
        _ = Register(
            usernameField.text,
            emailField.text,
            passwordField.text,
            DD.text,
            MM.text,
            YYYY.text
        );
    }

    void OnGoogleLoginPressed()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("Running on Android");
        }
        else if (Application.platform == RuntimePlatform.WindowsPlayer ||
                 Application.platform == RuntimePlatform.OSXPlayer ||
                 Application.platform == RuntimePlatform.LinuxPlayer ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
            Debug.Log("Running on PC");
            cloudDB.StartListener();
            cloudDB.OpenGoogleLogin();
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(cloudDB.sub))
        {
            _ = ggLogin(cloudDB.sub);
        }
    }

     #endregion
     #region Logic

    async Task Login(string username, string password)
    {
        PlayerDataDB savedplayer = await localplayerDB.GetPlayerByLoginAsync(username, password);
        if (savedplayer == null) return;
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetString("password", password);
        PlayerPrefs.Save();
        player.data.username = savedplayer.username;
        player.data.password = savedplayer.password;
        player.data.email = savedplayer.email;
        player.data.password = savedplayer.password;
        player.data.createwhen = savedplayer.createwhen;
        player.data.user_id = savedplayer.user_id;
        player.data.virgin = savedplayer.virgin;
        player.data.rating = savedplayer.rating;
        player.data.rankpoint = savedplayer.rankpoint;
        player.data.fragments = savedplayer.fragments;
        player.data.gems = savedplayer.gems;
        player.data.admin = savedplayer.admin;
        player.data.profilePicBase64 = savedplayer.profilePicBase64;
        PopulateDecks();

        if (player.data.admin)
        {
           SceneManager.LoadScene("admin");
        }
        else
        {
           SceneManager.LoadScene("MainMenu");
        }
       
    }

    async Task ggLogin(string sub)
    {
        PlayerDataDB savedplayer = await localplayerDB.GetPlayerByggLoginAsync(sub);
        cloudDB.sub = null;
        if (savedplayer == null) return;
        PlayerPrefs.SetString("username", savedplayer.username);
        PlayerPrefs.SetString("password", savedplayer.password);
        PlayerPrefs.Save();
        player.data.username = savedplayer.username;
        player.data.password = savedplayer.password;
        player.data.email = savedplayer.email;
        player.data.password = savedplayer.password;
        player.data.createwhen = savedplayer.createwhen;
        player.data.user_id = savedplayer.user_id;
        player.data.virgin = savedplayer.virgin;
        player.data.rating = savedplayer.rating;
        player.data.rankpoint = savedplayer.rankpoint;
        player.data.fragments = savedplayer.fragments;
        player.data.gems = savedplayer.gems;
        player.data.admin = savedplayer.admin;
        player.data.profilePicBase64 = savedplayer.profilePicBase64;
        PopulateDecks();

        if (player.data.admin)
        {
            SceneManager.LoadScene("admin");
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }

    }

    async Task Register(string username, string email, string password, string DD, string MM, string YYYY)
    {
        string bday = $"{DD}-{MM}-{YYYY}";
        string userId = Guid.NewGuid().ToString();
        PlayerDataDB playerdata = new PlayerDataDB();
        playerdata.user_id = userId;
        playerdata.username = username;
        playerdata.email = email;
        playerdata.password = password;
        playerdata.bday = bday;
        playerdata.virgin = true;
        bool success = await localplayerDB.SavePlayerAsync(playerdata);
        if (!success) return;
        SwitchUI(true);
    }
    #endregion

    public void PopulateDecks()
    {
        deckList = decks.GetPlayerDecks(player.data.user_id);
        player.Decks.Clear();
        foreach (Transform child in transform)
        {
            if (child.name != "AddDeck")
            {
                Destroy(child.gameObject);
            }
        }

        // Instantiate a DeckPrefab for each deck in the player's deck list
        foreach (var deck in deckList)
        {
            Deck tempdeck = new Deck();
            if (deck.mat_id == 21000932)
            {
                foreach (Mat mat in sprites.mats)
                {
                    if (1612 == mat.id)
                    {
                        tempdeck.mat = mat;
                        break;
                    }
                }
            }
            else
            {
                if(deck.mat_id != 0)
                {
                    InventoryDataDB itr = inventory.GetInventoryItem(deck.mat_id);
                    CapsuleDataDB capp = capsuleDatabase.GetCapsule(itr.capsuleid);
                    foreach (Mat mat in sprites.mats)
                    {
                        if (capp.itemid == mat.id)
                        {
                            tempdeck.mat = mat;
                            break;
                        }
                    }
                }
            }
            List<CapsuleDataDB> list = new List<CapsuleDataDB>();
            for (int i = 1; i <= 5; i++)
            {
                int who = 0;
                if (i == 1) { who = deck.slot1; }
                else if (i == 2) { who = deck.slot2; }
                else if (i == 3) { who = deck.slot3; }
                else if (i == 4) { who = deck.slot4; }
                else if (i == 5) { who = deck.slot5; }
                if (who != 0)
                {
                    InventoryDataDB item = inventory.GetInventoryItem(who);
                    CapsuleDataDB cap = capsuleDatabase.GetCapsule(item.capsuleid);
                    list.Add(cap);
                }
            }
            foreach (CapsuleDataDB cap in list)
            {
                foreach (Piecer a in piercers.roster)
                {
                    if (cap.itemid == a.ID)
                    {
                        Piecer temp = a.Clone();
                        foreach (visuals d in sprites.spdt)
                        {
                            if (a.ID == d.id && cap.variant == d.variant)
                            {
                                temp.Skin = d.sprites;
                            }
                        }
                        temp.alt = cap.variant;
                        tempdeck.AddPiecer(temp);
                        break;
                    }
                }
            }
            tempdeck.name = deck.name;
            tempdeck.deckid = deck.deck_id;
            tempdeck.maindeck = deck.maindeck;
            player.Decks.Add(tempdeck);
        }

        int foundyoun = 0;

        for (int i = 0; i < player.Decks.Count; i++)
        {
            if (player.Decks[0].maindeck)
            {
                foundyoun = i;
                Debug.Log(foundyoun);
            }
        }

        for (int i = foundyoun; i < player.Decks.Count - 1; i++)
        {
            Deck temp = player.Decks[i];
            player.Decks[i] = player.Decks[i + 1];
            player.Decks[i + 1] = temp;
        }
    }
}