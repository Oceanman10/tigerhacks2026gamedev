using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour
{
    public Camera mainCamera;
    public float distanceFromCamera = 5f;
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.forward * GameSettings.Instance.ballVelocity * Time.deltaTime;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        float clampedX = Mathf.Clamp(mousePos.x, 50f, Screen.width - 50f);
        float clampedY = Mathf.Clamp(mousePos.y, 50f, Screen.height - 50f);

        Vector3 screenPoint = new Vector3(clampedX, clampedY, distanceFromCamera);
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPoint);

        transform.position = new Vector3(worldPoint.x, worldPoint.y, transform.position.z);
    }
}
