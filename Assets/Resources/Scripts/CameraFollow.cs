using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("El Transform del jugador al que la cámara seguirá")]
    public Transform target;

    [Tooltip("Desfase vertical (en unidades de mundo) entre la cámara y el jugador")]
    public float verticalOffset = 2f;

    void LateUpdate()
    {
        if (target == null) return;

        // Conservamos la posición X y Z de la cámara, y actualizamos solo Y
        Vector3 pos = transform.position;
        pos.y = target.position.y + verticalOffset;
        transform.position = pos;
    }
}