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
        Debug.Log("[Save] SaveManager ready. File: " + FilePath);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    static CharacterBase FindPlayer()
    {
        GameObject[] tagged = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject go in tagged)
        {
            CharacterBase cb = go.GetComponent<CharacterBase>();
            if (cb == null) cb = go.GetComponentInParent<CharacterBase>();
            if (cb == null) cb = go.GetComponentInChildren<CharacterBase>();
            if (cb != null) return cb;
        }
        return null;
    }

    SaveData ReadFile()
    {
        if (!File.Exists(FilePath)) return null;
        try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath)); }
        catch { return null; }
    }

    // spawnPoint: pass a Transform to save a checkpoint at that spot.
    // Leave it empty to save where the player is standing,
    // unless a checkpoint already exists in this scene, then keep the checkpoint spot.
    public void Save(string saveId, Transform spawnPoint = null)
    {
        Debug.Log("[Save] Save called: " + saveId);

        CharacterBase p = FindPlayer();
        if (p == null)
        {
            Debug.LogWarning("[Save] No object with tag Player and CharacterBase found.");
            return;
        }

        string scene = SceneManager.GetActiveScene().name;
        SaveData old = ReadFile();

        SaveData d = new SaveData
        {
            sceneName = scene,
            lastSaveId = saveId,
            health = p.CurrentHealth,
            mana = p.CurrentMana,
            stamina = p.CurrentStamina
        };

        if (spawnPoint != null)
        {
            d.posX = spawnPoint.position.x;
            d.posY = spawnPoint.position.y;
            d.posZ = spawnPoint.position.z;
            d.rotY = spawnPoint.eulerAngles.y;
            d.isCheckpoint = true;
            Debug.Log("[Save] Checkpoint saved at " + spawnPoint.name);
        }
        else if (old != null && old.isCheckpoint && old.sceneName == scene)
        {
            d.posX = old.posX;
            d.posY = old.posY;
            d.posZ = old.posZ;
            d.rotY = old.rotY;
            d.isCheckpoint = true;
            Debug.Log("[Save] Kept checkpoint spot from: " + old.lastSaveId);
        }
        else
        {
            d.posX = p.transform.position.x;
            d.posY = p.transform.position.y;
            d.posZ = p.transform.position.z;
            d.rotY = p.transform.eulerAngles.y;
            d.isCheckpoint = false;
        }

        try
        {
            File.WriteAllText(FilePath, JsonUtility.ToJson(d, true));
        }
        catch (Exception e)
        {
            Debug.LogError("[Save] Could not write file: " + e.Message);
            return;
        }

        PlayerPrefs.SetInt(KeyHasGame, 1);
        PlayerPrefs.Save();
        Debug.Log("[Save] Saved to " + FilePath);
        Saved?.Invoke();
    }

    public void Continue()
    {
        pending = ReadFile();
        if (pending == null)
        {
            Debug.LogWarning("[Save] Continue pressed but no save file at " + FilePath);
            return;
        }
        Debug.Log("[Save] Continue: loading " + gameplaySceneName);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void DeleteSave()
    {
        pending = null;
        if (File.Exists(FilePath)) File.Delete(FilePath);
        PlayerPrefs.SetInt(KeyHasGame, 0);
        PlayerPrefs.Save();
        Debug.Log("[Save] Save deleted (New Game)");
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

        Debug.Log("[Save] Player moved to saved spot");
        pending = null;
    }
}