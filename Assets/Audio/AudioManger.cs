using NUnit.Framework;
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


    //combat var
    [SerializeField] private AudioClip combatTrack;
    [SerializeField] private float ExitFade;
    private int aggro;
    private bool inCombat;


    //Sound Effect
    [SerializeField] private sfxAudio[] sfxTracks;
    [SerializeField] private float sfxVolume;
    //Track List
    [SerializeField] private sceneAudio[] sceneTracks;
    //Fade In/Out,Volume
    [SerializeField] private float transitionTime;
    [SerializeField] private float volume;
    //What plays the audio

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private AudioClip audioClip;

    private List<AudioSource> sfxList = new List<AudioSource>();
    

    private void Awake()
    {
        if (inst != null)
        {
            Destroy(gameObject);
            return;
        }
        inst = this;

        //Keep this object when entering new scene
        DontDestroyOnLoad(gameObject);

        //Scene Audio Sorce SetUp
        musicSource = GetComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        //Sound Effect Source
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        //Build SoundEffects
        for (int i = 0; i < sfxTracks.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            sfxList.Add(source);
            
        }


        SceneManager.sceneLoaded += SceneLoaded;
        SceneAudio(SceneManager.GetActiveScene().name);

    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
        if (inst == this)
        {
            inst = null;
        }
    }
    //called once a scene loads
    void SceneLoaded(Scene scene, LoadSceneMode mode) => SceneAudio(scene.name);

    //Find and Play correct track
    void SceneAudio(string scene)
    {
        foreach (var track in sceneTracks)
        {

            if (track.Name != scene)
            {
                continue;
            }

            audioClip = track.clip;

            //Dont restart already playing track
            if (musicSource.clip == track.clip && musicSource.isPlaying)
            {
                return;
            }
            //fade to next track
            StopAllCoroutines();
            StartCoroutine(FadeInOut(track.clip));
            return;
        }
    }

    void PlayTrack(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }
        StopAllCoroutines();
        StartCoroutine(FadeInOut(clip));
    }



    //Fade out/in method
    IEnumerator FadeInOut(AudioClip nextClip)
    {
        float time = Mathf.Max(transitionTime, 0.1f);

        //Fade out
        float startVolume = musicSource.volume;
        for (float i = 0f; i < time; i += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, i / time);
            yield return null;
        }
        musicSource.volume = 0f;

        //start new track
        musicSource.clip = nextClip;
        musicSource.Play();


        //Fade in
        for (float i = 0f; i < time; i += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, volume, i / time);
            yield return null;
        }
        musicSource.volume = volume;
    }




    public void CombatStarted()
    {
        aggro++;
        if (inCombat || combatTrack == null)
        {
            return;
        }
        inCombat = true;
        PlayCombatMusic(combatTrack);
    }
    public void CombatEnded()
    {
        aggro = Mathf.Max(0, aggro - 1);
        if (aggro > 0 || !inCombat)
        {
            return;
        }
        inCombat = false;
        PlayTrack(audioClip);
    }

    public void PlayCombatMusic(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }
        PlayTrack(clip);
    }

    //Sound Effects
    public void PlaySFX(string Name)
    {
        foreach (var sfx in sfxTracks)
        {
            if (sfx.Name != Name)
            {
                continue;
            }
            if (sfx.clip == null || sfx.clip.length == 0)
            {
                return;
            }
            sfxSource.pitch = sfx.randomPitch ? Random.Range(0.6f, 1.0f) : 1f;
            float finalVolume = (sfx.volume > 0f ? sfx.volume : 1f) * sfxVolume;
            sfxSource.PlayOneShot(sfx.clip, finalVolume);
            return;
        }
    }

    public void PlaySFX(AudioClip clip, float volume, bool randomPitch)
    {
        if (clip == null)
        {
            return;
        }
        AudioSource source = GetSFXSource();
        sfxSource.pitch = randomPitch ? Random.Range(0.6f, 1.0f) : 1f;
        float finalVolume = Mathf.Clamp01(volume) * sfxVolume;
        sfxSource.clip = clip;
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    private AudioSource GetSFXSource()
    {
        foreach (var source in sfxList)
        {
            if (source.isPlaying == false)
            {
                return source; 
            }
        }
        return sfxList[0];
    }


}
