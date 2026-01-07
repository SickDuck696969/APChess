using NUnit.Framework;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;


[RequireComponent(typeof(SpriteRenderer))]
public class Tile : NetworkBehaviour, IPointerDownHandler
{
    private SpriteRenderer sr;
    private Color baseColor;
    private float brightnessBoost = 1.3f;
    private float saturationBoost = 1.2f;
    public int[] pos;
    public NetworkList<int> networkpos = new NetworkList<int>(
    readPerm: NetworkVariableReadPermission.Everyone,
    writePerm: NetworkVariableWritePermission.Server
    );
    public SpriteData spritedatabase;

    public Piecer piecer;
    public GameObject infocardprefab;
    public GameObject boardgenerateor;

    private GameObject currentInfocard;

    public Color highlightColor = Color.yellow;
    public static Tile currentlySelected;
    public static Boolean hasMoved = false;
    public static Boolean hasSpelled = false;
    public static Boolean animovin = false;
    public Boolean isSmhmoved = true;
    public Boolean isSpelling = false;
    public GameObject overlay;
    static Vector3 save;
    public Animator animator;
    public AnimationClip clip = null;
    List<EffectStatus> effectStatuses = new List<EffectStatus>();
    public NetworkVariable<FixedString64Bytes> skinPath = new NetworkVariable<FixedString64Bytes>();
    public int id;
    public piercerdatabase pcdtb;
    public static Tile movingstandby;
    public Gameplaysetting gameplaysetting;
    public static Tile beingattacked;
    public bool withinrange = false;
    public string range = "1 diagonal";
    public bool king = false;
    public bool objection = false;
    public Coroutine animationruuning;
    public TMP_Text timer;
    public MatchDatabase matchbase;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public override void OnNetworkSpawn()
    {
        matchbase = new MatchDatabase();
        boardgenerateor = GameObject.FindGameObjectWithTag("GameController");
        baseColor = sr.color;
        animator = GetComponent<Animator>();
        animator.enabled = false;
        overlay.GetComponent<Animator>().enabled = false;
        if (networkpos.Count >= 2)
        {
            pos = new int[] { networkpos[0], networkpos[1] };
        }
        else
        {
            pos = new int[2] { 0, 0 };
            Debug.LogWarning("networkpos not populated yet on spawn!");
        }
        if (pos[1] == 1)
        {
            piecer = gameplaysetting.players[0].Decks[0].Piecers[pos[0]];
            piecer.color = gameplaysetting.players[0].pColor;
            piecer.tile = this;
            piecer.effect.caster = piecer;
            foreach (visuals a in spritedatabase.spdt)
            {
                if (a.id == piecer.ID && a.variant == piecer.alt)
                {
                    piecer.Skin = a.sprites;
                }
            }
        }
        else if (pos[1] == 6)
        {
            piecer = gameplaysetting.players[1].Decks[0].Piecers[4-pos[0]];
            piecer.color = gameplaysetting.players[1].pColor;
            piecer.tile = this;
            piecer.effect.caster = piecer;
            foreach (visuals a in spritedatabase.spdt)
            {
                if (a.id == piecer.ID && a.variant == piecer.alt)
                {
                    piecer.Skin = a.sprites;
                }
            }
        }
        else if(pos[1] == 7 || pos[1] == 0)
        {
            if (pos[0] == 2)
            {
                piecer = new Avatar();
                king = true;
                if (pos[1] == 7) king = false;
            }
            foreach (visuals a in spritedatabase.spdt)
            {
                if (a.id == piecer.ID && a.variant == piecer.alt)
                {
                    piecer.Skin = a.sprites;
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void declareServerRpc(ulong senderId, int winner)
    {
        // winner: 0 = sender wins, 1 = opponent wins
        // sender sees themselves as 0

        ulong hostId = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
        ulong clientId = NetworkManager.Singleton.ConnectedClientsList[1].ClientId;

        ulong actualWinner;
        ulong actualLoser;

        if (winner == 0) // sender says they win
        {
            actualWinner = senderId;
            actualLoser = (senderId == hostId ? clientId : hostId);
        }
        else // sender says the opponent wins
        {
            actualLoser = senderId;
            actualWinner = (senderId == hostId ? clientId : hostId);
        }

        // Notify both clients correctly
        declareClientRpc(actualWinner, actualLoser);
    }
    [ClientRpc]
    void declareClientRpc(ulong winnerId, ulong loserId)
    {
        ulong local = NetworkManager.Singleton.LocalClientId;

        if (local == winnerId)
        {
            ShowLoseScreen();
            Debug.Log("YOU LOSE");
        }
        else
        {
            ShowWinScreen();
            Debug.Log("YOU WIN");
        }
    }

    private void Update()
    {
        if (beingattacked != null || beingattacked != this)
        {
            objection = false;
        }
        if (king)
        {
            if(piecer == null)
            {
                Time.timeScale = 0f;
                int who = 0;
                if (pos[1] == 0)
                {
                    who = 0;
                }else if (pos[1] == 7)
                {
                    who = 1;
                }
                declareServerRpc(NetworkManager.Singleton.LocalClientId, who);
            }
        }
        GameObject.FindGameObjectWithTag("timer").GetComponent<TMP_Text>().text = boardgenerateor.GetComponent<BoardGenerator>().seconds.ToString();
        if(boardgenerateor.GetComponent<BoardGenerator>().seconds == 120)
        {
            hasMoved = true;
            hasSpelled = true;
            boardgenerateor.GetComponent<BoardGenerator>().TurnManagerServerRpc();
        }
        if (hasMoved && hasSpelled)
        {
            boardgenerateor.GetComponent<BoardGenerator>().skipturn.GetComponent<RawImage>().color = Color.red;
        }
        else
        {
            boardgenerateor.GetComponent<BoardGenerator>().skipturn.GetComponent<RawImage>().color = Color.black;
        }
        if(boardgenerateor.GetComponent<BoardGenerator>().turn != 0)
        {
            boardgenerateor.GetComponent<BoardGenerator>().skipturn.GetComponent<UnityEngine.UI.Button>().enabled = false;
        }else
        {
            boardgenerateor.GetComponent<BoardGenerator>().skipturn.GetComponent<UnityEngine.UI.Button>().enabled = true;
        }

        if (GetComponent<RectTransform>().localScale != new Vector3(1, 1, 1))
        {
            GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
        if (movingstandby != null && !isSmhmoved)
        {
            HideInfoCard();
            if (movingstandby.clip != null && movingstandby.isSmhmoved)
            {
                runanim();
            }
            else
            {
                movingstandby.isSmhmoved = false;
            }
        }
        if (movingstandby != null && movingstandby.transform.position != transform.position && !isSmhmoved && !movingstandby.isSmhmoved)
        {
            if (movingstandby.clip != null)
            {
                if (animovin)
                {
                    piecer = movingstandby.piecer;
                    Color c = sr.color;
                    c.a = 0;
                    sr.color = c;
                    movingstandby.HideInfoCard();
                    ShowInfoCard();
                    movingstandby.transform.position = Vector3.Lerp(
                        movingstandby.transform.position,
                        transform.position,
                        Time.deltaTime * 10f
                    );
                }
            }
            else
            {
                piecer = movingstandby.piecer;
                Color c = sr.color;
                c.a = 0;
                sr.color = c;
                movingstandby.HideInfoCard();
                ShowInfoCard();
                movingstandby.transform.position = Vector3.Lerp(
                    movingstandby.transform.position,
                    transform.position,
                    Time.deltaTime * 10f
                );
            }
            if (movingstandby.transform.position == transform.position)
            {
                isSmhmoved = true;
                if (movingstandby.isSmhmoved)
                {
                    Debug.Log("extramove");
                    if (movingstandby.piecer != null)
                    {
                        movingstandby.piecer.xtramove = false;
                    }
                    movingstandby.piecer = null;
                    movingstandby.sr.sprite = null;
                    Color c = sr.color;
                    c.a = 1;
                    sr.color = c;
                    movingstandby.transform.position = save;
                    movingstandby = null;
                    Highlight();
                    hasMoved = true;
                }
                else if (!movingstandby.isSmhmoved)
                {
                    Debug.Log("move");
                    overlay.GetComponent<Animator>().enabled = false;
                    overlay.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
                    if (movingstandby.piecer != null)
                    {
                        movingstandby.piecer.xtramove = false;
                    }
;                   movingstandby.piecer = null;
                    movingstandby.sr.sprite = null;
                    Color c = sr.color;
                    c.a = 1;
                    sr.color = c;
                    movingstandby.isSmhmoved = true;
                    currentlySelected = this;
                    movingstandby.transform.position = save;
                    movingstandby = null;
                    Highlight();
                    hasMoved = true;
                    isSmhmoved = true;
                }
            }
        }
        if (currentlySelected != this)
        {
            Deselect();
        }
        if (piecer != null)
        {
            piecer.effect.caster = piecer;
            piecer.tile = GetComponent<Tile>();
            Debug.Log($"piecer: {piecer.name}, [{pos[0]}, {pos[1]}], effect {piecer.effect.caster.tile.pos[0]}, [{piecer.effect.caster.tile.pos[0]}]");
            if (piecer.Skin[0] != null) sr.sprite = piecer.Skin[0];
            sr.material.SetColor("_Color", piecer.color);
            overlay.GetComponent<SpriteRenderer>().sprite = null;
            piecer.effect.actioncheck();
            if(gameplaysetting.GY.Count > 0)
            {
                for(int i = 0; i <  gameplaysetting.GY.Count; i++)
                {
                    gameplaysetting.GY[i].effect.actioncheck();
                }
            }
            if(piecer.effectStatuses.Count > 0)
            {
                for(int i = 0; i < piecer.effectStatuses.Count; i++)
                {
                    piecer.effectStatuses[i].actioncheck();
                }
            }
            if (currentlySelected != null && currentlySelected.isSmhmoved && currentlySelected.piecer.color == gameplaysetting.players[0].pColor && boardgenerateor.GetComponent<BoardGenerator>().turn == 0) 
            {
                bool yesyoucanlookatit = false;
                if (currentlySelected.isSpelling)
                {
                    if (currentlySelected.piecer.effect.friendly)
                    {
                        yesyoucanlookatit = true;
                    }
                    else
                    {
                        if (currentlySelected.piecer.color != piecer.color)
                        {
                            yesyoucanlookatit = true;
                        }
                    }
                }
                else if (!hasMoved || currentlySelected.piecer.xtramove)
                {
                    if (currentlySelected.piecer.color != piecer.color)
                    {
                        yesyoucanlookatit = true;
                    }
                }
                else
                {
                    withinrange = false;
                }
                if (yesyoucanlookatit)
                {
                    string[] parts = currentlySelected.range.Split(' ');

                    int howfar = int.Parse(parts[0]);
                    string type = parts[1];

                    switch (type)
                    {
                        case "diagonal":
                            if (Math.Abs(currentlySelected.pos[0] - pos[0]) == howfar &&
                                    Math.Abs(currentlySelected.pos[1] - pos[1]) == howfar)
                            {
                                withinrange = true;
                            }
                            else
                            {
                                withinrange = false;
                            }
                            break;
                        case "radius":
                            int dx = Mathf.Abs(currentlySelected.pos[0] - pos[0]);
                            int dy = Mathf.Abs(currentlySelected.pos[1] - pos[1]);

                            if (dx + dy <= howfar)
                            {
                                withinrange = true;
                            }
                            else
                            {
                                withinrange = false;
                            }
                            break;
                        case "fullboard":
                            withinrange = true;
                            break;
                    }
                }
            }
            else
            {
                withinrange = false;
            }
            if (withinrange)
            {
                overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("dqw");
            }
            else
            {
                overlay.GetComponent<SpriteRenderer>().sprite = null;
            }
        }
        else
        {
            sr.sprite = null;
            overlay.GetComponent<SpriteRenderer>().sprite = null;
            if (currentlySelected != null && currentlySelected.sr.sprite != null && currentlySelected.piecer.color == gameplaysetting.players[0].pColor && boardgenerateor.GetComponent<BoardGenerator>().turn == 0)
            {
                if (!hasMoved || currentlySelected.piecer.xtramove)
                {
                    if (currentlySelected.pos[0] == pos[0] - 1 && currentlySelected.pos[1] == pos[1])
                    {
                        if (currentlySelected.isSmhmoved && isSmhmoved)
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("asc");
                        }
                        else
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = null;
                        }
                    }
                    else if (currentlySelected.pos[0] == pos[0] + 1 && currentlySelected.pos[1] == pos[1])
                    {
                        if (currentlySelected.isSmhmoved && isSmhmoved)
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("asc");
                        }
                        else
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = null;
                        }
                    }
                    else if (currentlySelected.pos[1] == pos[1] + 1 && currentlySelected.pos[0] == pos[0])
                    {
                        if (currentlySelected.isSmhmoved && isSmhmoved)
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("asc");
                        }
                        else
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = null;
                        }
                    }
                    else if (currentlySelected.pos[1] == pos[1] - 1 && currentlySelected.pos[0] == pos[0])
                    {
                        if (currentlySelected.isSmhmoved && isSmhmoved)
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("asc");
                        }
                        else
                        {
                            overlay.GetComponent<SpriteRenderer>().sprite = null;
                        }
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void MoveServerRpc(ulong senderId, int x, int y, int desx, int desy, bool hasclip, bool kill)
    {
        Debug.Log($"sa: {hasclip}");
        bool who = true;
        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            x = 4 - x;
            y = 7 - y;
            desx = 4 - desx;
            desy = 7 - desy;
            who = !who;
        }
        foreach (Tile t in boardgenerateor.GetComponent<BoardGenerator>().tiles)
        {
            if (t.pos[0] == x && t.pos[1] == y)
            {
                save = t.transform.position;
                movingstandby = t;
                Debug.Log($"[SetMovingClientRpc] movingstandby set to {this.name}, saved position = {save}");
                gameplaysetting.log = gameplaysetting.log + $"[{boardgenerateor.GetComponent<BoardGenerator>().seconds.ToString()}]" + $"{gameplaysetting.players[who ? 0 : 1].data.username}'s" + $"{t.piecer.name}[{x}][{y}]";
            }
        }
        foreach (Tile t in boardgenerateor.GetComponent<BoardGenerator>().tiles)
        {
            if (t.pos[0] == desx && t.pos[1] == desy)
            {
                if (kill)
                {
                    gameplaysetting.GY.Add(t.piecer);
                    t.piecer = null;
                    foreach (Piecer a in gameplaysetting.GY)
                    {
                        Debug.Log(a.name);
                    }
                    gameplaysetting.log = gameplaysetting.log + "kills" + $"{gameplaysetting.players[!who ? 0 : 1].data.username}'s" + $" {t.piecer.name}[{desx}][{desy}] " + "and";
                }
                t.isSmhmoved = false;
                beingattacked = null;
                gameplaysetting.log = gameplaysetting.log + "->moves to" + $"[{desx}][{desy}]" + "\n";
            }
        }
        who = !who;
        foreach (Tile t in boardgenerateor.GetComponent<BoardGenerator>().tiles)
        {
            if (t.pos[0] == 4 - x && t.pos[1] == 7 - y)
            {
                Debug.Log($"{t.pos[0]}, {t.pos[1]}");
                t.SetMovingClientRpc(NetworkManager.Singleton.LocalClientId, hasclip, who ? 0 : 1);
            }
        }
        foreach (Tile t in boardgenerateor.GetComponent<BoardGenerator>().tiles)
        {
            if (t.pos[0] == 4 - desx && t.pos[1] == 7 - desy)
            {
                Debug.Log($"{t.pos[0]}, {t.pos[1]}");
                t.SetmovedClientRpc(NetworkManager.Singleton.LocalClientId, kill, !who ? 0 : 1);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void spellServerRpc(ulong senderId, int x, int y, int desx, int desy, string nature)
    {
        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            x = 4 - x;
            y = 7 - y;
            desx = 4 - desx;
            desy = 7 - desy;
        }
        foreach (Tile t in boardgenerateor.GetComponent<BoardGenerator>().tiles)
        {
            if(t.pos[0] == x && t.pos[1] == y)
            {
                t.isSpelling = true;
            }
            if(t.pos[0] == desx && t.pos[1] == desy)
            {
                movingstandby = t;
            }
        }
        foreach (Tile t in boardgenerateor.GetComponent<BoardGenerator>().tiles)
        {
            if (t.pos[0] == 4 - x && t.pos[1] == 7 - y)
            {
                t.setspellClientRpc(NetworkManager.Singleton.LocalClientId);
            }
            if (t.pos[0] == 4 - desx && t.pos[1] == 7 - desy)
            {
                foreach (Tile ex in boardgenerateor.GetComponent<BoardGenerator>().tiles)
                {
                    t.spellClientRpc(NetworkManager.Singleton.LocalClientId);
                }
            }
        }
    }

    [ClientRpc]
    void setspellClientRpc(ulong senderId)
    {
        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            isSpelling = true;
        }
    }

    [ClientRpc]
    void spellClientRpc(ulong senderId)
    {
        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            movingstandby = this;
        }
    }

    [ClientRpc]
    void SetMovingClientRpc(ulong senderId, bool hasclip, int who)
    {
        Debug.Log($"[SetMovingClientRpc] Called. senderId={senderId}, localClientId={NetworkManager.Singleton.LocalClientId}");

        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            save = this.transform.position;
            movingstandby = this;

            Debug.Log($"[SetMovingClientRpc] movingstandby set to {this.name}, saved position = {save}");
            gameplaysetting.log = gameplaysetting.log + $"[{boardgenerateor.GetComponent<BoardGenerator>().seconds.ToString()}]" + $"{gameplaysetting.players[who].data.username}'s" + $"{this.piecer.name}[{this.pos[0]}][{this.pos[1]}]";
        }
        else
        {
            Debug.Log("[SetMovingClientRpc] Ignored because sender matches local client.");
        }
    }

    [ClientRpc]
    void SetmovedClientRpc(ulong senderId, bool kill, int who)
    {
        Debug.Log($"[SetmovedClientRpc] Called. senderId={senderId}, localClientId={NetworkManager.Singleton.LocalClientId}");

        if (NetworkManager.Singleton.LocalClientId != senderId)
        {
            if (kill)
            {
                gameplaysetting.GY.Add(piecer);
                piecer = null;
                foreach (Piecer a in gameplaysetting.GY)
                {
                    Debug.Log(a.name);
                }
                gameplaysetting.log = gameplaysetting.log + "kills" + $"{gameplaysetting.players[who].data.username}'s" + $"{this.piecer.name}[{this.pos[0]}][{this.pos[1]}]" + "and";
            }
            isSmhmoved = false;
            beingattacked = null;
            gameplaysetting.log = gameplaysetting.log + "moves to" + $"[{this.pos[0]}][{this.pos[1]}]" + "\n";
            Debug.Log("[SetmovedClientRpc] isSmhmoved set to FALSE (remote client triggered).");
        }
        else
        {
            Debug.Log("[SetmovedClientRpc] Ignored because sender matches local client.");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleClick();
    }
    void HandleClick()
    {
        if (currentlySelected == this && !isSpelling)
        {
            Deselect();
            currentlySelected = null;
        }
        else
        {
            try
            {
                if (piecer != null)
                {
                    if (withinrange)
                    {
                        if(!currentlySelected.isSpelling || currentlySelected.piecer.xtramove || !hasMoved)
                        {
                            StartCoroutine(ataction(true, true, false));
                        }
                    }
                }
                if (!hasMoved || currentlySelected.piecer.xtramove)
                {
                    if (currentlySelected != null &&
                        currentlySelected.piecer != null &&
                        currentlySelected.isSmhmoved &&
                        currentlySelected.piecer.color == gameplaysetting.players[0].pColor &&
                        boardgenerateor.GetComponent<BoardGenerator>().turn == 0)
                    {
                        if(piecer == null)
                        {
                            if ((Math.Abs(currentlySelected.pos[0] - pos[0]) == 1 && currentlySelected.pos[1] == pos[1]) ||
                            (Math.Abs(currentlySelected.pos[1] - pos[1]) == 1 && currentlySelected.pos[0] == pos[0]))
                            {
                                StartCoroutine(ataction(true, false, false));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while processing movement: {ex.Message}\n{ex.StackTrace}");
            }
            if (isSmhmoved)
            {
                if (piecer != null)
                {
                    Highlight();
                    ShowInfoCard();
                }
                if (currentlySelected != null)
                {
                    if (currentlySelected.isSmhmoved)
                    {
                        if (currentlySelected.isSpelling)
                        {
                            if (withinrange)
                            {
                                StartCoroutine(ataction(false, false, true));
                            }
                        }
                        else if(piecer != null)
                            currentlySelected = this;
                    }
                }
                else
                {
                    currentlySelected = this;
                }
            }
        }

    }

    public IEnumerator ataction(bool move, bool attack, bool spell)
    {
        Tile temp = currentlySelected;
        beingattacked = this;
        yield return new WaitUntil(() => !objection);
        if (move)
        {
            if (beingattacked != null)
            {
                bool hasclip = false;
                if (currentlySelected.clip != null)
                {
                    hasclip = true;
                }
                MoveServerRpc(
                    NetworkManager.Singleton.LocalClientId,
                    temp.pos[0],
                    temp.pos[1],
                    beingattacked.pos[0],
                    beingattacked.pos[1],
                    hasclip,
                    attack
                );
            }
        }
        else if (spell)
        {
            if (beingattacked != null)
            {
                spellServerRpc(NetworkManager.Singleton.LocalClientId, currentlySelected.pos[0], currentlySelected.pos[1], beingattacked.pos[0], beingattacked.pos[1], piecer.effect.nature);
            }
            else
            {
                hasSpelled = true;
                isSpelling = false;
            }
        }
        yield return null;
    }

    public void react(string reaction, Sprite effectsprite)
    {
        animationruuning = StartCoroutine(PlayAnimationoverlay(reaction, effectsprite));
    }

    public IEnumerator PlayAnimationoverlay(string reaction, Sprite effectsprite)
    {
        Animator anim = null;
        try
        {
            anim = transform.Find("effect").GetComponent<Animator>();
        }
        catch (ArgumentNullException ex)
        {
            Debug.Log(ex);
            yield break;
        }
        transform.Find("effect").GetComponent<SpriteRenderer>().sprite = effectsprite;
        anim.Play(reaction, 0, 0f);
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
        transform.Find("effect").GetComponent<SpriteRenderer>().sprite = null;
    }

    public void runanim()
    {
        StartCoroutine(PlayAnimationThenIdle());
    }

    public IEnumerator PlayAnimationoverlay()
    {
        Animator animator = overlay.GetComponent<Animator>();
        animator.enabled = true;
        animator.Update(0);
        animator.Play("anchor", 0, 0f);
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator PlayAnimationThenIdle()
    {
        movingstandby.isSmhmoved = false;
        Animator anim = null;
        AnimationClip clip = null;
        try
        {
            anim = movingstandby.animator;
            clip = movingstandby.clip;
        }
        catch (ArgumentNullException ex)
        {
            Debug.Log(ex);
            yield break;
        }
        anim.applyRootMotion = false;
        anim.enabled = true;
        anim.Play(clip.name);
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        float animLength = clip.length / anim.speed;
        yield return new WaitForSeconds(Mathf.Max(0, animLength - 0.25f));
        animovin = true;
        yield return new WaitForSeconds(0.25f);
        anim.enabled = false;
        yield return new WaitForSeconds(0.2f);
        if(movingstandby.clip != null) movingstandby.clip = null;
        animovin = false;
    }

    void Highlight()
    {
        SpriteRenderer srr = transform.Find("border").GetComponent<SpriteRenderer>();
        Color c = srr.color;
        c.a = 1;
        srr.color = c;
    }

    void Deselect()
    {
        SpriteRenderer srr = transform.Find("border").GetComponent<SpriteRenderer>();
        Color c = srr.color;
        c.a = 0;
        srr.color = c;
        HideInfoCard();
    }

    public void ShowInfoCard()
    {
        if (infocardprefab == null)
        {
            Debug.LogWarning("No infocard prefab assigned!");
            return;
        }

        if (currentInfocard != null)
        {
            Destroy(currentInfocard);
        }

        Vector3 position = new Vector3(-5.936256f, 0.7979151f, 0);
        currentInfocard = Instantiate(infocardprefab);
        currentInfocard.transform.position = position;
        currentInfocard.transform.SetParent(null);
        currentInfocard.GetComponent<Actions>().piecer = piecer;
    }

    public void HideInfoCard()
    {
        if (currentInfocard != null)
        {
            Destroy(currentInfocard);
            currentInfocard = null;
        }
    }

    Color AdjustBrightness(Color color, float factor)
    {
        return new Color(
            Mathf.Clamp01(color.r * factor),
            Mathf.Clamp01(color.g * factor),
            Mathf.Clamp01(color.b * factor),
            color.a
        );
    }

    Color AdjustSaturation(Color color, float factor)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * factor);
        return Color.HSVToRGB(h, s, v);
    }

    [ClientRpc]
    public void ResetStatusClientRpc()
    {
        hasMoved = false;
        hasSpelled = false;
    }

    [ClientRpc]
    public void ResetTileSpellingClientRpc()
    {
        isSpelling = false;
        if (piecer != null) 
        {
            piecer.effect.end();
        }
    }

    public void setstatus(int oneformovezeroforspell, Boolean value)
    {
        if (oneformovezeroforspell == 0)
        {
            hasSpelled = value;
        }
        else if (oneformovezeroforspell == 1)
        {
            hasMoved = value;
        }
        else if (oneformovezeroforspell == 2)
        {
            isSpelling = value;
        }
    }

    public Boolean getsatus(int oneformovezeroforspell)
    {
        if (oneformovezeroforspell == 0)
        {
            return hasSpelled;
        }
        else if (oneformovezeroforspell == 1)
        {
            return hasMoved;
        }
        else
        {
            return isSpelling;
        }
    }

    public void setcurrentselected(Tile title)
    {
        currentlySelected = title;
    }

    public Tile getcurrentselected()
    {
        return currentlySelected;
    }

    public void resettarget()
    {
        movingstandby = null;
    }

    public void resetattacked()
    {
        beingattacked = null;
    }

    public Tile gettarget()
    {
        return movingstandby;
    }

    public void settarget(Tile attacked)
    {
        movingstandby = attacked;
    }

    public void setattacked(Tile attacked)
    {
        beingattacked = attacked;
    }

    public Tile getattacked()
    {
        return beingattacked;
    }

    public void ShowWinScreen()
    {
        CreateEndScreen("WIN", Color.green);
    }

    public void ShowLoseScreen()
    {
        CreateEndScreen("LOSE", Color.red);
    }

    void CreateEndScreen(string message, Color color)
    {
        // ----- Canvas -----
        GameObject canvasGO = new GameObject("EndScreenCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ----- Background Panel -----
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasGO.transform);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.5f); // transparent black

        RectTransform bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // ----- Big WIN / LOSE text -----
        GameObject textGO = new GameObject("ResultText");
        textGO.transform.SetParent(canvasGO.transform);

        Text text = textGO.AddComponent<Text>();
        text.text = message;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 120;

        RectTransform textRT = text.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0.5f, 0.6f);
        textRT.anchorMax = new Vector2(0.5f, 0.6f);
        textRT.anchoredPosition = Vector2.zero;
        textRT.sizeDelta = new Vector2(600, 200);

        // ----- Button (Return to Main Menu) -----
        GameObject buttonGO = new GameObject("MainMenuButton");
        buttonGO.transform.SetParent(canvasGO.transform);

        UnityEngine.UI.Button button = buttonGO.AddComponent<UnityEngine.UI.Button>();
        Image buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.9f);

        RectTransform buttonRT = button.GetComponent<RectTransform>();
        buttonRT.anchorMin = new Vector2(0.5f, 0.3f);
        buttonRT.anchorMax = new Vector2(0.5f, 0.3f);
        buttonRT.anchoredPosition = Vector2.zero;
        buttonRT.sizeDelta = new Vector2(300, 90);

        // Button label
        GameObject btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(buttonGO.transform);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "Return to Main Menu";
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        btnText.color = Color.black;
        btnText.fontSize = 40;

        RectTransform btnTextRT = btnText.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        // Add button functionality
        button.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
            ShutdownServerRpc();
        });
    }
    [ServerRpc(RequireOwnership = false)]
    public void ShutdownServerRpc()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server shutting down...");
            NetworkManager.Singleton.Shutdown();
        }
    }
}
