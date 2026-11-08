using UnityEngine;

public class PauseSaveHook : MonoBehaviour
{
    public void SaveNow()
    {
        Debug.Log("[Save] SaveNow called. Instance: " + (SaveManager.Instance != null));
        if (SaveManager.Instance != null)
            SaveManager.Instance.Save("quit");
    }
}