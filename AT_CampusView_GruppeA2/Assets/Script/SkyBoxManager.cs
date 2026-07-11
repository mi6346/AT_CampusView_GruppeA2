using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SkyBoxManager : MonoBehaviour
{
    // Attribute
    
    // ==============================
    // Skybox
    // ==============================

    // Material mit dem eigenen Skybox-Shader
    public Material skyboxMaterial;

    // Dauer der Überblendung zwischen zwei Panoramen
    public float fadeDuration = 2f;

    // Angabe, ob momentan bereits eine Überblendung läuft
    private bool isFading = false;

    
    // ==============================
    // Positionen
    // ==============================
    
    // Start-Position aus der positions.txt
    public string startPosition = "07";
    
    // Position, an der sich der Benutzer momentan befindet
    private string currentPosition;

    // Alle von der aktuellen Position erreichbaren Ziele
    private List<PositionImporter.PositionEntry> currentTargets;
    
    // Momentan ausgewähltes Ziel
    private PositionImporter.PositionEntry selectedTarget;
    
    // Importer für die positions.txt
    public PositionImporter positionImporter;

    //
    private PositionImporter.PositionEntry previousTarget;

    //
    private Texture2D targetPanorama;
    
    
    // ==============================
    // Bilder
    // ==============================

    // Dateipfad des aktuell geladenen Panoramas
    private string imagePath;


    // ==============================
    // Kamera
    // ==============================
    
    // Referenz auf die Main Camera
    public Transform cameraTransform;


    // Maximale Abweichung (± Grad), damit ein Ziel als getroffen gilt
    public float hotspotTolerance = 3f;
    

    // ==============================
    // UI
    // ==============================
    
    // Fadenkreuz in der Bildschirm-Mitte
    public Image recticle;


    // ==============================
    // UI
    // ==============================

    // AudioSource-Komponente am selben GameObject für Loop-Sound pro Position
    private AudioSource audioSource;

    
    // Methoden
    
    void Start()
    {
        // Start-Überblendwert: Zu Beginn wird keine Überblendung angezeigt.
        skyboxMaterial.SetFloat("_Blend", 0f);

        // Initialisierung der Start-Position: Die Anwendung startet an der festgelegten Position.
        currentPosition = startPosition;

        // Der Recticle soll zunächst unsichtbar sein.
        recticle.enabled = false;

        // AudioSource-Komponente am gleichen GameObject holen
        audioSource = GetComponent<AudioSource>();
        
        // Position laden: Erst nachdem die Daten geladen werden, wird die erste Position angezeigt.
        StartCoroutine(InitializePositionData());
    }

    void Update()
    { 
        // -----------------------------
        // Zielposition bestimmen
        // -----------------------------
        
        UpdateSelectedTarget();
        
        // -----------------------------
        // Recticle anzeigen
        // -----------------------------

        recticle.enabled = (selectedTarget != null);

        // -----------------------------
        // Übergang starten
        // -----------------------------

        if (Keyboard.current.upArrowKey.wasPressedThisFrame && 
            selectedTarget != null &&
            !isFading
            )
        {
            StartCoroutine(FadeRoutine());
        }
    }

    
    // ==============================
    // Methode: Ausführung einer Überblendung zwischen zwei Panoramen
    // ==============================

    private IEnumerator FadeRoutine()
    {
        // Verhindert mehrere gleichzeitige Überblendungen
        isFading = true;

        // Ziel vor der Schleife speichern: Während der ganzen überblendung
        // werden Start- und Zielwinkel gemerkt.
        PositionImporter.PositionEntry target = selectedTarget;

        // Kamerawinkel zum Zeitpunkt des Tastendrucks merken.
        float cameraAngleAtStart = cameraTransform.eulerAngles.y;
        
        // Berechnung für das Zielbild (_Tex2) berechnen
        float rotationOffset = Mathf.DeltaAngle(cameraAngleAtStart, target.rotate2);
        
        // Zielbild ausgleichen
        skyboxMaterial.SetFloat("_Rotation2", rotationOffset);

        // Quellbild bleibt unverändert
        skyboxMaterial.SetFloat("_Rotation1", 0f);
        
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float blend = Mathf.Clamp01(elapsed / fadeDuration);
            skyboxMaterial.SetFloat("_Blend", blend); // Bild-Überblendung wie bisher
            yield return null;
        }

        // Überblendung abgeschlossen
        skyboxMaterial.SetFloat("_Blend", 1f);

        // Neue Position übernehmen
        currentPosition = target.id2;

        // Kamera auf den Zielwinkel setzen
        cameraTransform.eulerAngles = new Vector3(
            cameraTransform.eulerAngles.x,
            target.rotate2,
            cameraTransform.eulerAngles.z
        );

        // Neue Position vollständig laden
        ShowPosition(currentPosition);

        // Kamera auf den Zielwinkel drehen
        cameraTransform.eulerAngles = new Vector3(
            cameraTransform.eulerAngles.x,
            target.rotate2,
            cameraTransform.eulerAngles.z
        );

        // Offsets zurücksetzen
        skyboxMaterial.SetFloat("_Rotation1", 0f);
        skyboxMaterial.SetFloat("_Rotation2", 0f);
        
        UpdateSelectedTarget();

        // Überblendung beendet
        isFading = false;

        // Debugging
        Debug.Log("Neue Position: " + currentPosition);
    }

    
    // ==============================
    // Methode: Bild über angegebenen Dateipfad laden und als Texture2D zurückgeben
    // ==============================

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

        // Erstellung einer leeren Unity-Textur
        // (Die tatsächlichen Bild-Daten werden anschließend geladen.)
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

    // ==============================
    // Methode: Lädt die zur Position gehörende Audiodatei und spielt sie im Loop ab
    // ==============================

    private IEnumerator LoadAndPlayAudio(string positionId)
    {
        // Pfad zur Audiodatei zusammensetzen (z.B. .../Audio/07.wav)
        string audioPath = Path.Combine(Application.dataPath, "Audio", positionId + ".wav");

        // Für UnityWebRequest wird ein "file://"-URL benötigt, kein normaler Pfad
        string url = "file://" + audioPath;

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV))
        {
            // Warten, bis die Datei geladen ist (asynchron, blockiert das Spiel nicht)
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                // Vorheriges Audio stoppen, bevor das neue gestartet wird
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.loop = true; // Soll dauerhaft im Loop laufen
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Audio nicht gefunden oder konnte nicht geladen werden: " + audioPath);
            }
        }
    }
    
    private void ShowPosition(string positionId)
    {
        currentPosition = positionId;

        // -----------------------------
        // Positionsdaten laden
        // -----------------------------
        
        currentTargets = positionImporter.GetEntriesFor(currentPosition);

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

        // -----------------------------
        // Aktuelles Panorama laden
        // -----------------------------

        selectedTarget = null;
        previousTarget = null;

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

        // -----------------------------
        // Skybox zurücksetzen
        // -----------------------------
        
        // Übergabe des geladenen Ziel-Panromas an den Shader an die SkyBox
        skyboxMaterial.SetFloat("_Blend", 0f);

        // NEU: Passendes Audio zur neuen Position laden und im Loop abspielen
        StartCoroutine(LoadAndPlayAudio(currentPosition));
        
        // Debugging: Ausgabe der aktuellen Position
        Debug.Log($"Position {currentPosition} geladen.");
    }


    // ==============================
    // Methode: Durchläuft alle erreichbaren Zielpositionen und bestimmt,
    // welche Zielposition moment der Blickrichtung der Kamera am nächsten liegt.
    // ==============================
    
    private void UpdateSelectedTarget()
    {
        selectedTarget = null;

        if (currentTargets == null || currentTargets.Count == 0)
        {
            return;
        }

        float smallestDifference = float.MaxValue;

        // Einmal berechnen statt in jeder Runde neu
        float cameraAngle = cameraTransform.eulerAngles.y;

        foreach (var target in currentTargets)
        {

            float difference = Mathf.Abs(
                Mathf.DeltaAngle(cameraAngle, target.rotate1)
            );

            // Das momentan beste Ziel merken
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                selectedTarget = target;
            }
        }

        // Liegt kein Übergang innerhalb der Toleranz, wird kein Ziel ausegwählt.
        if (smallestDifference > hotspotTolerance)
        {
            selectedTarget = null;
            return;
        }

        // -----------------------------
        // Zielbild vorbereiten.
        // -----------------------------

        string targetImagePath = Path.Combine(
            Application.dataPath, "Images", selectedTarget.id2 + ".jpg"
        );

        if (selectedTarget != previousTarget)
        {
            targetPanorama = LoadTextureFromFile(targetImagePath);
            skyboxMaterial.SetTexture("_Tex2", targetPanorama);
            previousTarget = selectedTarget;
        }
    }
    
    private IEnumerator InitializePositionData()
    {
        // Einen Frame warten, damit der PositionImporter Zeit hat, seine Coroutine zu beenden.
        yield return null;

        currentTargets = positionImporter.GetEntriesFor(currentPosition);

        previousTarget = null;
        
        if (currentTargets.Count > 0)
        {
            ShowPosition(currentPosition);
        }
    }
}