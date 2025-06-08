using UnityEngine;
using TMPro;

public class AlternativaController : MonoBehaviour
{
    public TextMeshPro textoAlternativa;
    private bool esCorrecta;
    private PreguntaManager manager;

    public void Configurar(string texto, bool correcta, PreguntaManager m)
    {
        textoAlternativa.text = texto;
        esCorrecta = correcta;
        manager = m;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            manager.Responder(esCorrecta);
        }
    }
}
