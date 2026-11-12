using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] int sen;
    [SerializeField] int vertMin;
    [SerializeField] int vertMax;

    float rotationX;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sen;
        float mouseY = Input.GetAxis("Mouse Y") * sen;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, vertMin, vertMax);
        transform.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
