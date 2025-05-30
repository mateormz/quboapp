using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    Transform target;
    float offsetY;

    // Para detectar cuándo soltar la cámara
    Transform stopFollowTarget;
    float stopFollowHeight;

    /// <summary>
    /// Empieza a seguir a este transform.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        offsetY = transform.position.y - target.position.y;
    }

    /// <summary>
    /// Una vez que este objeto (grassEnd) esté completamente visible,
    /// la cámara dejará de seguir.
    /// stopHeight = altura total del sprite (por ejemplo 1.5f).
    /// </summary>
    public void SetStopFollow(Transform grassEndTransform, float stopHeight)
    {
        stopFollowTarget = grassEndTransform;
        stopFollowHeight = stopHeight;
    }

    void LateUpdate()
    {
        var cam = GetComponent<Camera>();
        bool following = target != null;

        // 1) Si todavía seguimos al jugador, ajustamos la Y
        if (following)
        {
            Vector3 pos = transform.position;
            pos.y = target.position.y + offsetY;
            transform.position = pos;
        }

        // 2) Comprobamos si debemos soltar la cámara
        if (stopFollowTarget != null)
        {
            // Topo de la cámara en mundo:
            float camTopY = transform.position.y + cam.orthographicSize;
            // Topo del grassEnd en mundo:
            float grassEndTopY = stopFollowTarget.position.y + (stopFollowHeight / 2f);

            if (camTopY >= grassEndTopY)
            {
                // Soltamos la cámara: deja de seguir al jugador
                target = null;
                // y ya no necesitamos esta comprobación
                stopFollowTarget = null;
            }
        }
    }
}