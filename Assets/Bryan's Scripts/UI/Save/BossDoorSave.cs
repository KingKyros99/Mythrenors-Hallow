using UnityEngine;

public class BossDoorSave : MonoBehaviour
{
    public string roomId = "bossdoor";
    bool done;

    void OnTriggerEnter(Collider other)
    {
        if (done || !other.CompareTag("Player")) return;
        done = true;
        if (SaveManager.Instance != null) SaveManager.Instance.Save(roomId);
    }
}