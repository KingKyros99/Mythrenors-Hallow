using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioManger : MonoBehaviour
{
    public static AudioManger inst { get; private set; }

    [System.Serializable]
    public struct sceneAudio
    {
        public string Name;
        public AudioClip clip;
    }

    [System.Serializable]
    public struct sfxAudio
    {
        public string Name;
        public AudioClip clip;
        public float volume;
        public bool randomPitch;
    }

    [SerializeField] private AudioClip combatTrack;
    [SerializeField] private float ExitFade;

    private int aggro;
    private bool inCombat;

    [SerializeField] private sfxAudio[] sfxTracks;
    [SerializeField] private float sfxVolume;

    [SerializeField] private sceneAudio[] sceneTracks;

    [SerializeField] private float transitionTime;
    [SerializeField] private float volume;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioClip audioClip;

    private float settingsSfxLevel = 1f;

    private readonly List<AudioSource> sfxList =
        new List<AudioSource>();

    private void Awake()
    {
        if (inst != null)
        {
            Destroy(gameObject);
            return;
        }

        inst = this;

        if (transform.parent != null)
            transform.SetParent(null, true);

        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        sfxSource = CreateSFXSource();

        int sourceCount = sfxTracks != null
            ? Mathf.Max(1, sfxTracks.Length)
            : 1;

        for (int i = 0; i < sourceCount; i++)
        {
            sfxList.Add(CreateSFXSource());
        }

        SetSfxLevel(GameSettings.SfxLevel);

        SceneManager.sceneLoaded += SceneLoaded;

        SceneAudio(SceneManager.GetActiveScene().name);
    }

    private AudioSource CreateSFXSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();

        source.loop = false;
        source.playOnAwake = false;
        source.volume = settingsSfxLevel;

        return source;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;

        if (inst == this)
            inst = null;
    }

    public void SetSfxLevel(float level)
    {
        settingsSfxLevel = Mathf.Clamp01(level);

        if (sfxSource != null)
            sfxSource.volume = settingsSfxLevel;

        foreach (AudioSource source in sfxList)
        {
            if (source != null)
                source.volume = settingsSfxLevel;
        }
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneAudio(scene.name);
    }

    private void SceneAudio(string scene)
    {
        if (sceneTracks == null)
            return;

        foreach (sceneAudio track in sceneTracks)
        {
            if (track.Name != scene)
                continue;

            audioClip = track.clip;

            if (musicSource.clip == track.clip &&
                musicSource.isPlaying)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(FadeInOut(track.clip));
            return;
        }
    }

    private void PlayTrack(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        StopAllCoroutines();
        StartCoroutine(FadeInOut(clip));
    }

    private IEnumerator FadeInOut(AudioClip nextClip)
    {
        float time = Mathf.Max(transitionTime, 0.1f);
        float startVolume = musicSource.volume;

        for (float i = 0f; i < time; i += Time.unscaledDeltaTime)
        {
            musicSource.volume =
                Mathf.Lerp(startVolume, 0f, i / time);

            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.clip = nextClip;
        musicSource.Play();

        for (float i = 0f; i < time; i += Time.unscaledDeltaTime)
        {
            musicSource.volume =
                Mathf.Lerp(0f, volume, i / time);

            yield return null;
        }

        musicSource.volume = volume;
    }

    public void CombatStarted()
    {
        aggro++;

        if (inCombat || combatTrack == null)
            return;

        inCombat = true;
        PlayCombatMusic(combatTrack);
    }

    public void CombatEnded()
    {
        aggro = Mathf.Max(0, aggro - 1);

        if (aggro > 0 || !inCombat)
            return;

        inCombat = false;
        PlayTrack(audioClip);
    }

    public void PlayCombatMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        PlayTrack(clip);
    }

    public void PlaySFX(string Name)
    {
        if (sfxTracks == null || sfxSource == null)
            return;

        foreach (sfxAudio sfx in sfxTracks)
        {
            if (sfx.Name != Name)
                continue;

            if (sfx.clip == null || sfx.clip.length == 0)
                return;

            sfxSource.pitch = sfx.randomPitch
                ? Random.Range(0.6f, 1.0f)
                : 1f;

            float finalVolume =
                (sfx.volume > 0f ? sfx.volume : 1f) *
                sfxVolume;

            sfxSource.PlayOneShot(sfx.clip, finalVolume);
            return;
        }
    }

    public void PlaySFX(
        AudioClip clip,
        float volume,
        bool randomPitch,
        float speed = 1f)
    {
        if (clip == null)
            return;

        AudioSource source = GetSFXSource();

        source.pitch = randomPitch
            ? speed * Random.Range(0.9f, 1.1f)
            : speed;

        float finalVolume =
            Mathf.Clamp01(volume) * sfxVolume;

        source.PlayOneShot(clip, finalVolume);
    }

    private AudioSource GetSFXSource()
    {
        foreach (AudioSource source in sfxList)
        {
            if (source != null && !source.isPlaying)
                return source;
        }

        AudioSource additionalSource = CreateSFXSource();
        sfxList.Add(additionalSource);

        return additionalSource;
    }
}
