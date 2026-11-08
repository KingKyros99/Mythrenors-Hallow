using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public string gameplaySceneName = "MainScene";
    public event Action Saved;

    const string KeyHasGame = "mh.save.hasGame";
    SaveData pending;

    string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    static CharacterBase FindPlayer()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        return go != null ? go.GetComponent<CharacterBase>() : null;
    }

    public void Save(string saveId)
    {
        Debug.Log("[Save] Save called: " + saveId);

        CharacterBase p = FindPlayer();
        if (p == null)
        {
            Debug.LogWarning("[Save] No object with tag Player and CharacterBase found.");
            return;
        }

        SaveData d = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            lastSaveId = saveId,
            posX = p.transform.position.x,
            posY = p.transform.position.y,
            posZ = p.transform.position.z,
            rotY = p.transform.eulerAngles.y,
            health = p.CurrentHealth,
            mana = p.CurrentMana,
            stamina = p.CurrentStamina
        };

        File.WriteAllText(FilePath, JsonUtility.ToJson(d));
        PlayerPrefs.SetInt(KeyHasGame, 1);
        PlayerPrefs.Save();
        Debug.Log("[Save] Saved to " + FilePath);
        Saved?.Invoke();
    }

    public void Continue()
    {
        if (!File.Exists(FilePath)) return;
        pending = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void DeleteSave()
    {
        pending = null;
        if (File.Exists(FilePath)) File.Delete(FilePath);
        PlayerPrefs.SetInt(KeyHasGame, 0);
        PlayerPrefs.Save();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pending == null || scene.name != pending.sceneName) return;

        CharacterBase p = FindPlayer();
        if (p == null) return;

        CharacterController cc = p.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        p.transform.SetPositionAndRotation(
            new Vector3(pending.posX, pending.posY, pending.posZ),
            Quaternion.Euler(0f, pending.rotY, 0f));
        if (cc != null) cc.enabled = true;

        // I will uncomment the line below once SetStats is put into the CharacterBase
        // p.SetStats(pending.health, pending.mana, pending.stamina);

        pending = null;
    }
}