using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoardGenerator : NetworkBehaviour
{
    [Header("Controls")]
    public int turn = 0;
    public int turncount = 1;
    public GameObject skipturn;
    public List<Piecer> GY = new List<Piecer>();
    public GameObject Textbubble;
    public TMP_InputField chat;

    [Header("References")]
    public GameObject tilePrefab;
    public Transform boardRef;
    public Player player;
    public Gameplaysetting gameplaysetting;
    public GameObject Protagmat;
    public GameObject Antagmat;
    public GameObject ProtagProfile;
    public GameObject AntagProfile;
    public GameObject VS;
    public Sprite ProtagOff;
    public Sprite AntagOff;
    public Transform turndisplay;
    public SpriteData sprites;

    [Header("Board Settings")]
    public int width = 8;
    public int height = 8;
    public float spacing = 1f;
    

    [Header("Auto Clear Old Tiles")]
    public bool clearOldTiles = true;
    public Tile[,] tiles;

    float timeAccumulator = 0f;
    public int seconds = 0;
    public bool timerunning = false;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            Debug.Log("BoardGenerator: Only the server generates the board.");
            return;
        }
        
        initmationClientRpc();
    }

    void Update()
    {
        if (timerunning)
        {
            timeAccumulator += Time.deltaTime;

            if (timeAccumulator >= 1f)
            {
                seconds++;
                timeAccumulator -= 1f;
            }
        }
            
    }
    void OnChatSubmit(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            Debug.Log("butth");
            ChatServerRpc(NetworkManager.Singleton.LocalClientId, text);
            chat.text = "";
            chat.ActivateInputField();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChatServerRpc(ulong senderId, string message)
    {
        bool who = true;
        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            who = !who;
        }
        ChatClientRpc(NetworkManager.Singleton.LocalClientId, who, message);
    }
    [ClientRpc]
    void ChatClientRpc(ulong senderId, bool whoad, string message)
    {
        if(NetworkManager.Singleton.LocalClientId != senderId)
        {
            whoad = !whoad;
        }
        int who = whoad ? 0 : 1;
        Debug.Log(who);
        Textbubble.gameObject.SetActive(true);
        Textbubble.GetComponent<Image>().color = gameplaysetting.players[who].pColor;
        Textbubble.GetComponentInChildren<TMP_Text>().text = gameplaysetting.players[who].data.username + ": " + message;
        Textbubble.GetComponent<Button>().onClick.RemoveAllListeners();
        Textbubble.GetComponent<Button>().onClick.AddListener(() =>
        {
            Textbubble.gameObject.SetActive(false);
        });
        gameplaysetting.log = gameplaysetting.log + $"[{seconds.ToString()}]" + gameplaysetting.players[who].data.username + " said: " + message + "\n";
    }

    [ClientRpc]
    void skinboardClientRpc()
    {
        for (int i = 0; i < gameplaysetting.players.Count; i++) 
        {
            if (i == 0)
            {
                Protagmat.GetComponent<Image>().sprite = gameplaysetting.players[0].Decks[0].mat.mat[1];
            }
            else if (i == 1)
            {
                Antagmat.GetComponent<Image>().sprite = gameplaysetting.players[1].Decks[0].mat.mat[0];
            }
        }
        foreach (avatar a in sprites.avatars)
        {
            if (a.id.ToString() == gameplaysetting.players[0].data.profilePicBase64)
            {
                ProtagProfile.transform.Find("protag_profile").GetComponent<Image>().sprite = a.avatara;
            }
            if (a.id.ToString() == gameplaysetting.players[1].data.profilePicBase64)
            {
                AntagProfile.transform.Find("antag_profile").GetComponent<Image>().sprite = a.avatara;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void GenboardServerRpc()
    {
        GenerateBoard();
    }

    public void GenerateBoard()
    {
        if (tilePrefab == null || boardRef == null)
        {
            Debug.LogError("Missing tilePrefab or boardRef reference!");
            return;
        }

        Tile.currentlySelected = null;

        if (clearOldTiles)
        {
            for (int i = boardRef.childCount - 1; i >= 0; i--)
                Destroy(boardRef.GetChild(i).gameObject);
        }

        tiles = new Tile[width, height];

        Vector3 startPos = boardRef.position
            - new Vector3((width - 1) * spacing * 0.5f, (height - 1) * spacing * 0.5f, 0f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 tilePos = startPos + new Vector3(x * spacing, y * spacing, 0f);
                GameObject tile = Instantiate(tilePrefab);
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                Material newMat = new Material(sr.sharedMaterial);
                sr.material = newMat;
                tile.transform.position = tilePos;
                tile.name = $"Tile_{x}_{y}";
                tile.GetComponent<Tile>().networkpos.Add(x);
                tile.GetComponent<Tile>().networkpos.Add(y);
                tiles[x, y] = tile.GetComponent<Tile>();
                tile.GetComponent<NetworkObject>().Spawn();
                tile.transform.SetParent(boardRef, true);
                tile.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1); ;
            }
        }
    }

    [ClientRpc]
    void initmationClientRpc()
    {
        Protagmat = GameObject.FindWithTag("protagmat");
        Antagmat = GameObject.FindWithTag("antagmat");
        ProtagOff = ProtagProfile.GetComponent<SpriteRenderer>().sprite;
        AntagOff = AntagProfile.GetComponent<SpriteRenderer>().sprite;
        turndisplay = skipturn.transform.Find("Turn");
        Textbubble.gameObject.SetActive(false);
        gameplaysetting.log = "";
        chat.onSubmit.AddListener(OnChatSubmit);
        ProtagProfile.transform.Find("name/Viewport/Content/protag_name").GetComponent<TMP_Text>().text = gameplaysetting.players[0].data.username;
        AntagProfile.transform.Find("name (1)/Viewport/Content/protag_name").GetComponent<TMP_Text>().text = gameplaysetting.players[1].data.username;
        skinboardClientRpc();
        StartCoroutine(initmation());
        turnmanager();
        skipturn.GetComponent<Button>().onClick.AddListener(TurnManagerServerRpc);
        turndisplay = skipturn.transform.Find("Turn");
    }

    private IEnumerator initmation()
    {
        Animator protagmator = Protagmat.GetComponent<Animator>();
        Animator antagmator = Antagmat.GetComponent<Animator>();
        Animator VSanimator = VS.GetComponent<Animator>();
        protagmator.enabled = true;
        antagmator.enabled = true;
        protagmator.Play("protagboardcolide", 0, 0f);
        antagmator.Play("antagboard", 0, 0f);
        VSanimator.enabled = true;
        yield return null;
        float len = protagmator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(len);
        protagmator.enabled = false;
        antagmator.enabled = false;
        GenboardServerRpc();
        yield return new WaitForSeconds(0.6f);
        VSanimator.enabled = false;
        VS.GetComponent<SpriteRenderer>().sprite = null;
        timerunning = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TurnManagerServerRpc()
    {
        turnmanager();
    }


    void turnmanager()
    {
        turn = (turn == 0) ? 1 : 0;
        if (tiles != null &&
        tiles.GetLength(0) > 0 &&
        tiles.GetLength(1) > 0)
        {
            if (tiles[0, 0].IsSpawned) 
            {
                for (int i = 0; i < tiles.GetLength(0); i++)
                {
                    for(int j = 0; j < tiles.GetLength(1); j++)
                    {
                        tiles[i, j].ResetTileSpellingClientRpc();
                    }
                }
                tiles[0, 0].ResetStatusClientRpc();
                tiles[0, 0].ResetStatusClientRpc();
                turncount++;
            }
        }
        uipClientRpc(turn, turncount, NetworkManager.Singleton.LocalClientId);
    }

    [ClientRpc]
    void uipClientRpc(int turn, int turncount, ulong senderId)
    {
        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            this.turn = (turn == 0) ? 1 : 0;
        }
        StartCoroutine(turnimation());
        turndisplay.GetComponent<TMP_Text>().text = "Turn " + turncount.ToString();
        timeAccumulator = 0f;
        seconds = 0;
    }

    private IEnumerator turnimation()
    {
        Animator animator = null;
        ProtagProfile.GetComponent<SpriteRenderer>().sprite = ProtagOff;
        AntagProfile.GetComponent<SpriteRenderer>().sprite = AntagOff;
        if (turn == 0)
        {
            animator = ProtagProfile.GetComponent<Animator>();
        }
        else if (turn == 1)
        {
            animator = AntagProfile.GetComponent<Animator>();
        }
        animator.enabled = true;
        animator.Play("New Animation", 0, 0f);
        float len = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.enabled = false;
    }

    public Tile GetTile(int x, int y)
    {
        if (tiles == null) return null;
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        return tiles[x, y];
    }
}
