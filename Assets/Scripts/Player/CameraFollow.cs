using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform player;

    [Header("Camera Angle / Offset")]
    public Vector3 offset = new Vector3(0, 3, -10); 

    void LateUpdate()
    {
        if (player != null)
        {
            // We only want the camera to follow the left/right movement (X axis).
            // We lock the Y (height) and Z (depth) so the 2.5D illusion doesn't break!
            transform.position = new Vector3(player.position.x + offset.x, offset.y, offset.z);
        }
    }
}