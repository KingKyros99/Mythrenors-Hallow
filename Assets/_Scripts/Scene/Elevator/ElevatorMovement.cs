using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorMovement : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip elevatorEffect;
    [SerializeField] private AudioClip stopEffect;
    [SerializeField] private float volume;

    [Header("Movement")]
    [SerializeField] private float Cooldown = 5f;
    [SerializeField] private float Timer;

    public bool canMove;
    public float speed;

    [SerializeField] private int startPoint;
    [SerializeField] private Transform[] points;

    [Header("Saving")]
    [SerializeField] private string elevatorId;

    [SerializeField]
    private bool restoreToCheckpointFloor = true;

    [Serializable]
    private class ElevatorSaveDate
    {
        public Vector3 position;
        public int index;
        public bool moveBack;
        public bool canMove;
    }

    private string SaveFilePath =>
        Path.Combine(
            Application.persistentDataPath,
            $"elevator_{gameObject.scene.name}_{elevatorId}.json"
        );

    private string PlayerSaveFilePath =>
        Path.Combine(
            Application.persistentDataPath,
            "save.json"
        );

    private Rigidbody rb;
    private AudioSource source;

    private int index;
    private bool moveBack;
    private bool wasMoving;
    private bool restoredBeforeStart;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();

        if (source != null)
        {
            source.Stop();
            source.clip = elevatorEffect;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 25f;
            source.volume = volume;
        }
    }

    private void Start()
    {
        if (!HasValidPoints())
        {
            canMove = false;
            return;
        }

        if (!restoredBeforeStart)
        {
            bool hasSave =
                PlayerPrefs.GetInt("mh.save.hasGame", 0) == 1;

            if (hasSave)
                LoadElevator();

            if (!restoredBeforeStart)
            {
                int initialStop = Mathf.Clamp(
                    startPoint,
                    0,
                    points.Length - 1
                );

                PlaceAtStop(initialStop);
            }
        }

        MovementAudio();
    }

    private void FixedUpdate()
    {
        if (!HasValidPoints())
            return;

        if (Timer > 0f)
        {
            Timer = Mathf.Max(
                0f,
                Timer - Time.fixedDeltaTime
            );

            canMove = false;
            MovementAudio();
            return;
        }

        if (!canMove)
        {
            MovementAudio();
            return;
        }

        index = Mathf.Clamp(index, 0, points.Length - 1);

        Vector3 currentPosition = rb != null
            ? rb.position
            : transform.position;

        Vector3 targetPosition = points[index].position;

        Vector3 nextPosition = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            Mathf.Max(0f, speed) * Time.fixedDeltaTime
        );

        if (rb != null)
            rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        if ((nextPosition - targetPosition).sqrMagnitude <= 0.000001f)
        {
            int arrivedStop = index;

            canMove = false;
            Timer = Mathf.Max(0f, Cooldown);

            SetNextDestination(arrivedStop);
        }

        MovementAudio();
    }

    private bool HasValidPoints()
    {
        if (points == null || points.Length < 2)
            return false;

        foreach (Transform point in points)
        {
            if (point == null)
                return false;
        }

        return true;
    }

    private void SetNextDestination(int currentStop)
    {
        if (currentStop <= 0)
        {
            moveBack = false;
            index = 1;
        }
        else if (currentStop >= points.Length - 1)
        {
            moveBack = true;
            index = points.Length - 2;
        }
        else
        {
            index = moveBack
                ? currentStop - 1
                : currentStop + 1;
        }
    }

    private void PlaceAtStop(int stop)
    {
        stop = Mathf.Clamp(stop, 0, points.Length - 1);

        SetPosition(points[stop].position);

        canMove = false;
        Timer = 0f;

        SetNextDestination(stop);
        ResetMovementAudio();
    }

    private void SetPosition(Vector3 position)
    {
        transform.position = position;

        if (rb != null)
            rb.position = position;

        Physics.SyncTransforms();
    }

    private int FindClosestStopByHeight(float height)
    {
        int closestStop = 0;
        float closestDistance = float.PositiveInfinity;

        for (int i = 0; i < points.Length; i++)
        {
            float distance = Mathf.Abs(
                points[i].position.y - height
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestStop = i;
            }
        }

        return closestStop;
    }

    private SaveData ReadPlayerSave()
    {
        if (!File.Exists(PlayerSaveFilePath))
            return null;

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(
                File.ReadAllText(PlayerSaveFilePath)
            );

            if (data == null ||
                data.sceneName != gameObject.scene.name)
            {
                return null;
            }

            return data;
        }
        catch
        {
            return null;
        }
    }

    private void MovementAudio()
    {
        if (canMove && !wasMoving)
            StartElevatorSfx();
        else if (!canMove && wasMoving)
            StopElevatorSfx();

        wasMoving = canMove;
    }

    private void ResetMovementAudio()
    {
        if (source != null)
            source.Stop();

        wasMoving = false;
    }

    private void StartElevatorSfx()
    {
        if (elevatorEffect == null || source == null)
            return;

        source.clip = elevatorEffect;
        source.loop = true;
        source.volume = volume;

        float maxStartTime = Mathf.Min(
            4f,
            Mathf.Max(0f, elevatorEffect.length - 0.01f)
        );

        source.time = UnityEngine.Random.Range(0f, maxStartTime);
        source.pitch = UnityEngine.Random.Range(0.6f, 1f);

        if (!source.isPlaying)
            source.Play();
    }

    private void StopElevatorSfx()
    {
        if (source != null)
            source.Stop();

        if (stopEffect != null && AudioManger.inst != null)
        {
            AudioManger.inst.PlaySFX(
                stopEffect,
                volume,
                true,
                1f
            );
        }
    }

    public void SaveElevator()
    {
        ElevatorSaveDate save = new ElevatorSaveDate
        {
            position = rb != null
                ? rb.position
                : transform.position,

            index = index,
            moveBack = moveBack,
            canMove = canMove
        };

        try
        {
            File.WriteAllText(
                SaveFilePath,
                JsonUtility.ToJson(save, true)
            );
        }
        catch
        {
            return;
        }
    }

    public void LoadElevator()
    {
        if (!HasValidPoints())
            return;

        SaveData playerSave = ReadPlayerSave();

        if (restoreToCheckpointFloor &&
            playerSave != null &&
            playerSave.isCheckpoint)
        {
            int stop = FindClosestStopByHeight(playerSave.posY);

            PlaceAtStop(stop);
            restoredBeforeStart = true;
            return;
        }

        if (!File.Exists(SaveFilePath))
            return;

        ElevatorSaveDate data;

        try
        {
            data = JsonUtility.FromJson<ElevatorSaveDate>(
                File.ReadAllText(SaveFilePath)
            );
        }
        catch
        {
            return;
        }

        if (data == null)
            return;

        SetPosition(data.position);

        index = Mathf.Clamp(
            data.index,
            0,
            points.Length - 1
        );

        moveBack = data.moveBack;
        canMove = data.canMove;
        Timer = 0f;

        ResetMovementAudio();

        restoredBeforeStart = true;
    }

    private void OnDisable()
    {
        ResetMovementAudio();
    }
}