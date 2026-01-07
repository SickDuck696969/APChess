using UnityEngine;
using UnityEngine.UI;

public class Rule : MonoBehaviour
{
    public GameObject RulePanel;
    public void Open() { 
        RulePanel.SetActive(true);
    }
    public void Close()
    {
        RulePanel.SetActive(false);
    }  
}
