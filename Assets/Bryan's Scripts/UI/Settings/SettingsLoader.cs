using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


public class SettingsLoader : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private Volume brightVolume;
    [SerializeField] private Volume darkVolume;
    [SerializeField] private InputActionAsset inputActions;   
    [SerializeField] private bool applyScreenSettings = true;

    private void Start()
    {
        
        GameSettings.Values v = GameSettings.Load();
        GameSettings.LoadBindings(inputActions);
        GameSettings.ApplyAudio(mixer, v);
        if (applyScreenSettings)
            GameSettings.ApplyVideo(v, brightVolume, darkVolume);
        else
            GameSettings.ApplyBrightness(v.brightness, brightVolume, darkVolume);
    }
}
