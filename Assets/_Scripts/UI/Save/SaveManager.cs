using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    public string gameplaySceneName = "MainScene";
    public event Action Saved;

    private const string KeyHasGame = "mh.save.hasGame";

    private SaveData pending;

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        Instance = null;
    }

    private static CharacterBase FindPlayer()
    {
        GameObject[] tagged =
            GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject go in tagged)
        {
            CharacterBase player = go.GetComponent<CharacterBase>();

            if (player == null)
                player = go.GetComponentInParent<CharacterBase>();

            if (player == null)
                player = go.GetComponentInChildren<CharacterBase>();

            if (player != null)
                return player;
        }

        return null;
    }

    private static PlayerInteractions FindInteractions(
        CharacterBase player)
    {
        PlayerInteractions interactions =
            player.GetComponentInChildren<PlayerInteractions>(true);

        if (interactions == null)
        {
            interactions =
                player.GetComponentInParent<PlayerInteractions>();
        }

        return interactions;
    }

    private SaveData ReadFile()
    {
        if (!File.Exists(FilePath))
            return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(
                File.ReadAllText(FilePath)
            );
        }
        catch
        {
            return null;
        }
    }

    public bool HasSave()
    {
        SaveData data = ReadFile();

        return data != null &&
               !string.IsNullOrWhiteSpace(data.sceneName);
    }

    public void Save(string saveId, Transform spawnPoint = null)
    {
        CharacterBase player = FindPlayer();

        if (player == null)
            return;

        PlayerInteractions interactions = FindInteractions(player);

        if (interactions == null)
            return;

        string scene = SceneManager.GetActiveScene().name;

        SaveData old = ReadFile();

        SaveData data = new SaveData
        {
            sceneName = scene,
            lastSaveId = saveId,

            health = player.CurrentHealth,
            mana = player.CurrentMana,
            stamina = player.CurrentStamina,

            keys = new List<string>(interactions.Keys)
        };

        if (spawnPoint != null)
        {
            data.posX = spawnPoint.position.x;
            data.posY = spawnPoint.position.y;
            data.posZ = spawnPoint.position.z;
            data.rotY = spawnPoint.eulerAngles.y;

            data.isCheckpoint = true;
        }
        else if (old != null &&
                 old.isCheckpoint &&
                 old.sceneName == scene)
        {
            data.posX = old.posX;
            data.posY = old.posY;
            data.posZ = old.posZ;
            data.rotY = old.rotY;

            data.isCheckpoint = true;
        }
        else
        {
            data.posX = player.transform.position.x;
            data.posY = player.transform.position.y;
            data.posZ = player.transform.position.z;
            data.rotY = player.transform.eulerAngles.y;

            data.isCheckpoint = false;
        }

        try
        {
            File.WriteAllText(
                FilePath,
                JsonUtility.ToJson(data, true)
            );
        }
        catch
        {
            return;
        }

        PlayerPrefs.SetInt(KeyHasGame, 1);
        PlayerPrefs.Save();

        foreach (ElevatorMovement elevator in
                 FindObjectsByType<ElevatorMovement>(
                     FindObjectsInactive.Include))
        {
            elevator.SaveElevator();
        }

        Saved?.Invoke();
    }

    public bool PrepareContinue()
    {
        pending = ReadFile();

        if (pending == null ||
            string.IsNullOrWhiteSpace(pending.sceneName))
        {
            pending = null;
            return false;
        }

        return true;
    }

    public void Continue()
    {
        if (!PrepareContinue())
            return;

        Time.timeScale = 1f;

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void DeleteSave()
    {
        pending = null;

        if (File.Exists(FilePath))
            File.Delete(FilePath);

        foreach (string file in Directory.GetFiles(
                     Application.persistentDataPath,
                     "elevator_*.json"))
        {
            File.Delete(file);
        }

        PlayerPrefs.SetInt(KeyHasGame, 0);
        PlayerPrefs.Save();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pending == null || scene.name != pending.sceneName)
            return;

        CharacterBase player = FindPlayer();

        if (player == null)
            return;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        bool controllerWasEnabled =
            controller != null && controller.enabled;

        if (controllerWasEnabled)
            controller.enabled = false;

        player.transform.SetPositionAndRotation(
            new Vector3(
                pending.posX,
                pending.posY,
                pending.posZ
            ),
            Quaternion.Euler(0f, pending.rotY, 0f)
        );

        if (controllerWasEnabled)
            controller.enabled = true;

        PlayerInteractions interactions = FindInteractions(player);

        if (interactions != null)
        {
            interactions.Keys.Clear();

            if (pending.keys != null)
                interactions.Keys.AddRange(pending.keys);
        }

        foreach (ElevatorMovement elevator in
                 FindObjectsByType<ElevatorMovement>(
                     FindObjectsInactive.Include))
        {
            elevator.LoadElevator();
        }

        pending = null;
    }
}