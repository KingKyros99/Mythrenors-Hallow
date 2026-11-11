using UnityEngine;

public class MusicVolumeHook : MonoBehaviour
{
    private static MusicVolumeHook inst;
    private AudioSource musicSource;
    private float baseVolume;
    private float lastWritten = -1f;

    private void Awake()
    {
        if (inst != null && inst != this)
        {
            Destroy(gameObject);
            return;
        }
        inst = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameSettings.ApplyAudio(null, GameSettings.Load());
    }

    private void LateUpdate()
    {
        if (musicSource == null)
        {
            if (AudioManger.inst == null) return;
            musicSource = AudioManger.inst.GetComponent<AudioSource>();
            lastWritten = -1f;
            if (musicSource == null) return;
        }

        // If AudioManger changed the volume (fade), remember its value as the base.
        float now = musicSource.volume;
        if (lastWritten < 0f || !Mathf.Approximately(now, lastWritten))
            baseVolume = now;

        float target = baseVolume * GameSettings.MusicLevel;
        musicSource.volume = target;
        lastWritten = target;
    }
}