using TMPro;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public static Tooltip Instance;

    [SerializeField] private CanvasGroup group;
    [SerializeField] public TMP_Text text;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Hide();
    }


    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 5f; // distance from camera
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }


    public void Show(string message)
    {
        text.text = message;
        group.alpha = 1f;
    }

    public void Hide()
    {
        group.alpha = 0f;
    }
}