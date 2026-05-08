using UnityEngine;

[System.Serializable]
public class NoteData
{
    // values of the note store in an empty c# class
    public float time;
    public KeyCode key;
    public bool hit;
    public float duration;   // 0 = tap, >0 = hold
    public bool isBeingHeld; // true while player holds the key
}