// Debugging, Notizen, etc.

// SkyBoxManager.cs
// Start ()

// Überprüfung, ob die Texturen gesetzt wurden bzw. of die Start-Methode ausgeführt wird.
        /* Debug.Log("Start-Textur: " + startTexture);
        Debug.Log("Target-Textur: " + targetTexture); */

// Überprüfung, ob PositionImporter verbunden ist
        /* if (positionImporter != null)
        {
            Debug.Log("PositionImporter verbunden.");
        } else {
            Debug.LogError("PositionImporter wurde nicht zugewiesen");
        } */

        // Debugging
        // Debug.Log("Anzahl geladener Positionen: " + positionImporter.entries.Count);

/*
        // Anfangs existiert noch keine kleinste Differenz
        float smallestDifference = float.MaxValue;
        
        // Zunächst kein Ziel ausgewählt
        selectedTarget = null;

        float cameraAngle = cameraTransform.eulerAngles.y;

        // Alle erreichbaren Zielpositionen durchlaufen
        foreach (var target in currentTargets)
        {
            // Berechnung der Winkelabweichung zwischen Kamera und Ziel
            float difference = Mathf.Abs(
                Mathf.DeltaAngle(target.rotate1, cameraTransform.eulerAngles.y)
            );

            // Falls dieses Ziel näher an der Blickrichtung liegt, wird es als neues Ziel gespeichert.
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                selectedTarget = target;
            }
        }

        if (smallestDifference > hotspotTolerance)
        {
            selectedTarget = null;
        }
*/