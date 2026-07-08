using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkyBoxManager : MonoBehaviour
{
    // Attribute
    
    public Material skyboxMaterial;

    // Start-Position
    public string startPosition = "07";

    // Ermöglicht das manuelle Setzen der Panorama-Bilder
    public Texture startTexture;
    public Texture targetTexture;
    
    // Dauer des Fades
    public float fadeDuration = 2f;

    private Texture currentTexture;

    // Ermöglicht das Importieren des GameObjects "PositionImporter"
    public PositionImporter positionImporter;

    // Liste, die alle Positionen, die von der aktuellen Position erreichbar sind
    private List<PositionImporter.PositionEntry> currentTargets;

    // Funktion zum Anzeigen einer SkyBox
    public void SetSkybox(Texture texture)
    {
        currentTexture = texture;

        skyboxMaterial.SetTexture("_Tex1", texture);
        skyboxMaterial.SetTexture("_Tex2", texture);
        skyboxMaterial.SetFloat("_Blend", 0f);
    }

    /* private Texture2D LoadTexture(string positionId)
    {
        return Resources.Load<Texture2D>("Images/" + positionId);
    } */
    
    void Start()
    {
        // Texturen werden in den Attributen gespeichert
        skyboxMaterial.SetTexture("_Tex1", startTexture);
        skyboxMaterial.SetTexture("_Tex2", targetTexture);

        skyboxMaterial.SetFloat("_Blend", 0f);

        // Überprüfung, ob die Texturen gesetzt wurden bzw. of die Start-Methode ausgeführt wird.
        Debug.Log("Start- und Zieltextur gesetzt.");

        // Überprüfung, ob PositionImporter verbunden ist
        if (positionImporter != null)
        {
            Debug.Log("PositionImporter verbunden.");
        } else {
            Debug.LogError("PositionImporter wurde nicht zugewiesen");
        }

        // Debugging
        Debug.Log("Anzahl geladener Positionen: " + positionImporter.entries.Count);

        StartCoroutine(InitializePositionData());
    }

    void Update()
    { 
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(FadeRoutine());
        }
    }

    // Methode zur Erstellung eines flüssigen Übergangs
    // (Schrittweise Erhöhung des Blend-Werts)
    private IEnumerator FadeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float blend = Mathf.Clamp01(elapsed / fadeDuration);
            skyboxMaterial.SetFloat("_Blend", blend);
            yield return null;
        }

        skyboxMaterial.SetFloat("_Blend", 1f);

        // Die Zieltextur wird zur aktuellen Textur
        // Blend-Wert muss entsprechend auf 0 zurückgesetzt werden
        skyboxMaterial.SetTexture("_Tex1", targetTexture);
        skyboxMaterial.SetFloat("_Blend", 0f);

        startTexture = targetTexture;

        // Debugging
        Debug.Log("Neue Starttextur: " + startTexture.name);
    }

    private IEnumerator InitializePositionData()
    {
        // Einen Frame warten, damit der PositionImporter Zeit hat, seine Coroutine zu beenden.
        yield return null;

        currentTargets = positionImporter.GetEntriesFor(startPosition);

        Debug.Log("Erreichbare Zielpositionen:");

        foreach (var entry in currentTargets)
        {
            Debug.Log($"{entry.id1} -> {entry.id2}");
            Debug.Log($"{entry.id1} -> {entry.id2} | " + $"Rotation aktuell: {entry.rotate1} | " + $"Rotation Ziel: {entry.rotate2}");
        }
    }

    
}