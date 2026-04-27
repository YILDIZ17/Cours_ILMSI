using UnityEngine;

// Caméra qui suit le joueur (offset fixe).
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 5.2f, -8.5f);
    public float lookAtHeight = 1.15f;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = 62f;
        }

        transform.position = target.position + offset;
        transform.LookAt(target.position + Vector3.up * lookAtHeight, Vector3.up);
    }
}
