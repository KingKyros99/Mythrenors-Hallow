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


    [SerializeField] private float Cooldown = 5;
    [SerializeField] private float Timer;
    //Elevator Fields
    [SerializeField] public bool canMove;
    [SerializeField] public float speed;
    [SerializeField] int startPoint;
    [SerializeField] Transform[] points;

    [SerializeField] private string elevatorId;
    [Serializable]
    private class ElevatorSaveDate
    {
        public Vector3 position;
        public int index;
        public bool moveBack;
        public bool canMove;
    }
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, $"elevator_{SceneManager.GetActiveScene().name}_{elevatorId}.json");

    private Rigidbody rb;
    private int index;
    private bool moveBack;

    private bool wasMoving;
    private AudioSource source;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        source.Stop();
        source.clip = elevatorEffect;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 25f;
        source.volume = volume;
    }


    private void OnEnable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved += SaveElevator;
        }
    }

    private void OnDisable()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved -= SaveElevator;
        }
    }

    void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.Saved -= SaveElevator;
            SaveManager.Instance.Saved += SaveElevator;

        }
        bool hasSave = PlayerPrefs.GetInt("mh.save.hasGame", 0) == 1;
        if (hasSave && File.Exists(SaveFilePath))
        {
            LoadElevator();
        }
        else
        {
            if (File.Exists(SaveFilePath))
            {
                File.Delete(SaveFilePath);
            }


            //set elevator to its start position
            Vector3 startPos = points[startPoint].position;
            transform.position = startPos;
            if (rb != null)
            {
                rb.position = startPos;
            }
            Physics.SyncTransforms();
            index = startPoint;
        }
        if (canMove)
        {
            StartElevatorSfx();
            wasMoving = true;
        }
        else
        {
            if (source != null)
            {
                source.Stop();
                wasMoving = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (Timer > 0f)
        {
            Timer -= Time.fixedDeltaTime;
            canMove = false;
            MovementAudio();
            return;
        }
        //check if elevator reaches its destination
        if (Vector3.Distance(transform.position, points[index].position) == 0f)
        {
            if (rb != null)
            {
                rb.MovePosition(points[index].position);
            }
            else
            {
                transform.position = points[index].position;
            }
            //stop the elevator and change index
            canMove = false;
            Timer = Cooldown;
            if (index == points.Length - 1)
            {
                moveBack = true;
                index--;
                MovementAudio();
                return;
            }
            else if (index == 0)
            {
                moveBack = false;
                index++;
                MovementAudio();
                return;
            }

            if (moveBack == true)
            {
                index--;
            }
            else
            {
                index++;
            }

        }

        MovementAudio();
        if (canMove == true)
        {
            //move elevator the destination point
            transform.position = Vector3.MoveTowards(transform.position, points[index].position, speed * Time.deltaTime);
        }
    }
    private void MovementAudio()
    {
        if (canMove && !wasMoving)
        {
            StartElevatorSfx();
        }
        else if (!canMove && wasMoving)
        {
            StopElevatorSfx();
        }
        wasMoving = canMove;
    }


    private void StartElevatorSfx()
    {
        if (elevatorEffect == null || source == null)
        {
            return;
        }
        source.clip = elevatorEffect;
        source.volume = volume;
        source.time = UnityEngine.Random.Range(0f, 4f);
        source.pitch = UnityEngine.Random.Range(0.6f, 1.0f);
        if (!source.isPlaying)
        {
            source.Play();
        }
    }
    private void StopElevatorSfx()
    {
        if (source != null && source.isPlaying)
        {
            source.loop = false;
            source.Stop();
        }
        if (stopEffect != null && AudioManger.inst != null)
        {
            AudioManger.inst.PlaySFX(stopEffect, volume, true, 1f);
        }
    }
    public void SaveElevator()
    {
        Vector3 pos = rb != null ? rb.position : transform.position;
        ElevatorSaveDate save = new ElevatorSaveDate
        {
            position = transform.position,
            index = index,
            moveBack = moveBack,
            canMove = canMove
        };

        File.WriteAllText(SaveFilePath, JsonUtility.ToJson(save, true));
    }
    public void LoadElevator()
    {
        if (!File.Exists(SaveFilePath))
        {
            return;
        }
        string file = File.ReadAllText(SaveFilePath);
        ElevatorSaveDate data = JsonUtility.FromJson<ElevatorSaveDate>(file);
        if (data != null)
        {
            transform.position = data.position;
            if (rb != null)
            {
                rb.position = data.position;
            }
            Physics.SyncTransforms();
            index = data.index;
            moveBack = data.moveBack;
            canMove = data.canMove;
        }


    }

}
