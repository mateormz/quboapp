using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Lily : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI textoValor;   // referencia al texto
    [HideInInspector] public bool esCorrecto;
    [HideInInspector] public LilyManager manager;

    public void SetValor(string nuevoValor)
    {
        if (textoValor != null)
            textoValor.text = nuevoValor;
    }

    // Este método ahora llama al manager pasándole esta instancia
    public void OnPointerClick(PointerEventData eventData)
    {
        if (manager != null)
            manager.OnLilyClicked(this);
    }
}