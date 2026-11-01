using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


public static class GameSettings
{
    
    public const string KeyMaster      = "mh.audio.master";
    public const string KeyMusic       = "mh.audio.music";
    public const string KeySfx         = "mh.audio.sfx";
    public const string KeyAmbience    = "mh.audio.ambience";
    public const string KeyResW        = "mh.video.resW";
    public const string KeyResH        = "mh.video.resH";
    public const string KeyFullscreen  = "mh.video.fullscreen";
    public const string KeyQuality     = "mh.video.quality";
    public const string KeyBrightness  = "mh.video.brightness";
    public const string KeyVsync       = "mh.video.vsync";
    public const string KeyMouseSens   = "mh.controls.mouseSens";
    public const string KeyInvertY     = "mh.controls.invertY";
    public const string KeyBindings    = "mh.controls.bindings";   
    public const string KeyCameraShake = "mh.game.cameraShake";
    public const string KeyEnemyBars   = "mh.game.enemyBars";
    public const string KeySubtitles   = "mh.game.subtitles";

    
    public const string MixerMaster   = "MasterVolume";
    public const string MixerMusic    = "MusicVolume";
    public const string MixerSfx      = "SFXVolume";
    public const string MixerAmbience = "AmbienceVolume";

    [System.Serializable]
    public struct Values
    {
        public float master, music, sfx, ambience;   
        public int resW, resH;
        public bool fullscreen;
        public int quality;
        public float brightness;                      
        public bool vsync;
        public float mouseSens;                       
        public bool invertY;
        public bool cameraShake, enemyBars, subtitles;
    }

    public static Values Load()
    {
        Values v;
        v.master      = PlayerPrefs.GetFloat(KeyMaster, 80f);
        v.music       = PlayerPrefs.GetFloat(KeyMusic, 60f);
        v.sfx         = PlayerPrefs.GetFloat(KeySfx, 75f);
        v.ambience    = PlayerPrefs.GetFloat(KeyAmbience, 70f);
        v.resW        = PlayerPrefs.GetInt(KeyResW, Screen.currentResolution.width);
        v.resH        = PlayerPrefs.GetInt(KeyResH, Screen.currentResolution.height);
        v.fullscreen  = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
        v.quality     = PlayerPrefs.GetInt(KeyQuality, QualitySettings.GetQualityLevel());
        v.brightness  = PlayerPrefs.GetFloat(KeyBrightness, 50f);
        v.vsync       = PlayerPrefs.GetInt(KeyVsync, 1) == 1;
        v.mouseSens   = PlayerPrefs.GetFloat(KeyMouseSens, 1f);
        v.invertY     = PlayerPrefs.GetInt(KeyInvertY, 0) == 1;
        v.cameraShake = PlayerPrefs.GetInt(KeyCameraShake, 1) == 1;
        v.enemyBars   = PlayerPrefs.GetInt(KeyEnemyBars, 1) == 1;
        v.subtitles   = PlayerPrefs.GetInt(KeySubtitles, 0) == 1;
        v.quality     = Mathf.Clamp(v.quality, 0, QualitySettings.names.Length - 1);
        return v;
    }

    public static void Save(Values v)
    {
        PlayerPrefs.SetFloat(KeyMaster, v.master);
        PlayerPrefs.SetFloat(KeyMusic, v.music);
        PlayerPrefs.SetFloat(KeySfx, v.sfx);
        PlayerPrefs.SetFloat(KeyAmbience, v.ambience);
        PlayerPrefs.SetInt(KeyResW, v.resW);
        PlayerPrefs.SetInt(KeyResH, v.resH);
        PlayerPrefs.SetInt(KeyFullscreen, v.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KeyQuality, v.quality);
        PlayerPrefs.SetFloat(KeyBrightness, v.brightness);
        PlayerPrefs.SetInt(KeyVsync, v.vsync ? 1 : 0);
        PlayerPrefs.SetFloat(KeyMouseSens, v.mouseSens);
        PlayerPrefs.SetInt(KeyInvertY, v.invertY ? 1 : 0);
        PlayerPrefs.SetInt(KeyCameraShake, v.cameraShake ? 1 : 0);
        PlayerPrefs.SetInt(KeyEnemyBars, v.enemyBars ? 1 : 0);
        PlayerPrefs.SetInt(KeySubtitles, v.subtitles ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void ApplyAudio(AudioMixer mixer, Values v)
    {
        if (mixer == null) return;
        mixer.SetFloat(MixerMaster, ToDecibels(v.master));
        mixer.SetFloat(MixerMusic, ToDecibels(v.music));
        mixer.SetFloat(MixerSfx, ToDecibels(v.sfx));
        mixer.SetFloat(MixerAmbience, ToDecibels(v.ambience));
    }

    public static void ApplyVideo(Values v, Volume brightVolume, Volume darkVolume)
    {
        // Quality first, because changing quality can reset VSync.
        if (QualitySettings.GetQualityLevel() != v.quality)
            QualitySettings.SetQualityLevel(v.quality, true);
        QualitySettings.vSyncCount = v.vsync ? 1 : 0;

        FullScreenMode mode = v.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        if (v.resW > 0 && v.resH > 0)
            Screen.SetResolution(v.resW, v.resH, mode);
        else
            Screen.fullScreenMode = mode;

        ApplyBrightness(v.brightness, brightVolume, darkVolume);
    }

    public static void ApplyBrightness(float brightness, Volume brightVolume, Volume darkVolume)
    {
        float t = (brightness - 50f) / 50f;
        if (brightVolume != null) brightVolume.weight = Mathf.Clamp01(t);
        if (darkVolume != null) darkVolume.weight = Mathf.Clamp01(-t);
    }

    
    public static void LoadBindings(InputActionAsset actions)
    {
        if (actions == null) return;
        string json = PlayerPrefs.GetString(KeyBindings, "");
        actions.RemoveAllBindingOverrides();
        if (!string.IsNullOrEmpty(json)) actions.LoadBindingOverridesFromJson(json);
    }

    public static void SaveBindings(InputActionAsset actions)
    {
        if (actions == null) return;
        PlayerPrefs.SetString(KeyBindings, actions.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }

    public static float ToDecibels(float value0to100)
    {
        return Mathf.Log10(Mathf.Max(value0to100 / 100f, 0.0001f)) * 20f;
    }
}
