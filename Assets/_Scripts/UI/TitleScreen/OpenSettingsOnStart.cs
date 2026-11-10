using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OpenSettingsOnStart : MonoBehaviour
{
    public Button settingsButton;

    IEnumerator Start()
    {
        if (!PauseMenuUI.OpenSettingsOnLoad) yield break;
        PauseMenuUI.OpenSettingsOnLoad = false;
        yield return null;
        yield return null;
        settingsButton.onClick.Invoke();
    }
}