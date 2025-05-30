using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Transform target;
    float offsetY;

    /// <summary>
    /// Llamar justo después de instanciar al jugador para fijar el target y el offset
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        offsetY = transform.position.y - target.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 pos = transform.position;
        pos.y = target.position.y + offsetY;
        transform.position = pos;
    }
}