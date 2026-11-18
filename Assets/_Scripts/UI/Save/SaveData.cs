using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string sceneName;
    public string lastSaveId;

    public float posX;
    public float posY;
    public float posZ;
    public float rotY;

    public bool isCheckpoint;

    public int health;
    public int mana;
    public float stamina;

    public List<string> keys = new List<string>();
}