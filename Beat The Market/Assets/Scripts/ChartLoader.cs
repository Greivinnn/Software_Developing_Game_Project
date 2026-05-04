using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ChartLoader : MonoBehaviour
{
    public NoteManager noteManager;
    public string chartFileName = "song1.json"; // change per song

    IEnumerator Start()
    {
        string path = System.IO.Path.Combine(
            Application.streamingAssetsPath, chartFileName);

        UnityWebRequest req = UnityWebRequest.Get(path);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Chart load failed: " + req.error);
            yield break;
        }

        ChartData chart = JsonUtility.FromJson<ChartData>(req.downloadHandler.text);
        noteManager.chartMode = true;

        foreach (ChartNote n in chart.notes)
        {
            KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), n.key);
            float spawnAheadTime = 2f; // how many seconds before hit to spawn
            float delay = n.time - spawnAheadTime;

            if (delay > 0)
                yield return new WaitForSeconds(delay);

            noteManager.SpawnChartNote(key, n.time);
        }
    }
}