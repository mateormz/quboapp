using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float verticalOffset = 2f;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 pos = transform.position;
        pos.y = target.position.y + verticalOffset;
        transform.position = pos;
    }
}
