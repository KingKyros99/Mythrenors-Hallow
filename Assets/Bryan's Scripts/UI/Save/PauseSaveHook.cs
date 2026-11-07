using UnityEngine;

public class PauseSaveHook : MonoBehaviour
{
    public void SaveNow()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save("quit");
    }
}
