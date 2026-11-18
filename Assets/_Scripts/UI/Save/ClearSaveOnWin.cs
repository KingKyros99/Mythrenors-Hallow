using UnityEngine;

public class ClearSaveOnWin : MonoBehaviour
{
    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSave();
        }
    }
}