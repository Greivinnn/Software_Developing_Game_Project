using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    // game mechanic variables
    public float songTime = 0f;
    public int money = 0;
    public int multiplier = 1;
    public float baseNoteValue = 10f;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        songTime += Time.deltaTime;
    }

    public void HitNote()
    {
        multiplier++;
        money += (int)(baseNoteValue * multiplier);
        Debug.Log("HIT | Money: " + money + " | Mult: " + multiplier);
    }

    public void MissNote()
    {
        multiplier = 1;
        Debug.Log("MISS | Money: " + money + " | Mult: " + multiplier);
    }

}
