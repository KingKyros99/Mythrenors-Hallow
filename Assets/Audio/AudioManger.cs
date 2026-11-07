using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(AudioSource))]
public class AudioManger : MonoBehaviour
{
    public static AudioManger inst { get; private set; }
    [System.Serializable] public struct sceneAudio { 
        public string Name; 
        public AudioClip clip; 
    }

    //Track List
    [SerializeField] private sceneAudio[] Tracks;
    //Fade In/Out,Volume
    [SerializeField] private float transitionTime;
    [SerializeField] private float volume;
    //What plays the audio
    private AudioSource source;


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
        //Audio Sorce SetUp
        source = GetComponent<AudioSource>();
        source.loop = true;
        source.playOnAwake = false;

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
        foreach (var track in Tracks)
        {

            if (track.Name != scene)
            {
                continue;
            }

            //Dont restart already playing track
            if (source.clip == track.clip && source.isPlaying)
            {
                return;
            }
            //fade to next track
            StopAllCoroutines();
            StartCoroutine(FadeInOut(track.clip));
            return;
        }
    }


    //Fade out/in method
    IEnumerator FadeInOut(AudioClip nextClip)
    {
        float time = Mathf.Max(transitionTime, 0.1f);

        //Fade out
        float startVolume = source.volume;
        for (float i = 0f; i < time; i += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, i / time);
            yield return null;
        }
        source.volume = 0f;

        //start new track
        source.clip = nextClip;
        source.Play();


        //Fade in
        for (float i = 0f; i < time; i += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(0f, volume, i / time);
            yield return null;
        }
        source.volume = volume;
    }
}
