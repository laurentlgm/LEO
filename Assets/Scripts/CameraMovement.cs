using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMovement : MonoBehaviour
{
    private Camera cam;
    private Vector3 previousPosition;

    // If Earth was not in (0,0,0) then:
    //private Transform target; 


    void Awake()
    {
        cam = Camera.main;
    }


    void LateUpdate()
    {
        if (Input.GetMouseButtonDown(0) )
        {
            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);

            // Could have created a target instead of new Vector3():
            cam.transform.position = new Vector3(); 
            cam.transform.Rotate(new Vector3(1, 0, 0), direction.y * 180);
            cam.transform.Rotate(new Vector3(0, 1, 0), -direction.x * 180, Space.World);
            // 2.2f is the start distance of the cam:
            cam.transform.Translate(new Vector3(0, 0, -2.2f)); 

            previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            cam.fieldOfView = cam.fieldOfView - 5;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            cam.fieldOfView= cam.fieldOfView + 5;
        }
    }
}
