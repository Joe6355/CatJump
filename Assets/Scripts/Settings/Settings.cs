using System.Collections;
using UnityEngine;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("���������� ���������")]
    public TMP_Dropdown dropdown_Controller;
    public TMP_Dropdown dropdown_Language;

    [Header("��������� �������� ����������")]
    public GameObject[] mobileControlObjects; // �������� ���������� ����������

    private void Start()
    {
        // ��������� ����������
        int controlIndex = GetDefaultControlIndex();
        dropdown_Controller.value = controlIndex;

        dropdown_Controller.onValueChanged.AddListener(OnControllerChanged);
        dropdown_Language.onValueChanged.AddListener(OnLanguageChanged);

        // ������������� ��������� ��� ������
        OnControllerChanged(dropdown_Controller.value);
        OnLanguageChanged(dropdown_Language.value);
    }

    // ���������� ��/�������
    private void OnControllerChanged(int value)
    {
        // 0 - ��, 1 - �������
        bool isMobile = (value == 1);

        foreach (var obj in mobileControlObjects)
            obj.SetActive(isMobile);
    }

    // ������������ �����
    private void OnLanguageChanged(int value)
    {
        string selectedLanguage = dropdown_Language.options[value].text;
        LanguageManager.Instance.SetLanguage(selectedLanguage);

        string langCode = (value == 0) ? "ru" : "en";
        LanguageManager.Instance.SetLanguage(langCode);
    }

    private int GetDefaultControlIndex()
    {
        // 0 - ��, 1 - �������
#if UNITY_ANDROID || UNITY_IOS
    return 1; // �������
#else
        return 0; // ��
#endif
    }
}
