using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 50f;

    void Update()
    {
        float horizontal = 0;

        if (Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1;

        if (Input.GetKey(KeyCode.RightArrow))
            horizontal = 1;

        transform.Rotate(0,
                         horizontal * rotationSpeed * Time.deltaTime,
                         0);
    }
}