using System.Collections;
using UnityEngine;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("Выпадающие настройки")]
    public TMP_Dropdown dropdown_Controller;
    public TMP_Dropdown dropdown_Language;

    [Header("Мобильные элементы управления")]
    public GameObject[] mobileControlObjects;

    public PlayerController playerController;

    private void Start()
    {
        playerController = FindObjectOfType<PlayerController>();

        int controlIndex = GetDefaultControlIndex();
        dropdown_Controller.value = controlIndex;

        dropdown_Controller.onValueChanged.AddListener(OnControllerChanged);
        dropdown_Language.onValueChanged.AddListener(OnLanguageChanged);

        OnControllerChanged(dropdown_Controller.value);
        OnLanguageChanged(dropdown_Language.value);
    }

    private void OnControllerChanged(int value)
    {
        bool isMobile = (value == 1);

        foreach (var obj in mobileControlObjects)
            obj?.SetActive(isMobile);

        if (playerController != null)
            playerController.useMobileControls = isMobile;
    }

    private void OnLanguageChanged(int value)
    {
        string langCode = (value == 0) ? "ru" : "en";
        LanguageManager.Instance.SetLanguage(langCode);
    }

    private int GetDefaultControlIndex()
    {
#if UNITY_ANDROID || UNITY_IOS
        return 1; // Телефон
#else
        return 0; // ПК
#endif
    }
}
