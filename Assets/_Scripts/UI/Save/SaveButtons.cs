using UnityEngine;

public class SaveButtons : MonoBehaviour
{
    public void Continue()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.Continue();
    }

    public void NewGame()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.DeleteSave();
    }
}