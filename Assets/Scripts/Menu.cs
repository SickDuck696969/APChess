using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Canvas panel;
    public Button button;
    public GameObject actualpanel;
    public Camera cam;
    public Button resume;
    public Button mainmenu;
    public Button quitgame;
    public SceneNavigator sceneNavigator;

    public Player player;

    public Vector3 positionA;
    public Vector3 positionB;

    public AudioSource audioSource;
    public AudioSource sfx;
    public Gameplaysetting gameplaysetting;

    private bool atA = true;
    private bool isMoving = false;
    public float moveDuration = 0.35f;

    public Slider musicvolume;
    public TMP_Dropdown Playlist;

    public TMP_Text ipdisplay;
    public GameObject Fragmentsaccount;
    public GameObject Gemsaccount;

    public Animator fader;
    public GameObject logsandshit;
    public MatchDatabase matchdatabase;
    public BannedDatabase bannedDatabase;
    public SpriteData sprites;
    public IEnumerator AdjustDigi(int who, int toWhatValue)
    {
        // Move toward the value
        while (who != toWhatValue)
        {
            who = Mathf.RoundToInt(
                Mathf.Lerp(who, toWhatValue, Time.deltaTime * 10f)
            );

            yield return null;
        }
    }


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        matchdatabase = new MatchDatabase();
        bannedDatabase = new BannedDatabase();
        AssignCamera();
        logsandshit.transform.Find("Viewport/Panel").gameObject.SetActive(false);
        button.onClick.AddListener(TogglePanelPosition);
        resume.onClick.AddListener(TogglePanelPosition);
        mainmenu.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        });
        quitgame.onClick.AddListener (() =>
        {
            #if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
        musicvolume.onValueChanged.AddListener(SetVolume);
        Playlist.onValueChanged.AddListener(playmusic);
        if (PlayerPrefs.HasKey("volume")){
            musicvolume.value = PlayerPrefs.GetFloat("volume");
        }
        SetVolume(musicvolume.value);
        playmusic(0);
    }
    private void Update()
    {
        ipdisplay.text = player.ip;
        Fragmentsaccount.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = player.data.fragments.ToString();
        Gemsaccount.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = player.data.gems.ToString();
        if(player.data.profilePicBase64 != string.Empty)
        {
            foreach (avatar a in sprites.avatars) 
            {
                if(string.Compare(player.data.profilePicBase64, a.id.ToString()) == 0)
                {
                    player.ava = a.avatara; break;
                }
            }
        }
        if (SceneManager.GetActiveScene().name == "Game" || SceneManager.GetActiveScene().name == "Login")
        {
            var c = ipdisplay.color;
            c.a = 0f;
            ipdisplay.color = c;

            foreach (var r in Fragmentsaccount.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(1, 1, 1, 0);
            foreach (var r in Fragmentsaccount.GetComponentsInChildren<TMP_Text>())
            {
                c = r.color;
                c.a = 0f;
                r.color = c;
            }
            foreach (var r in Gemsaccount.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(1, 1, 1, 0);
            foreach (var r in Gemsaccount.GetComponentsInChildren<TMP_Text>())
            {
                c = r.color;
                c.a = 0f;
                r.color = c;
            }
            logsandshit.gameObject.SetActive(true);
            logsandshit.transform.Find("Viewport/Content/Text (TMP)").GetComponent<TMP_Text>().text = gameplaysetting.log;
            logsandshit.transform.Find("Button").GetComponent<Button>().onClick.AddListener(() =>
            {
                if (logsandshit.transform.Find("Viewport/Panel").gameObject.activeSelf)
                {
                    logsandshit.transform.Find("Viewport/Panel").gameObject.SetActive(false);
                }
                else
                {
                    logsandshit.transform.Find("Viewport/Panel").gameObject.SetActive(true);
                }
            });
        }
        else
        {
            var c = ipdisplay.color;
            c.a = 1f;
            ipdisplay.color = c;

            foreach (var r in Fragmentsaccount.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(1, 1, 1, 1);
            foreach (var r in Fragmentsaccount.GetComponentsInChildren<TMP_Text>())
            {
                c = r.color;
                c.a = 1f;
                r.color = c;
            }
            foreach (var r in Gemsaccount.GetComponentsInChildren<SpriteRenderer>())
                r.color = new Color(1, 1, 1, 1);
            foreach (var r in Gemsaccount.GetComponentsInChildren<TMP_Text>())
            {
                c = r.color;
                c.a = 1f;
                r.color = c;
            }
            logsandshit.gameObject.SetActive(false);
        }
    }
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnSceneChanged;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnSceneChanged;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log("About to leave scene: " + scene.name);
        fadeout();
    }

    void SetVolume(float value)
    {
        PlayerPrefs.SetFloat("volume", value);
        audioSource.volume = value;
    }

    void OnSceneChanged(UnityEngine.SceneManagement.Scene oldScene, UnityEngine.SceneManagement.Scene newScene)
    {
        if(oldScene.name == "Login" && player.data.virgin)
        {
            StartCoroutine(AdjustDigi(player.data.fragments, 1000000));
            StartCoroutine(AdjustDigi(player.data.gems, 1800));
        }
        gameplaysetting.playing = null;
        fadein();
        AssignCamera();
        sceneNavigator = cam.GetComponent<SceneNavigator>();
        System.Collections.Generic.List<AudioClip> playlist = new List<AudioClip>();
        if (SceneManager.GetActiveScene().name == "Shop")
        {
            playlist = gameplaysetting.ShopPlayList;
        }
        else if (SceneManager.GetActiveScene().name == "Game")
        {
            playlist = gameplaysetting.MatchPlayList;
        }
        else if (SceneManager.GetActiveScene().name == "Lobby")
        {
            playlist = gameplaysetting.LobbyPlayList;
        }
        Playlist.ClearOptions();
        foreach (AudioClip a in playlist)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = a.name;
            Playlist.options.Add(option);
            Playlist.RefreshShownValue();
        }
        playmusic(0);
    }

    public void playmusic(int index)
    {
        System.Collections.Generic.List<AudioClip> playlist = new List<AudioClip>();
        if (SceneManager.GetActiveScene().name == "Shop")
        {
            playlist = gameplaysetting.ShopPlayList;
        }
        else if (SceneManager.GetActiveScene().name == "Game")
        {
            playlist = gameplaysetting.MatchPlayList;
        }
        else if (SceneManager.GetActiveScene().name == "Lobby")
        {
            playlist = gameplaysetting.LobbyPlayList;
        }
        if (playlist.Count > 0)
        {
            audioSource.clip = playlist[index];
        }
        audioSource.Play();
    }

    void AssignCamera()
    {
        cam = Camera.main;
        if (cam != null)
        {
            panel.worldCamera = cam;
        }
        sceneNavigator = cam.GetComponent<SceneNavigator>();
    }

    void TogglePanelPosition()
    {
        if (!isMoving)
        {
            Vector3 target = atA ? positionB : positionA;
            StartCoroutine(MovePanel(target));
            atA = !atA;
        }
    }

    IEnumerator MovePanel(Vector3 target)
    {
        isMoving = true;

        RectTransform rt = actualpanel.GetComponent<RectTransform>();
        Vector3 start = rt.anchoredPosition;
        float time = 0f;

        while (time < moveDuration)
        {
            float t = time / moveDuration;
            rt.anchoredPosition = Vector3.Lerp(start, target, t);
            time += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = target;
        isMoving = false;
    }

    public void fadein()
    {
        fader.Play("fade");
    }

    public void fadeout()
    {
        fader.Play("fadeout");
    }
}
