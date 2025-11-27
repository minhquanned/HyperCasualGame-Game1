using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    public Camera targetCamera;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        // Hướng canvas nhìn về camera
        transform.LookAt(transform.position + targetCamera.transform.forward,
                         targetCamera.transform.up);
    }
}
