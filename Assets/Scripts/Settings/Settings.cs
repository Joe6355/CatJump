using System.Collections;
using UnityEngine;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("Выпадающие настройки")]
    public TMP_Dropdown dropdown_Controller;
    public TMP_Dropdown dropdown_Language;

    [Header("Мобильные элементы управления")]
    public GameObject[] mobileControlObjects; // элементы мобильного управления

    private void Start()
    {
        // Автовыбор управления
        int controlIndex = GetDefaultControlIndex();
        dropdown_Controller.value = controlIndex;

        dropdown_Controller.onValueChanged.AddListener(OnControllerChanged);
        dropdown_Language.onValueChanged.AddListener(OnLanguageChanged);

        // Инициализация состояния при старте
        OnControllerChanged(dropdown_Controller.value);
        OnLanguageChanged(dropdown_Language.value);
    }

    // Управление ПК/Телефон
    private void OnControllerChanged(int value)
    {
        // 0 - ПК, 1 - Телефон
        bool isMobile = (value == 1);

        foreach (var obj in mobileControlObjects)
            obj.SetActive(isMobile);
    }

    // Переключение языка
    private void OnLanguageChanged(int value)
    {
        string selectedLanguage = dropdown_Language.options[value].text;
        LanguageManager.Instance.SetLanguage(selectedLanguage);

        string langCode = (value == 0) ? "ru" : "en";
        LanguageManager.Instance.SetLanguage(langCode);
    }

    private int GetDefaultControlIndex()
    {
        // 0 - ПК, 1 - Телефон
#if UNITY_ANDROID || UNITY_IOS
    return 1; // Телефон
#else
        return 0; // ПК
#endif
    }
}
