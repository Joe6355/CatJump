using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string key;
    private TMP_Text text;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        UpdateText();
    }

    public void UpdateText()
    {
        text.text = LanguageManager.Instance.GetText(key);
    }
}
