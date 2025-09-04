using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    private Dictionary<string, Dictionary<string, string>> languages = new Dictionary<string, Dictionary<string, string>>();
    private string currentLanguage = "ru";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            LoadLocalization();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadLocalization()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("localization"); // localization.json â Assets/Resources/
        languages = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(jsonFile.text);
    }

    public void SetLanguage(string lang)
    {
        currentLanguage = lang;
        foreach (var loc in FindObjectsOfType<LocalizedText>())
            loc.UpdateText();
    }

    public string GetText(string key)
    {
        if (languages.ContainsKey(currentLanguage) && languages[currentLanguage].ContainsKey(key))
            return languages[currentLanguage][key];
        else
            return key;
    }

    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }
}
