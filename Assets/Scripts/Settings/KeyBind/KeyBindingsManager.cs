using System.IO;
using UnityEngine;


//класс модель для сериализации
[System.Serializable]
public class KeyBindingsData
{
    public string Left = "A";
    public string Right = "D";
    public string Jump = "Space";
}

[System.Serializable]
public class GameSettingsData
{
    public KeyBindingsData KeyBindings = new KeyBindingsData();
}



//основной скрипт для работы с json
public class KeyBindingsManager : MonoBehaviour
{
    public static KeyBindingsManager Instance;

    public GameSettingsData Settings = new GameSettingsData();

    private string settingsPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
            settingsPath = Path.Combine(Application.persistentDataPath, "settings.json");
            LoadBinds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadBinds()
    {
        if (File.Exists(settingsPath))
        {
            string json = File.ReadAllText(settingsPath);
            Settings = JsonUtility.FromJson<GameSettingsData>(json);
        }
        else
        {
            // Если файла нет — создаём с дефолтными значениями
            SaveBinds();
        }
    }

    public void SaveBinds()
    {
        string json = JsonUtility.ToJson(Settings, true);
        File.WriteAllText(settingsPath, json);
    }

    // Получить KeyCode по действию
    public KeyCode GetBind(string action)
    {
        string code = "None";
        switch (action)
        {
            case "Left": code = Settings.KeyBindings.Left; break;
            case "Right": code = Settings.KeyBindings.Right; break;
            case "Jump": code = Settings.KeyBindings.Jump; break;
        }
        return (KeyCode)System.Enum.Parse(typeof(KeyCode), code);
    }

    // Поменять бинд и сразу сохранить в json
    public void SetBind(string action, KeyCode key)
    {
        switch (action)
        {
            case "Left": Settings.KeyBindings.Left = key.ToString(); break;
            case "Right": Settings.KeyBindings.Right = key.ToString(); break;
            case "Jump": Settings.KeyBindings.Jump = key.ToString(); break;
        }
        SaveBinds();
    }
}
