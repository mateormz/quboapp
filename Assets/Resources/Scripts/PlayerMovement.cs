using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Tooltip("Duraci�n en segundos del salto")]
    public float jumpDuration = 0.6f;

    [Tooltip("Altura m�xima del arco de salto")]
    public float jumpHeight = 1f;

    private bool isMoving = false;

    /// <summary>
    /// Inicia un salto hacia target, invocando onComplete al llegar.
    /// </summary>
    public void MoveTo(Vector3 target, Action onComplete)
    {
        if (isMoving) return;
        StartCoroutine(JumpRoutine(target, onComplete));
    }

    private IEnumerator JumpRoutine(Vector3 target, Action onComplete)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;
            // interpolaci�n lineal + arco vertical
            Vector3 horizontal = Vector3.Lerp(start, target, t);
            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;
            transform.position = horizontal + Vector3.up * height;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isMoving = false;
        onComplete?.Invoke();
    }
}