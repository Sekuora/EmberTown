using UnityEngine;

public class Faction : MonoBehaviour

{
    private string id;
    private int level;

    public int Level { get => level; set => level = value; }
    public string Id { get => id; set => id = value; }
}