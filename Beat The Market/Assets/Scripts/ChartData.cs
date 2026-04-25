using System.Collections.Generic;

[System.Serializable]
public class ChartData
{
    public string songName;
    public string audioFile;
    public float bpm;
    public float offset;
    public List<ChartNote> notes;
}

[System.Serializable]
public class ChartNote
{
    public float time;
    public string key;
}