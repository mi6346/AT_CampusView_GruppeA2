using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PositionImporter : MonoBehaviour
{
    const string POSITIONS_FILE = "positions.txt"; // file name
    const int NR_ENTRIES = 4; // number of entries per line in positions file 

    [System.Serializable]
    public class PositionEntry
    {
        public string id1;     // from position
        public string id2;     // to position
        public float rotate1;  // rotation in "from"
        public float rotate2;  // rotation in "to"

        public override string ToString()
        {
            return $"{id1} -> {id2}: rotate1={rotate1}, rotate2={rotate2}";
        }
    }

    // the list with all position entries
    public List<PositionEntry> entries = new List<PositionEntry>();


    void Start()
    {
        StartCoroutine(LoadPositions()); // load once, at start of app

        Debug.Log("Skript ist am laufen");
        
        // Optional: log to check

        foreach (var entry in entries)
        {
            Debug.Log(entry);
        }
    }

    // loads all positions from file "positions.txt" in the directory /Assets/StreamingAssets
    // into the list with position entries
    IEnumerator LoadPositions()
    {
        string path = Path.Combine(Application.streamingAssetsPath, POSITIONS_FILE);

        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            yield break;
        }

        string[] lines = File.ReadAllLines(path);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length != 4)
            {
                Debug.LogWarning("Invalid line format: " + line);
                continue;
            }

            PositionEntry entry = new PositionEntry
            {
                id1 = parts[0].Trim(),
                id2 = parts[1].Trim(),
                rotate1 = float.Parse(parts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture),
                rotate2 = float.Parse(parts[3].Trim(), System.Globalization.CultureInfo.InvariantCulture)
            };

            entries.Add(entry);
        }
    }


    // returns a list with position entries for one specific position
    public List<PositionEntry> GetEntriesFor(string fromID)
    {
        List<PositionEntry> result = new List<PositionEntry>();

        foreach (var entry in entries)
        {
            if (entry.id1 == fromID)
            {
                result.Add(entry);
            }
        }

        return result;
    }

}
