using System;

[Serializable]
public class SaveData
{
    public string sceneName;
    public string lastSaveId;

    public float posX, posY, posZ;
    public float rotY;

    public bool isCheckpoint;

    public int health;
    public int mana;
    public float stamina;
}