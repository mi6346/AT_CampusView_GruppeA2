using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 50f;

    void Start()
    {
        // Platzhalter
    }
    
    void Update()
    {
        float horizontal = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
        {
            horizontal = -1f;
        }

        if (Keyboard.current.rightArrowKey.isPressed)
        {
            horizontal = 1f;
        }

        transform.Rotate(0, horizontal * rotationSpeed * Time.deltaTime, 0);

        // Debugging
        if (horizontal != 0)
        {
            Debug.Log("Kamera wird gedreht");
        }
    }
}