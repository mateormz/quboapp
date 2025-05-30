using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Lily : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textoValor; // referencia al texto que muestra el valor
    public bool esCorrecto;            // marca si este nenúfar es la respuesta
    [HideInInspector] public LilyManager manager;

    // Asigna el texto
    public void SetValor(string nuevoValor)
    {
        if (textoValor != null)
            textoValor.text = nuevoValor;
    }

    // Este método se llama cuando el objeto es clickeado o tocado
    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
            manager.VerificarRespuesta(esCorrecto);
    }
}