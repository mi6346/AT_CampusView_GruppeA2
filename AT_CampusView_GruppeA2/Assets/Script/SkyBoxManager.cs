using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkyBoxManager : MonoBehaviour
{
    // Attribute
    public Material skyboxMaterial;
    
    public string startPosition = "07";                             // Start-Position
    private string currentPosition;                                 // Aktuelle Position
    
    public float fadeDuration = 2f;                                 // Dauer des Fades (in Sekunden)
    
    public PositionImporter positionImporter;                       // Ermöglicht das Importieren des GameObjects "PositionImporter"
    private List<PositionImporter.PositionEntry> currentTargets;    // Liste, die alle Positionen, die von der aktuellen Position erreichbar sind
    private string imagePath;
    
    private float currentRotation;
    private float targetRotation;

    private PositionImporter.PositionEntry selectedTarget;

    public Transform cameraTransform;                               // Speicherung des Kamerawinkels

    public Image recticle;

    public float hotspotTolerance = 3f;                             // Definition des (plus-minus) Toleranzbereichs

    void Start()
    {
        // Start-Überblendwert
        skyboxMaterial.SetFloat("_Blend", 0f);

        // Initialisierung der aktuellen Position zum Start
        currentPosition = startPosition;

        // Position laden
        StartCoroutine(InitializePositionData());

        recticle.enabled = false;
    }

    void Update()
    { 
        if (Keyboard.current.spaceKey.wasPressedThisFrame && selectedTarget != null && IsTargetVisible())
        {
            StartCoroutine(FadeRoutine());
        }

        // Überprüfung, welches Ziel ausgewählt werden soll
        UpdateSelectedTarget();
        
        // Debugging: Ausführen des Quellcode-Teils beim Drücken der T-Taste 
        // Wenn überhaupt ein Ziel existiert, kann das Recticle angezeigt werden.
        recticle.enabled = (selectedTarget != null);

        if (selectedTarget != null)
        {
            Debug.Log("Aktuelles Ziel: " + selectedTarget.id2);
        }

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("Kamera: " + cameraTransform.eulerAngles.y);
            Debug.Log("Ziel: " + currentRotation);
            Debug.Log("Differenz: " + Mathf.Abs(Mathf.DeltaAngle(cameraTransform.eulerAngles.y, currentRotation)));
        }
    }

    // Methode zur Erstellung eines flüssigen Übergangs (Schrittweise Erhöhung des Blend-Werts)
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

        // Überblendung abgeschlossen
        skyboxMaterial.SetFloat("_Blend", 1f);

        // Aktuelle Position wird zur Zielposition
        currentPosition = selectedTarget.id2;

        // Neue Position vollständig laden
        ShowPosition(currentPosition);

        // Debugging
        Debug.Log("Neue Position: " + currentPosition);
    }

    // Bild über angegebenen Dateipfad laden und als Texture2D zurückgeben
    private Texture2D LoadTextureFromFile(string path)
    {
        // Überprüfung: "Existiert die Datei überhaupt?"
        if (!File.Exists(path))
        {
            Debug.LogError("Bild nicht gefunden: " + path);
            return null;
        }

        // Auslese der Daten aus der Bild-Datei & Speicherung der Daten als Byte-Array
        // JPG-Dateien bestehen intern aus Bytes, die dann in eine Unity-Textur umgewandelt werden.
        byte[] imageData = File.ReadAllBytes(path);

        // Erstellung einer neuen leeren Texture2D
        Texture2D texture = new Texture2D(2, 2);

        // Konvertierung der Bild-Daten in eine verwendbare Unity-Textur
        if (texture.LoadImage(imageData))
        {
            // Rückgabe der fertigen Textur beim erfolgreichen Laden
            return texture;
        }
        
        // Fehlermeldung, wenn beim Umwandeln ein Fehler auftritt
        Debug.LogError("Bild konnte nicht geladen werden.");
        
        return null;
    }
    
    private void ShowPosition(string positionId)
    {
        currentPosition = positionId;

        currentTargets = positionImporter.GetEntriesFor(currentPosition);

        Debug.Log("Erreichbare Ziele: ");

        foreach (var entry in currentTargets)
        {
            Debug.Log($"{entry.id1} -> {entry.id2} | Rotation: {entry.rotate1}°");
        }
        
        // Überprüfung, ob Zielpositionen existiert
        if (currentTargets.Count == 0)
        {
            Debug.LogWarning("Keine Zielposition gefunden.");
            return;
        }

        selectedTarget = null;

        // Speicherung der Rotationswerte
        PositionImporter.PositionEntry nextEntry = currentTargets[0];

        currentRotation = nextEntry.rotate1;
        targetRotation = nextEntry.rotate2;

        // Übergabe der Rotationswerte an den Shader
        skyboxMaterial.SetFloat("_Rotation1", currentRotation);
        skyboxMaterial.SetFloat("_Rotation2", targetRotation);
        
        // Aktuelles Panorama laden

        // Erstellung des vollständigen Dateipfads zum Panoramas
        // (z.B. Assets/Images/07.jpg)
        imagePath = Path.Combine(Application.dataPath, "Images", currentPosition + ".jpg");
        
        // Laden des Panoramas (Methode s. oben) anhand des zuvor erzeugten Dateipfads
        Texture2D currentPanorama = LoadTextureFromFile(imagePath);
        
        // Überprüfung, ob das Panorama erfolgreich geladen wurde
        if (currentPanorama != null)
        {
            // Übergabe des geladenen Panoramas an den Shader als aktuelle SkyBox-Textur (_Tex1)
            skyboxMaterial.SetTexture("_Tex1", currentPanorama);
        }


        // Zielpanorama laden

        // Erstellung des vollständigen Dateipfads zum Panoramas 
        // (z.B. Assets/Images/10.jpg)
        string targetImagePath = Path.Combine(Application.dataPath, "Images", nextEntry.id2 + ".jpg");

        // Dateipfad des Ziel-Panoramas erzeugen
        Texture2D targetPanorama = LoadTextureFromFile(targetImagePath);

        if (targetPanorama != null)
        {
            skyboxMaterial.SetTexture("_Tex2", targetPanorama);
        }

        // Übergabe des geladenen Ziel-Panromas an den Shader an die SkyBox
        skyboxMaterial.SetFloat("_Blend", 0f);

        // Debugging: Ausgabe der aktuellen Position
        Debug.Log($"Position {currentPosition} geladen.");
    }
    
    // Hilfsmethode
    private bool IsTargetVisible()
    {
        float cameraAngle = Mathf.DeltaAngle(0f, cameraTransform.eulerAngles.y);
        
        return Mathf.Abs(cameraAngle) <= hotspotTolerance;
    }

    // Sucht die Zielposition, die momentan ausgewählt werden soll
    private void UpdateSelectedTarget()
    {
        // Zunächst kein Ziel ausgewählt
        selectedTarget = null;

        float cameraAngle = cameraTransform.eulerAngles.y;
        float smallestDifference = float.MaxValue;

        // Alle erreichbaren Zielpositionen durchlaufen
        foreach (var target in currentTargets)
        {
            // Winkel zwischen Kamera und Ziel berechnen
            float difference = Mathf.Abs(Mathf.DeltaAngle(cameraAngle, target.rotate1));

            // Das bisher beste Ziel merken
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                selectedTarget = target;
            }
        }

        if (smallestDifference > 5f)
        {
            selectedTarget = null;
        }
    }
    
    private IEnumerator InitializePositionData()
    {
        // Einen Frame warten, damit der PositionImporter Zeit hat, seine Coroutine zu beenden.
        yield return null;

        ShowPosition(currentPosition);
    }
}