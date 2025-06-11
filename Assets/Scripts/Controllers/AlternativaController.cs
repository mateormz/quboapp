using UnityEngine;
using TMPro;

public class AlternativaController : MonoBehaviour
{
    public TextMeshPro textoAlternativa;
    private bool esCorrecta;
    private PreguntaManager manager;
    private int indiceAlternativa;

    public void Configurar(string texto, bool correcta, PreguntaManager m, int index)
    {
        textoAlternativa.text = texto;
        esCorrecta = correcta;
        manager = m;
        indiceAlternativa = index;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.Responder(esCorrecta, indiceAlternativa);
        }
    }
}
