using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class Lobbysc : NetworkBehaviour
{
    public GameObject lobbyprefab;
    public HostStarter? hostStarter;
    public ClientConnector? clientConnector;
    public List<LobbyInfo> currentdisplay = new List<LobbyInfo>();
    public LobbyInfo selecteced;
    public TMP_InputField roomname;
    public GameObject hostbutton;
    public GameObject playbutton;
    public GameObject refreshbutton;
    public Player player;
    public Sprite basetexture;
    public GameObject hatch;
    public SpriteRenderer youImg;
    public SpriteRenderer oppImg;
    public Gameplaysetting gameplaysetting;
    public piercerdatabase pcdtb;
    public GameObject protagready;
    public GameObject antagready;
    public bool closed = false;
    public bool youready = false;
    public bool oppsready = false;
    public Animator you;
    public Animator opp;
    public SpriteData sprites;
    public async void Start()
    {
        Transform you = hatch.transform.Find("asc/RawImage");
        Transform opps = hatch.transform.Find("asc (1)/RawImage");

        youImg = you.GetComponent<SpriteRenderer>();
        oppImg = opps.GetComponent<SpriteRenderer>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        hostStarter = GetComponent<HostStarter>();
        clientConnector = GetComponent<ClientConnector>();

        Debug.Log(hostbutton.name);

        StartCoroutine(PollLobbies());

        refreshbutton.GetComponent<Button>().onClick.AddListener(() => {
            _ = RefreshLobbyList();
        });

        hostbutton.GetComponent<Button>().onClick.AddListener(() => {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            hostbutton.transform.Find("RawImage").GetComponent<RawImage>().color = new Color(1f, 0.725f, 0f, 1f);
            hostbutton.GetComponentInChildren<TMP_Text>().text = "...";
            hostbutton.GetComponent<Button>().enabled = false;
            hostStarter.CreateLobby(player.data.username, roomname.text);
        });

        playbutton.GetComponent<Button>().onClick.AddListener(() => 
        {
            StartCoroutine(fight());
        });
    }

    public IEnumerator fight()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnJoinedAsClient;
        Animator animator = hatch.GetComponent<Animator>();
        if (!closed)
        {
            animator.Play("closes");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            closed = true;
            if (!IsConnectedToHost())
            {
                clientConnector.ConnectToHost();
            }
            else
            {
                youready = true;
            }
        }
        else
        {
            animator.Play("open");
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
            closed = false;
            if (NetworkManager.Singleton.LocalClientId != 0)
            {
                NetworkManager.Singleton.Shutdown();
                oppsready = false;
            }
        }
    }

    public void connecttohost()
    {
        if(roomname.text != null || roomname.text != "")
        {
            clientConnector.relayJoinCode = roomname.text;
            if (!IsConnectedToHost())
            {
                clientConnector.ConnectToHost();
            }
        }
    }

    bool IsConnectedToHost()
    {
        var nm = NetworkManager.Singleton;
        return nm.IsClient || nm.IsServer || nm.IsHost;
    }


    private void Update()
    {
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("joincode"))
        {
            if (hostStarter.hosting != null) 
            {
                if(o.name == "Text (TMP)")
                {
                    o.GetComponent<TMP_Text>().text = hostStarter.hosting.LobbyCode;
                }
                o.GetComponent<TMP_Text>().color = new Color32(255, 255, 255, 255);
            }
            else
            {
                o.GetComponent<TMP_Text>().color = new Color32(255, 255, 255, 0);
            }
        }
        if (!string.IsNullOrEmpty(clientConnector.currentlobby.lobbyId))
            selecteced = clientConnector.currentlobby;
        if(hostStarter.hosting != null || !string.IsNullOrEmpty(selecteced.lobbyId))
        {
            hostStarter.yourImage = player.data.profilePicBase64;
            if (string.IsNullOrEmpty(hostStarter.yourImage))
            {
                youImg.sprite = basetexture;
            }
            else
            {
                foreach(avatar h in sprites.avatars)
                {
                    if(hostStarter.yourImage == h.id.ToString())
                    {
                        youImg.sprite = h.avatara;
                    }
                }
            }
            Color c = youImg.color;
            c.a = 1;
            youImg.color = c;
            protagready.GetComponent<SpriteRenderer>().color = c;
        }
        else
        {
            Color c = youImg.color;
            c.a = 0;
            youImg.color = c;
            protagready.GetComponent<SpriteRenderer>().color = c;
        }
        if (!string.IsNullOrEmpty(selecteced.lobbyId) || !string.IsNullOrEmpty(hostStarter.playerName))
        {
            if (string.IsNullOrEmpty(selecteced.hostimage) || !string.IsNullOrEmpty(hostStarter.playerName))
            {
                oppImg.sprite = basetexture;
            }
            else
            {
                foreach (avatar h in sprites.avatars)
                {
                    if (selecteced.hostimage == h.id.ToString())
                    {
                        oppImg.sprite = h.avatara;
                    }
                }
            }
            Color c = oppImg.color;
            c.a = 1;
            oppImg.color = c;
            antagready.GetComponent<SpriteRenderer>().color = c;
        }
        else
        {
            Color c = oppImg.color;
            c.a = 0;
            oppImg.color = c;
            antagready.GetComponent<SpriteRenderer>().color = c;
        }
        if (oppsready)
        {
            you.speed = 4f;
            opp.speed = 4f;
        }
        else
        {
            you.speed = 1f;
            opp.speed = 1f;
        }
        if (youready && oppsready)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
            youready = false;
            oppsready = false;
        }
    }

    public IEnumerator PollLobbies()
    {
        while (true)
        {
            _ = RefreshLobbyList();
            yield return new WaitForSeconds(3f);
        }
    }


    public async Task RefreshLobbyList()
    {
        var newList = await clientConnector.BrowseLobbies();

        if (!LobbyListsEqual(newList, currentdisplay))
        {
            RefreshUI(newList);
        }

        hostStarter.RefreshLobbyInfo();
    }

    public bool LobbyListsEqual(List<LobbyInfo> a, List<LobbyInfo> b)
    {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].lobbyId != b[i].lobbyId ||
                a[i].lobbyName != b[i].lobbyName ||
                a[i].hostName != b[i].hostName ||
                a[i].playerCount != b[i].playerCount)
            {
                return false;
            }
        }
        return true;
    }

    public void RefreshUI(List<LobbyInfo> lobbies)
    {
        string currentFilter = roomname.text;

        foreach (GameObject o in GameObject.FindGameObjectsWithTag("lobby"))
        {
            Destroy(o);
        }

        currentdisplay.Clear();

        foreach (var lobby in lobbies)
        {
            

            currentdisplay.Add(lobby);

            GameObject obj = Instantiate(lobbyprefab, transform);
            obj.GetComponent<lobbyd>().lobby = lobby;
            obj.GetComponent<lobbyd>().basetexture = basetexture;
            foreach (avatar h in sprites.avatars)
            {
                if (h.id.ToString() == lobby.hostimage)
                {
                    obj.GetComponent<lobbyd>().a = h.avatara;
                }
            }
            obj.GetComponent<lobbyd>().currentFilter = currentFilter;
            obj.GetComponent<lobbyd>().ass = GetComponent<Lobbysc>();
        }
    }



    public string SpriteToBase64(Sprite sprite)
    {
        Texture2D readable = MakeTextureReadable(sprite.texture);
        byte[] png = readable.EncodeToPNG();
        return Convert.ToBase64String(png);
    }

    public Sprite Base64ToSprite(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    public void OnJoinedAsClient(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            populate();
            Debug.Log("Client successfully connected, will run PlayerAdd after delay...");
            NetworkManager.Singleton.OnClientConnectedCallback -= OnJoinedAsClient;
        }
    }

    public void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            populate();
            oppsready = true;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
        else
        {
            hostbutton.GetComponent<Button>().onClick.RemoveAllListeners();
            hostbutton.transform.Find("RawImage").GetComponent<RawImage>().color = new Color(0f, 0.988f, 1f, 1f);
            hostbutton.GetComponentInChildren<TMP_Text>().text = "Hosting";
            hostbutton.GetComponent<Button>().enabled = true;
            hostbutton.GetComponent<Button>().onClick.AddListener(() => {
                hostbutton.GetComponent<Button>().onClick.RemoveAllListeners();
                hostbutton.transform.Find("RawImage").GetComponent<RawImage>().color = new Color(0.396f, 1f, 0f, 1f);
                hostbutton.GetComponentInChildren<TMP_Text>().text = "Host";
                hostStarter.ShutdownHost();
                hostbutton.GetComponent<Button>().onClick.AddListener(() => {
                    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                    hostbutton.transform.Find("RawImage").GetComponent<RawImage>().color = new Color(1f, 0.725f, 0f, 1f);
                    hostbutton.GetComponentInChildren<TMP_Text>().text = "...";
                    hostbutton.GetComponent<Button>().enabled = false;
                    hostStarter.CreateLobby(player.data.username, roomname.text);
                });
            });
        }
    }

    void populate()
    {
        if (player == null)
        {
            Debug.LogError("populate(): Player is null");
            return;
        }

        if (player.Decks == null || player.Decks.Count == 0)
        {
            Debug.LogError("populate(): Player has no decks when called.");
            return;
        }

        if (player.Decks[0].Piecers == null)
        {
            Debug.LogError("populate(): Piecer list is null.");
            return;
        }

        gameplaysetting.players.Clear();
        gameplaysetting.players.Add(player);

        if (IsClient && !IsHost)
        {
            SendPlayerToServer();
        }

        else if (IsHost)
        {
            SendPlayerToClients();
        }
    }
    public static Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            source.width, source.height, 0,
            RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D newTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        newTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        newTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return newTex;
    }

    void SendPlayerToServer()
    {
        PlayerData tempdata = new PlayerData();
        tempdata.user_id = player.data.user_id;
        tempdata.username = player.data.username;
        senddataServerRpc(tempdata, player.Decks[0].mat.name, player.data.profilePicBase64);

        foreach (Piecer a in player.Decks[0].Piecers)
            sendunitServerRpc(a.ID, a.alt, a.effect.ID);
    }

    void SendPlayerToClients()
    {
        ulong id = NetworkManager.Singleton.LocalClientId;
        PlayerData tempdata = new PlayerData();
        tempdata.user_id = player.data.user_id;
        tempdata.username = player.data.username;
        senddataClientRpc(id, tempdata, player.Decks[0].mat.name, player.data.profilePicBase64);

        foreach (Piecer a in player.Decks[0].Piecers)
            sendunitClientRpc(id, a.ID, a.alt, a.effect.ID);
    }



    [ServerRpc(RequireOwnership = false)]
    void senddataServerRpc(PlayerData playderinfo, string mat, string p)
    {
        Player black = ScriptableObject.CreateInstance<Player>();
        black.HexToColor("f90057");
        black.data = playderinfo;
        black.Decks = new List<Deck>() { new Deck() };
        foreach(Mat a in sprites.mats)
        {
            if(a.name == mat)
            {
                black.Decks[0].mat = a;
            }
        }
        black.data.profilePicBase64 = p;
        gameplaysetting.players.Add(black);
    }


    [ServerRpc(RequireOwnership = false)]
    void sendunitServerRpc(int piercerid, string pierceralt, int effectid)
    {
        if (gameplaysetting.players.Count < 2)
        {
            Debug.LogWarning("sendunitServerRpc(): No second player yet.");
            return;
        }

        Player p = gameplaysetting.players[1];

        if (p.Decks.Count == 0)
            p.Decks.Add(new Deck());

        foreach (Piecer a in pcdtb.roster)
        {
            if (a.ID == piercerid)
            {
                Piecer newp = a.Clone();
                newp.alt = pierceralt;
                p.Decks[0].AddPiecer(newp);
                return;
            }
        }
    }


    [ClientRpc]
    void senddataClientRpc(ulong senderId, PlayerData playderinfo, string mat, string p)
    {
        if (NetworkManager.Singleton.LocalClientId == senderId)
            return;

        Player black = ScriptableObject.CreateInstance<Player>();
        black.HexToColor("f90057");
        black.data = playderinfo;
        black.Decks = new List<Deck>() { new Deck() };
        foreach (Mat a in sprites.mats)
        {
            if (a.name == mat)
            {
                black.Decks[0].mat = a;
            }
        }
        black.data.profilePicBase64 = p;

        gameplaysetting.players.Add(black);
    }


    [ClientRpc]
    void sendunitClientRpc(ulong senderId, int piercerid, string pierceralt, int effectid)
    {
        if (NetworkManager.Singleton.LocalClientId == senderId)
            return;

        if (gameplaysetting.players.Count < 2)
        {
            Debug.LogWarning("sendunitClientRpc(): No second player yet.");
            return;
        }

        Player p = gameplaysetting.players[1];

        if (p.Decks.Count == 0)
            p.Decks.Add(new Deck());

        foreach (Piecer a in pcdtb.roster)
        {
            if (a.ID == piercerid)
            {
                Piecer newp = a.Clone();
                newp.alt = pierceralt;
                p.Decks[0].AddPiecer(newp);
                return;
            }
        }
    }
}
