using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerDataDB;

public class adminscript : MonoBehaviour
{
    public BannedDatabase banned;
    public List<BannedDataDB> banlist = new List<BannedDataDB>();
    public GameObject headcountprefab;
    public GameObject reports;
    public PlayerDatabase players;
    public GameObject record;
    public GameObject rapsheet;
    public GameObject panel;
    public GameObject statement;
    public MatchDatabase matchbase;
    public GameObject logprefaB;
    public Button Casebutton;
    public Button factcheckBUtton;
    public TMP_Text ID;
    public TMP_Text Name;
    void Start()
    {
        players = new PlayerDatabase();
        banned = new BannedDatabase();
        matchbase = new MatchDatabase();
        StartCoroutine(refreshban());
    }

    public IEnumerator refreshban()
    {
        while (true)
        {
            banlist.Clear();
            _ = RefreshbanList();
            yield return new WaitForSeconds(3f);
        }
    }

    public async Task RefreshbanList()
    {
        foreach (GameObject o in GameObject.FindGameObjectsWithTag("mine"))
        {
            Destroy(o);
        }

        var newList = banned.GetAllBans();
        

        if (!BanListsEqual(newList, banlist))
        {
            banlist = newList;
            foreach (BannedDataDB a in banlist)
            {
                Debug.Log(a.Case);
            }
        }
        foreach (BannedDataDB a in banlist) 
        {
            if (a.status != "rejected")
            {
                if (a.pending)
                {
                    GameObject j = Instantiate(headcountprefab, reports.transform);
                    j.transform.Find("ban/datebanned").GetComponent<TMP_Text>().text = $"{a.date_since} for {a.duration_days.ToString()} days";
                    PlayerDataDB f = players.GetPlayer(a.user_id);
                    j.transform.Find("ban/reported").GetComponent<TMP_Text>().text = f.username;
                    j.transform.Find("ban (1)/case").GetComponent<TMP_Text>().text = a.Case;
                    j.transform.Find("ban (2)/banthisnigga/Text (TMP)").GetComponent<TMP_Text>().text = "Ban";
                    j.transform.Find("ban (2)/banthisnigga").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        banned.ApproveBan(a.user_id);
                        _ = RefreshbanList();
                    });
                    j.transform.Find("ban (2)/aintdoneshit/Text (TMP)").GetComponent<TMP_Text>().text = "Unban";
                    j.transform.Find("ban (2)/banthisnigga").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        banned.RejectBan(a.user_id);
                        _ = RefreshbanList();
                    });

                    j.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        f = players.GetPlayer(a.user_id);
                        ID.text = f.user_id;
                        Name.text = f.username;
                        foreach (GameObject o in GameObject.FindGameObjectsWithTag("lobby"))
                        {
                            Destroy(o);
                        }
                        foreach (GameObject o in GameObject.FindGameObjectsWithTag("timer"))
                        {
                            Destroy(o);
                        }
                        GameObject b = Instantiate(statement, panel.transform.Find("Viewport/Content"));
                        b.transform.Find("Name").GetComponent<TMP_Text>().text = f.username;
                        b.transform.Find("case").GetComponent<TMP_Text>().text = a.Case;
                        b.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = a.Statement;
                        Casebutton.onClick.AddListener(() =>
                        {
                            foreach (GameObject o in GameObject.FindGameObjectsWithTag("lobby"))
                            {
                                Destroy(o);
                            }
                            foreach (GameObject o in GameObject.FindGameObjectsWithTag("timer"))
                            {
                                Destroy(o);
                            }
                            GameObject b = Instantiate(statement, panel.transform.Find("Viewport/Content"));
                            b.transform.Find("case").GetComponent<TMP_Text>().text = a.Case;
                            b.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = a.Statement;
                        });
                        factcheckBUtton.onClick.AddListener(() =>
                        {
                            foreach (GameObject o in GameObject.FindGameObjectsWithTag("lobby"))
                            {
                                Destroy(o);
                            }
                            foreach (GameObject o in GameObject.FindGameObjectsWithTag("timer"))
                            {
                                Destroy(o);
                            }
                            GameObject b = Instantiate(logprefaB, panel.transform.Find("Viewport/Content"));
                            List<MatchDataDB> g = matchbase.GetMatchesByPlayer(a.user_id);
                            if (g.Count > 0)
                            {
                                b.transform.Find("Text (TMP)").GetComponent<TMP_Text>().text = g[0].raw_log;
                            }
                        });
                    });
                }
            }
            else
            {
                GameObject j = Instantiate(rapsheet, record.transform);
            }
        }
    }
    public bool BanListsEqual(List<BannedDataDB> a, List<BannedDataDB> b)
    {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].user_id != b[i].user_id)
            {
                return false;
            }
        }
        return true;
    }
}
